using MTCG.Interfaces;

namespace MTCG.Server.HttpRequests;

public class PostTransaction : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public PostTransaction(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		if (!HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken))
		{
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

		return HttpRequestUtility.ExtractPathAddOns(request) != "packages"
			? string.Format(Text.HttpResponse_400_BadRequest)
			: _dataAccess.AquirePackage(authToken!);
	}
}
