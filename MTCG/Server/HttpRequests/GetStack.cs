using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests;

public class GetStack : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public GetStack(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		string response;
		try
		{
			response = RetrieveStack(HttpRequestUtility.ExtractBearerToken(request));
		}
		catch (InvalidOperationException)
		{
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

		return response;
	}

	private string RetrieveStack(string authToken)
	{
		if (!_dataAccess.IsTokenValid(authToken))
		{
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

		string username = _dataAccess.RetrieveUsernameFromToken(authToken);
		List<ICard> cardList = _dataAccess.RetrieveUserCards(username, "stacks");

		if (cardList.Count <= 0)
		{
			return string.Format(Text.HttpResponse_204_NoContent, Text.Description_GetStack_204);
		}

		string cardsJson = JsonConvert.SerializeObject(cardList, Formatting.Indented);
		return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetStack_200, cardsJson);
	}
}
