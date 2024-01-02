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
				return Text.Res_401_Unauthorized;
			}

			return response;
		}

		private static string RetrieveStack(string authToken)
		{
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				return Text.Res_401_Unauthorized;
			}

			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);
			List<Card> cardList = HttpRequestUtility.RetrieveUserCards(username, "stacks", dbConnection);

			if (cardList.Count <= 0)
			{
				// TODO: check why 204 didn't work
				// temporarily used 404
				return Text.Res_204_NoContent;
			}

			string cardsJson = JsonConvert.SerializeObject(cardList, Formatting.Indented);
			return String.Format(Text.Res_GetStack_200, cardsJson);
		}
	}
}
