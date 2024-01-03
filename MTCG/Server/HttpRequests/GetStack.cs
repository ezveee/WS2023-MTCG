using MTCG.Cards;
using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class GetStack : IHttpRequest
	{
		public string GetResponse(string request)
		{
			string response;
			try
			{
				response = RetrieveStack(HttpRequestUtility.ExtractBearerToken(request));
			}
			catch (InvalidOperationException)
			{
				return String.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			return response;
		}

		private static string RetrieveStack(string authToken)
		{
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				return String.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);
			List<Card> cardList = HttpRequestUtility.RetrieveUserCards(username, "stacks", dbConnection);

			if (cardList.Count <= 0)
			{
				return String.Format(Text.HttpResponse_204_NoContent, Text.Description_GetStack_204);
			}

			string cardsJson = JsonConvert.SerializeObject(cardList, Formatting.Indented);
			return String.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetStack_200, cardsJson);
		}
	}
}
