using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests;

public class PostPackage : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public PostPackage(IDataAccess dataAccess)
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

		List<Database.Schemas.Card>? package = JsonConvert.DeserializeObject<List<Database.Schemas.Card>>(jsonPayload);

		return package == null || package.Count != 5 ? Text.HttpResponse_400_BadRequest : _dataAccess.CreatePackage(package, authToken!);
	}
}
