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
				return Text.HttpResponse_400_BadRequest;
			}

			List<Guid>? cardIds = JsonConvert.DeserializeObject<List<Guid>>(jsonPayload);

			if (cardIds == null)
			{
				return Text.HttpResponse_400_BadRequest;
			}

			if (cardIds.Count != 4)
			{
				return String.Format(Text.HttpResponse_400_BadRequest, Text.Description_PutDeck_400);
			}

			// TODO: check if any of new cards are engaged in a trade

			string response;
			try
			{
				response = ConfigureDeck(cardIds, HttpRequestUtility.ExtractBearerToken(request));
			}
			catch (InvalidOperationException)
			{
				return String.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			return response;
		}

		private static string ConfigureDeck(List<Guid> cardIds, string authToken)
		{
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				return String.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);
			using (NpgsqlCommand command = new(@"SELECT COUNT(*) FROM stacks JOIN users ON stacks.userid = users.id WHERE users.username = @username AND stacks.cardid = ANY(@ids);", dbConnection))
			{
				command.Parameters.AddWithValue("username", username);
				command.Parameters.AddWithValue("ids", cardIds.ToArray());
				int count = Convert.ToInt32(command.ExecuteScalar());

				if (count != 4)
				{
					return String.Format(Text.HttpResponse_403_Forbidden, Text.Description_PutDeck_403);
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
				return Text.HttpResponse_500_InternalServerError;
			}

			dbConnection.Close();
			return String.Format(Text.HttpResponse_200_OK, Text.Description_PutDeck_200);
		}
	}
}
