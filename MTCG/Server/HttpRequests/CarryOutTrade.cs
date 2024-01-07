using MTCG.Interfaces;

namespace MTCG.Server.HttpRequests;

public class CarryOutTrade : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	private readonly Guid tradeId;
	private readonly string username;

	public CarryOutTrade(IDataAccess dataAccess, string tId, string user)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
		tradeId = new Guid(tId);
		username = user;
	}

	public string GetResponse(string request)
	{
		string jsonPayload = HttpRequestUtility.ExtractJsonPayload(request).Trim('\"');
		Guid offeredCardId = new(jsonPayload);

		if (!_dataAccess.DoesDealIdAlreadyExist(tradeId))
		{
			return string.Format(Text.HttpResponse_404_NotFound, Text.Description_PostTrading_CarryOutTrade_404);
		}

		return !_dataAccess.DoesCardBelongToUser(offeredCardId, username)
			|| _dataAccess.IsCardInUserDeck(offeredCardId, username)
			|| !DoesCardMeetRequirements(offeredCardId)
			? string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostTrading_CarryOutTrade_403)
			: _dataAccess.ExecuteTrade(tradeId, offeredCardId, username);
	}

	private bool DoesCardMeetRequirements(Guid offeredCardId)
	{
		Tuple<string, float> requiredStats = _dataAccess.GetTradeRequirements(tradeId);
		Tuple<string, float> offeredStats = _dataAccess.GetCardRequirements(offeredCardId);

		if (offeredStats.Item1 != requiredStats.Item1)
		{
			return false;
		}

		return offeredStats.Item2 >= requiredStats.Item2;
	}
}