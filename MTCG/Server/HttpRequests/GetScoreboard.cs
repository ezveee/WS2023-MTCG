using MTCG.Interfaces;

namespace MTCG.Server.HttpRequests
{
	public class GetScoreboard : IHttpRequest
	{
		readonly IDataAccess _dataAccess;
		public GetScoreboard(IDataAccess dataAccess)
		{
			_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
		}

		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			return _dataAccess.RetrieveScoreboard(_dataAccess.RetrieveUsernameFromToken(authToken!));
		}
	}
}
