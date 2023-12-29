using MTCG.Cards;
using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class PutDeck : IHttpRequest
	{
		public string GetResponse(string request)
		{
			string jsonPayload = HttpRequestUtility.ExtractJsonPayload(request);

			if (jsonPayload == null)
			{
				return Text.Res_400_BadRequest;
			}

			List<Guid>? cardIds = JsonConvert.DeserializeObject<List<Guid>>(jsonPayload);

			if (cardIds == null)
			{
				return Text.Res_400_BadRequest;
			}

			if (cardIds.Count != 4)
			{
				return Text.Res_PutDeck_400;
			}

			string response;
			try
			{
				response = ConfigureDeck(cardIds, HttpRequestUtility.ExtractBearerToken(request));
			}
			catch (InvalidOperationException)
			{
				return Text.Res_401_Unauthorized;
			}

			return response;
		}

		private static string ConfigureDeck(List<Guid> cardIds, string authToken)
		{
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				return Text.Res_401_Unauthorized;
			}

			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);
			using (NpgsqlCommand command = new(@"SELECT COUNT(*) FROM stacks JOIN users ON stacks.userid = users.id WHERE users.username = @username AND stacks.cardid = ANY(@ids);", dbConnection))
			{
				command.Parameters.AddWithValue("username", username);
				command.Parameters.AddWithValue("ids", cardIds.ToArray());
				int count = Convert.ToInt32(command.ExecuteScalar());

				if (count != 4)
				{
					return Text.Res_PutDeck_403;
				}
			}

			using var transaction = dbConnection.BeginTransaction();

			try
			{
				using (var command = new NpgsqlCommand())
				{
					command.Connection = dbConnection;
					command.Transaction = transaction;

					command.CommandText = "DELETE FROM decks USING users WHERE decks.userid = users.id AND users.username = @username;";
					command.Parameters.Clear();
					command.Parameters.AddWithValue("username", username);
					command.ExecuteNonQuery();

					foreach (Guid id in cardIds)
					{
						command.CommandText = "INSERT INTO decks (userid, cardid) SELECT id, @cardid FROM users WHERE username = @username";
						command.Parameters.Clear();
						command.Parameters.AddWithValue("username", username);
						command.Parameters.AddWithValue("cardid", id);
						command.ExecuteNonQuery();
					}
				}

				transaction.Commit();
				Console.WriteLine("Transaction committed successfully");
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
				return Text.Res_500_ServerError;
			}

			dbConnection.Close();
			return Text.Res_PutDeck_200;
		}


		private static void InsertIntoCardsTable(List<Database.Schemas.Card> package, NpgsqlCommand command)
		{
			foreach (var card in package)
			{
				command.CommandText = $@"INSERT INTO cards (id, name, cardtype, element, damage) VALUES (@id, @name, @cardtype, @element, @damage);";

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
