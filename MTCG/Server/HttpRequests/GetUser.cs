using MTCG.Database.Schemas;
using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests;

public class GetUser : IHttpRequest
{
	private readonly IDataAccess _dataAccess;
	public GetUser(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
	}

	public string GetResponse(string request)
	{
		if (!HttpRequestUtility.IsUserAccessValid(_dataAccess, request, out string? authToken))
		{
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

		string? user;
		if ((user = HttpRequestUtility.ExtractPathAddOns(request)) is null)
		{
			return Text.HttpResponse_400_BadRequest;
		}

		string tokenUser = _dataAccess.RetrieveUsernameFromToken(authToken!);
		if (tokenUser != "admin" && tokenUser != user)
		{
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

		if (!_dataAccess.DoesUserExist(user))
		{
			return string.Format(Text.HttpResponse_404_NotFound, Text.Description_Default_404_User);
		}

		UserData userData = _dataAccess.RetrieveUserData(user);

		string userDataJson = JsonConvert.SerializeObject(userData, Formatting.Indented);
		return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetUser_200, userDataJson);
	}
}
