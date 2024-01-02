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
	public class PostUser : IHttpRequest
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
			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new();
			command.Connection = dbConnection;

			try
			{
				command.CommandText = "INSERT INTO users (username, password, coins) VALUES (@username, @password, 20);";
				command.Parameters.AddWithValue("username", user.Username);
				command.Parameters.AddWithValue("password", user.Password);
				command.ExecuteNonQuery();

			}
			catch (PostgresException ex)
			{
				if (ex.SqlState == "23505") // == unique_violation (https://www.postgresql.org/docs/current/errcodes-appendix.html)
				{
					dbConnection.Close();
					return false;
				}
			}

			command.Parameters.Clear();
			command.CommandText = "SELECT id FROM users WHERE username = @username;";
			command.Parameters.AddWithValue("username", user.Username);
			int id = Convert.ToInt32(command.ExecuteScalar());

			command.Parameters.Clear();
			command.CommandText = "INSERT INTO userdata (userid, name, bio, image) VALUES (@id, @username, '', '');";
			command.Parameters.AddWithValue("id", id);
			command.Parameters.AddWithValue("username", user.Username);
			command.ExecuteNonQuery();

			command.Parameters.Clear();
			command.CommandText = "INSERT INTO userstats (userid, elo, wins, losses) VALUES (@id, 100, 0, 0);";
			command.Parameters.AddWithValue("id", id);
			command.ExecuteNonQuery();

			dbConnection.Close();
			return true;
		}
	}
}
