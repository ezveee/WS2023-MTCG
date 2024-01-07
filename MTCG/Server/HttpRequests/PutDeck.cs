using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests;

public class PutDeck : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public PutDeck(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		if (!HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken))
		{
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

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

		return cardIds.Count != 4
			? string.Format(Text.HttpResponse_400_BadRequest, Text.Description_PutDeck_400)
			: _dataAccess.ConfigureDeck(cardIds, authToken!);
	}
}
