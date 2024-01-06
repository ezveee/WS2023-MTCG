using MTCG.Interfaces;

namespace MTCG.Server.HttpRequests
{
	public class DeleteTrading : IHttpRequest
	{
		readonly IDataAccess _dataAccess;
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

			Guid tradeId;
			try
			{
				tradeId = new(HttpRequestUtility.ExtractPathAddOns(request));
			}
			catch
			{
				// TODO: maybe change to just text. ... cause no description is needed
				return string.Format(Text.HttpResponse_400_BadRequest);
			}

			return _dataAccess.DeleteTrade(tradeId, _dataAccess.RetrieveUsernameFromToken(authToken!));
		}
	}
}
