using MTCG.Database.Schemas;
using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests
{
	public class PutUser : IHttpRequest
	{
		readonly IDataAccess _dataAccess;
		public PutUser(IDataAccess dataAccess)
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

			// admin authorized but user not in db
			if (!_dataAccess.DoesUserExist(user))
			{
				return string.Format(Text.HttpResponse_404_NotFound, Text.Description_Default_404_User);
			}

			string jsonPayload = HttpRequestUtility.ExtractJsonPayload(request);

			if (jsonPayload == null)
			{
				return Text.HttpResponse_400_BadRequest;
			}

			UserData? userData = JsonConvert.DeserializeObject<UserData>(jsonPayload);

			if (userData == null)
			{
				return Text.HttpResponse_400_BadRequest;
			}

			return _dataAccess.InsertUserData(user, userData);
		}
	}
}
