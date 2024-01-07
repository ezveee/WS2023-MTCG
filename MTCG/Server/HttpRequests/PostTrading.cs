using MTCG.Database.Schemas;
using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests;

public class PostTrading : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public PostTrading(IDataAccess dataAccess)
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

		string? tradeId = HttpRequestUtility.ExtractPathAddOns(request);
		if (tradeId is not null)
		{
			return new CarryOutTrade(_dataAccess, tradeId, username).GetResponse(request);
		}

		TradingDeal trade;
		try
		{
			trade = HttpRequestUtility.DeserializeJson<TradingDeal>(request);
		}
		catch (Exception ex) when (ex is InvalidOperationException or JsonSerializationException)
		{
			return Text.HttpResponse_400_BadRequest;
		}

		return _dataAccess.CreateTrade(trade, username);
	}
}
