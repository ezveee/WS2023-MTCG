using MTCG.Database.Schemas;
using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests
{
	public class PostSession : IHttpRequest
	{
		readonly IDataAccess _dataAccess;
		public PostSession(IDataAccess dataAccess)
		{
			_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
		}

		public string GetResponse(string request)
		{
			string jsonPayload = HttpRequestUtility.ExtractJsonPayload(request);

			if (jsonPayload == null)
			{
				return Text.HttpResponse_400_BadRequest;
			}

			UserCredentials? user = JsonConvert.DeserializeObject<UserCredentials>(jsonPayload);

			if (user == null)
			{
				return Text.HttpResponse_400_BadRequest;
			}

			if (!LoginDbUser(user, out string authToken))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_PostSession_401);
			}

			return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_PostSession_200, authToken);
		}
		private bool LoginDbUser(UserCredentials user, out string authToken)
		{
			int count = _dataAccess.DoUserAndPasswordExist(user);

			if (count <= 0)
			{
				authToken = String.Empty;
				return false;
			}

			_dataAccess.CreateSession(user, out authToken);
			return true;
		}
	}
}
