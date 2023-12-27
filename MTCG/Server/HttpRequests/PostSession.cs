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
				return "HTTP/1.1 409 Conflict\r\nContent-Type: text/plain\r\n\r\nUser with same username already registered";

			return "HTTP/1.1 201 Created\r\nContent-Type: text/plain\r\n\r\nUser successfully created";
		}
		private static bool LoginDbUser(UserCredentials user)
		{
			try
			{
				var dbConnection = DBManager.GetDBConnection();
				dbConnection.Open();

				using NpgsqlCommand command = new($@"INSERT INTO users (username, password, coins, elo) VALUES ('{user.Username}', '{user.Password}', 20, 100);", dbConnection);
				command.ExecuteNonQuery();

				dbConnection.Close();
			}
			catch (PostgresException ex)
			{
				if (ex.SqlState == "23505") // == unique_violation (https://www.postgresql.org/docs/current/errcodes-appendix.html)
					return false;
			}

			return true;
		}
	}
}
