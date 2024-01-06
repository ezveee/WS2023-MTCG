using MTCG.Database.Schemas;
using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server.HttpRequests
{
	public class PostUser : IHttpRequest
	{
		readonly IDataAccess _dataAccess;
		public PostUser(IDataAccess dataAccess)
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

			if (!_dataAccess.CreateDbUser(user))
			{
				return string.Format(Text.HttpResponse_409_Conflict, Text.Description_PostUser_409);
			}

			return string.Format(Text.HttpResponse_201_Created, Text.Description_PostUser_201);
		}
	}
}
