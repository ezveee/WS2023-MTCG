using Microsoft.VisualBasic;
using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server.HttpRequests
{
	public class PostSession : IHttpRequest
	{
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
		private static bool LoginDbUser(UserCredentials user, out string authToken)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new(@"SELECT COUNT(*) FROM users WHERE username = @username AND password = @password;", dbConnection);
			command.Parameters.AddWithValue("username", user.Username);
			command.Parameters.AddWithValue("password", user.Password);
			int count = Convert.ToInt32(command.ExecuteScalar());

			if (count <= 0)
			{
				dbConnection.Close();
				authToken = String.Empty;
				return false;
			}

			CreateSession(user, dbConnection, out authToken);
			dbConnection.Close();

			return true;
		}

		private static void CreateSession(UserCredentials user, NpgsqlConnection dbConnection, out string authToken)
		{
			using NpgsqlCommand command = new(@"SELECT COUNT(*) FROM sessions WHERE username = @username;", dbConnection);
			command.Parameters.AddWithValue("username", user.Username);
			int count = Convert.ToInt32(command.ExecuteScalar());

			// token generation would be here
			// foundation for it implemented with 'out' parameters
			authToken = $"{user.Username}-mtcgToken";

			if (count <= 0)
			{
				using NpgsqlCommand insertStatement = new("INSERT INTO sessions (username, token, valid_until) VALUES (@username, @token, @validUntil);", dbConnection);

				insertStatement.Parameters.AddWithValue("username", user.Username);
				insertStatement.Parameters.AddWithValue("token", authToken);
				insertStatement.Parameters.AddWithValue("validUntil", DateTime.Now.AddMinutes(Constants.SessionTimeoutInMinutes));

				insertStatement.ExecuteNonQuery();
				return;
			}

			using NpgsqlCommand updateStatement = new($@"UPDATE sessions SET token = @token, valid_until = @validUntil WHERE username = @username;", dbConnection);

			updateStatement.Parameters.AddWithValue("token", authToken);
			updateStatement.Parameters.AddWithValue("validUntil", DateTime.Now.AddMinutes(Constants.SessionTimeoutInMinutes));
			updateStatement.Parameters.AddWithValue("username", user.Username);

			updateStatement.ExecuteNonQuery();
		}
	}
}
