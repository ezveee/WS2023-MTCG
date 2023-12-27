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
				return Text.Res_400;
			}

			List<Database.Schemas.Card>? package = JsonConvert.DeserializeObject<List<Database.Schemas.Card>>(jsonPayload);

			if (package == null || package.Count != 5)
			{
				return Text.Res_400;
			}

			return CreatePackage(package, HttpRequestUtility.ExtractBearerToken(request));
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

			// TODO: make it so that chekcing if token exists is seperate from ExtractBearerToken method
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				dbConnection.Close();
				return Text.Res_PostPackage_401;
			}

			using NpgsqlCommand command = new(@"INSERT INTO packages (cards) VALUES (@ids);", dbConnection);

			command.Parameters.AddWithValue("ids", new[] { package[0].Id, package[1].Id, package[2].Id, package[3].Id, package[4].Id });

			command.ExecuteNonQuery();

			InsertIntoCardsTable(package, dbConnection);

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

		private static void InsertIntoCardsTable(List<Database.Schemas.Card> package, NpgsqlConnection dbConnection)
		{
			foreach (var card in package)
			{
				using NpgsqlCommand command = new($@"INSERT INTO cards (id, name, cardtype_f, element_f, damage) VALUES (@id, @name, @cardtype, @element, @damage);", dbConnection);

				command.Parameters.AddWithValue("id", card.Id);
				command.Parameters.AddWithValue("name", card.Name);
				command.Parameters.AddWithValue("cardtype", HttpRequestUtility.cardCategories[card.Name].Item1);
				command.Parameters.AddWithValue("element", HttpRequestUtility.cardCategories[card.Name].Item2);
				command.Parameters.AddWithValue("damage", card.Damage);

				command.ExecuteNonQuery();

				Console.WriteLine("Card inserted successfully");
			}
		}
	}
}
