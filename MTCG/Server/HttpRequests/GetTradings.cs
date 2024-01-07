using MTCG.Database.Schemas;
using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests;

public class GetTradings : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public GetTradings(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		if (!HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out _))
		{
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

		List<TradingDeal> tradesList = _dataAccess.RetrieveTrades();

		if (tradesList.Count <= 0)
		{
			return string.Format(Text.HttpResponse_204_NoContent, Text.Description_GetTradings_204);
		}

		string tradesJson = JsonConvert.SerializeObject(tradesList, Formatting.Indented);
		return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetTradings_200, tradesJson);
	}
}
