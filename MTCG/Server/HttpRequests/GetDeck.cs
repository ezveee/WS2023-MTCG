using MTCG.Cards;
using MTCG.Database;
using MTCG.Interfaces.ICard;
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
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			return response;
		}

		private static string RetrieveDeck(string authToken)
		{
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);

			List<ICard> cardList = HttpRequestUtility.RetrieveUserCards(username, "decks", dbConnection);

			if (cardList.Count <= 0)
			{
				return string.Format(Text.HttpResponse_204_NoContent, Text.Description_GetDeck_204);
			}

			string cardsJson = JsonConvert.SerializeObject(cardList, Formatting.Indented);
			return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetDeck_200, cardsJson);
		}
	}
}
