using MTCG.Battle;
using MTCG.Database;
using MTCG.Interfaces.ICard;
using MTCG.Interfaces.IHttpRequest;

namespace MTCG.Server.HttpRequests
{
	public class PostBattle : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request, out string? authToken))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken!);

			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			List<ICard> deck = HttpRequestUtility.RetrieveUserCards(username, "decks", dbConnection);

			dbConnection.Close();

			if (deck.Count <= 0)
			{
				return string.Format(Text.HttpResponse_204_NoContent, Text.Description_GetDeck_204);
			}

			Player player = new(username, deck);

			return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_PostBattle_200, BattleManager.HandleBattle(player));
		}
	}
}
