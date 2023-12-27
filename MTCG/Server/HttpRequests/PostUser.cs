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
	internal class PostUser : IHttpRequest
	{
		public string GetResponse(string request)
		{
			string jsonPayload = HttpRequestUtility.ExtractJsonPayload(request);

			if (jsonPayload == null)
			{
				return Text.Res_400;
			}

			UserCredentials? user = JsonConvert.DeserializeObject<UserCredentials>(jsonPayload);

			if (user == null)
			{
				return Text.Res_400;
			}

			if (!CreateDbUser(user))
			{
				return Text.Res_PostUser_409;
			}

			return Text.Res_PostUser_201;
		}

		// TODO:	when trying to insert user with preexisting username, it doesn't create a new entry
		//			but the id goes up. find problem and fix
		//			possible solution: do query to check if exists if not -> insert
		private static bool CreateDbUser(UserCredentials user)
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
				{
					return false;
				}
			}

			return true;
		}
	}
}
