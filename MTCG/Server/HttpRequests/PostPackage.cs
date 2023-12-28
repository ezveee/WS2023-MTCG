using MTCG.Cards;
using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace MTCG.Server.HttpRequests
{
	internal class PostPackage : IHttpRequest
	{
		public string GetResponse(string request)
		{
			string jsonPayload = HttpRequestUtility.ExtractJsonPayload(request);

			if (jsonPayload == null)
			{
				return Text.Res_400_BadRequest;
			}

			List<Database.Schemas.Card>? package = JsonConvert.DeserializeObject<List<Database.Schemas.Card>>(jsonPayload);

			if (package == null || package.Count != 5)
			{
				return Text.Res_400_BadRequest;
			}

			string response;
			try
			{
				response = CreatePackage(package, HttpRequestUtility.ExtractBearerToken(request));
			}
			catch (InvalidOperationException)
			{
				return Text.Res_401_Unauthorized;
			}

			return response;
		}

		private static string CreatePackage(List<Database.Schemas.Card> package, string authToken)
		{
			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			if (DoCardsAlreadyExist(package, dbConnection))
			{
				dbConnection.Close();
				return Text.Res_PostPackage_409;
			}

			if (!IsAdmin(authToken))
			{
				dbConnection.Close();
				return Text.Res_PostPackage_403;
			}

			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				dbConnection.Close();
				return Text.Res_401_Unauthorized;
			}

			using var transaction = dbConnection.BeginTransaction();

			try
			{
				using (NpgsqlCommand command = new())
				{
					command.Connection = dbConnection;
					command.Transaction = transaction;

					command.CommandText = @"SELECT nextval('packageids')";
					int packageId = Convert.ToInt32(command.ExecuteScalar());

					command.CommandText = @"INSERT INTO packages (id, cardid) VALUES (@id, @cardid)";

					foreach (Database.Schemas.Card card in package)
					{
						command.Parameters.Clear();
						command.Parameters.AddWithValue("id", packageId);
						command.Parameters.AddWithValue("cardid", card.Id);
						command.ExecuteNonQuery();
					}

					InsertIntoCardsTable(package, command);
				}

				transaction.Commit();
				Console.WriteLine("Transaction committed successfully");
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
			}

			dbConnection.Close();
			return Text.Res_PostPackage_201;
		}

		private static bool DoCardsAlreadyExist(List<Database.Schemas.Card> package, NpgsqlConnection dbConnection)
		{
			using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM cards WHERE id IN (@id1, @id2, @id3, @id4, @id5);", dbConnection);

			command.Parameters.AddWithValue("id1", package[0].Id);
			command.Parameters.AddWithValue("id2", package[1].Id);
			command.Parameters.AddWithValue("id3", package[2].Id);
			command.Parameters.AddWithValue("id4", package[3].Id);
			command.Parameters.AddWithValue("id5", package[4].Id);

			int count = Convert.ToInt32(command.ExecuteScalar());

			return count > 0;
		}

		private static bool IsAdmin(string authToken)
		{
			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new($@"SELECT username FROM sessions WHERE token = '{authToken}';", dbConnection);
			string username = (string)command.ExecuteScalar();

			if (username != "admin")
			{
				dbConnection.Close();
				return false;
			}

			dbConnection.Close();
			return true;
		}

		private static void InsertIntoCardsTable(List<Database.Schemas.Card> package, NpgsqlCommand command)
		{
			foreach (var card in package)
			{
				command.CommandText = $@"INSERT INTO cards (id, name, cardtype_f, element_f, damage) VALUES (@id, @name, @cardtype, @element, @damage);";

				command.Parameters.Clear();
				command.Parameters.AddWithValue("id", card.Id);
				command.Parameters.AddWithValue("name", card.Name);
				command.Parameters.AddWithValue("cardtype", (int)Cards.Card.GetCardTypeByName(card.Name));
				command.Parameters.AddWithValue("element", (int)Cards.Card.GetElementTypeByName(card.Name));
				command.Parameters.AddWithValue("damage", card.Damage);

				command.ExecuteNonQuery();

				Console.WriteLine("Card inserted successfully");
			}
		}
	}
}
