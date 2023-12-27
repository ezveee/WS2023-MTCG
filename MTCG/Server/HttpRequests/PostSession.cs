using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server.HttpRequests
{
	internal class PostSession : IHttpRequest
	{
		public string GetResponse(string request)
		{
			string jsonPayload = IHttpRequest.ExtractJsonPayload(request);

			if (jsonPayload == null)
				return "";

			UserCredentials user = JsonConvert.DeserializeObject<UserCredentials>(jsonPayload);

			if (!LoginDbUser(user))
				return "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nInvalid username/password provided";

			return $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nAuthorization: Bearer {user.Username}-mtcgToken\r\n\r\nUser login successful";
		}
		private static bool LoginDbUser(UserCredentials user)
		{
			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM users WHERE username = '{user.Username}' AND password = '{user.Password}';", dbConnection);
			int count = Convert.ToInt32(command.ExecuteScalar());

			dbConnection.Close();

			if (count <= 0)
				return false;

			return true;
		}
	}
}
