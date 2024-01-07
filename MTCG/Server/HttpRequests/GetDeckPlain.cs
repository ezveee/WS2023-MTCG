using MTCG.Interfaces;
using System.Text;

namespace MTCG.Server.HttpRequests;

public class GetDeckPlain : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public GetDeckPlain(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		return !HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken)
			? string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401)
			: RetrieveDeck(authToken!);
	}

	private string RetrieveDeck(string authToken)
	{
		string username = _dataAccess.RetrieveUsernameFromToken(authToken);

		List<ICard> cardList = _dataAccess.RetrieveUserCards(username, "decks");

		if (cardList.Count <= 0)
		{
			return string.Format(Text.HttpResponse_204_NoContent, Text.Description_GetDeck_204);
		}

		StringBuilder cards = new();
		foreach (ICard card in cardList)
		{
			cards.AppendLine("----------------------------------------");
			cards.AppendLine("Id: " + card.Id);
			cards.AppendLine("Name: " + card.Name);
			cards.AppendLine("Element: " + card.Element);
			cards.AppendLine("Type: " + card.Type);
			cards.AppendLine("Damage: " + card.Damage);
		}

		return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetDeck_200, cards.ToString());
	}
}
