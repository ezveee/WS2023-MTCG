using MTCG.Interfaces;

namespace MTCG.Server.HttpRequests;

public class GetScoreboard : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public GetScoreboard(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		return !HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken)
			? string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401)
			: _dataAccess.RetrieveScoreboard(_dataAccess.RetrieveUsernameFromToken(authToken!));
	}
}
