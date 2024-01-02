using MTCG.Cards;
using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class GetDeck : IHttpRequest
	{
		public string GetResponse(string request)
		{
			string response;
			try
			{
				response = RetrieveDeck(HttpRequestUtility.ExtractBearerToken(request));
			}
			catch (InvalidOperationException)
			{
				return Text.Res_401_Unauthorized;
			}

			return response;
		}

		private static string RetrieveDeck(string authToken)
		{
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				return Text.Res_401_Unauthorized;
			}

			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);

			// TODO: see if description is even necessary
			string? description;
			using (NpgsqlCommand command = new(
				@"SELECT description FROM decks
				JOIN users ON decks.userid = users.id
				WHERE users.username = @user;", dbConnection))
			{
				command.Parameters.AddWithValue("user", username);

				object? obj = command.ExecuteScalar();
				description = (obj is null) ? string.Empty : obj.ToString();
			}

			List<Card> cardList = HttpRequestUtility.RetrieveUserCards(username, "decks", dbConnection);

			if (cardList.Count <= 0)
			{
				// TODO: check why 204 didn't work
				// temporarily used 404
				return Text.Res_204_NoContent;
			}

			string cardsJson = JsonConvert.SerializeObject(cardList, Formatting.Indented);
			return String.Format(Text.Res_GetDeck_200, cardsJson, description);
		}
	}
}
