using MTCG.Database.Schemas;
using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests;

public class GetStats : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public GetStats(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		return !HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken)
			? string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401)
			: RetrieveStats(_dataAccess.RetrieveUsernameFromToken(authToken!));
	}

	private string RetrieveStats(string username)
	{
		UserStats? stats = _dataAccess.RetrieveUserStats(username);

		// should never occur
		// stats get initialized upon user creation
		return stats is null
			? Text.HttpResponse_500_InternalServerError
			: string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetStats_200, JsonConvert.SerializeObject(stats, Formatting.Indented));
	}
}
