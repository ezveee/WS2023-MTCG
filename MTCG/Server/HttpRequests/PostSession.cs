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
				return Text.Res_400_BadRequest;
			}

			UserCredentials? user = JsonConvert.DeserializeObject<UserCredentials>(jsonPayload);

			if (user == null)
			{
				return Text.Res_400_BadRequest;
			}

			if (!LoginDbUser(user))
			{
				return Text.Res_PostSession_401;
			}

			// TODO: add to resource file; find out how to add username variable
			return String.Format(Text.Res_PostSession_200, user.Username);
		}
		private static bool LoginDbUser(UserCredentials user)
		{
			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM users WHERE username = '{user.Username}' AND password = '{user.Password}';", dbConnection);
			int count = Convert.ToInt32(command.ExecuteScalar());

			if (count <= 0)
			{
				dbConnection.Close();
				return false;
			}

			CreateSession(user, dbConnection);
			dbConnection.Close();

			return true;
		}

		private static void CreateSession(UserCredentials user, NpgsqlConnection dbConnection)
		{
			using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM sessions WHERE username = '{user.Username}';", dbConnection);
			int count = Convert.ToInt32(command.ExecuteScalar());

			if (count <= 0)
			{
				using NpgsqlCommand insertStatement = new("INSERT INTO sessions (username, token, valid_until) VALUES (@username, @token, @validUntil);", dbConnection);

				insertStatement.Parameters.AddWithValue("username", user.Username);
				insertStatement.Parameters.AddWithValue("token", $"{user.Username}-mtcgToken");
				insertStatement.Parameters.AddWithValue("validUntil", DateTime.Now.AddMinutes(Constants.SessionTimeoutInMinutes));

				insertStatement.ExecuteNonQuery();
				return;
			}

			using NpgsqlCommand updateStatement = new($@"UPDATE sessions SET valid_until = @validUntil WHERE username = @username;", dbConnection);

			updateStatement.Parameters.AddWithValue("validUntil", DateTime.Now.AddMinutes(Constants.SessionTimeoutInMinutes));
			updateStatement.Parameters.AddWithValue("username", user.Username);

			updateStatement.ExecuteNonQuery();
		}
	}
}
