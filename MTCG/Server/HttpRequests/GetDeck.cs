﻿using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests;

public class GetDeck : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public GetDeck(IDataAccess dataAccess)
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

		string cardsJson = JsonConvert.SerializeObject(cardList, Formatting.Indented);
		return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetDeck_200, cardsJson);
	}
}
