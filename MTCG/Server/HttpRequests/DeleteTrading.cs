using MTCG.Interfaces;

namespace MTCG.Server.HttpRequests;

public class DeleteTrading : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public DeleteTrading(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		if (!HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken))
		{
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

		string? result = HttpRequestUtility.ExtractPathAddOns(request);
		if (result is null)
		{
			return string.Format(Text.HttpResponse_400_BadRequest);
		}
		Guid tradeId = Guid.Parse(result);

		return _dataAccess.DeleteTrade(tradeId, _dataAccess.RetrieveUsernameFromToken(authToken!));
	}
}
