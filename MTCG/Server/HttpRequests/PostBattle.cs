using MTCG.Battle;
using MTCG.Interfaces;

namespace MTCG.Server.HttpRequests
{
	public class PostBattle : IHttpRequest
	{
		readonly IDataAccess _dataAccess;
		public PostBattle(IDataAccess dataAccess)
		{
			_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
		}

		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			string username = _dataAccess.RetrieveUsernameFromToken(authToken!);

			List<ICard> deck = _dataAccess.RetrieveUserCards(username, "decks");

			if (deck.Count <= 0)
			{
				return string.Format(Text.HttpResponse_204_NoContent, Text.Description_GetDeck_204);
			}

			Player player = new(username, deck);
			BattleManager battleManager = new(_dataAccess);

			return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_PostBattle_200, battleManager.HandleBattle(player));
		}
	}
}
