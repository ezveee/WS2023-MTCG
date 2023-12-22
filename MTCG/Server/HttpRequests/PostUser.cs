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
			Console.WriteLine();
			Console.WriteLine("This is getting called in PostUser.GetResponse()");
			string jsonPayload = IHttpRequest.ExtractJsonPayload(request);
			Console.WriteLine("Stops here");
			Console.WriteLine();

			if (jsonPayload == null)
				return "";

			User user = JsonConvert.DeserializeObject<User>(jsonPayload);
			Console.WriteLine($"Username: {user.Username}, Password: {user.Password}");

			if (!CreateDbUser(user))
				return "HTTP/1.1 409 Conflict\r\nContent-Type: text/plain\r\n\r\nUser with same username already registered";

			return "HTTP/1.1 201 Created\r\nContent-Type: text/plain\r\n\r\nUser successfully created";

		}

		// TODO:	when trying to insert user with preexisting username, it doesn't create a new entry
		//			but the id goes up. find problem and fix
		private static bool CreateDbUser(User user)
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

				//Console.WriteLine($"Error: {ex.Message}");
			}

			return true;
		}
	}
}
