using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class GetStats : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request, out string? authToken))
			{
				return Text.HttpResponse_401_Unauthorized;
			}

			return RetrieveStats(HttpRequestUtility.RetrieveUsernameFromToken(authToken!));
		}

		private static string RetrieveStats(string username)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			UserStats? stats = null;

			using (NpgsqlCommand command = new("SELECT userdata.name, userstats.elo, userstats.wins, userstats.losses FROM users INNER JOIN userstats ON userstats.userid = users.id INNER JOIN userdata ON userdata.userid = users.id WHERE users.username = @username;", dbConnection))
			{
				command.Parameters.AddWithValue("username", username);

				using NpgsqlDataReader reader = command.ExecuteReader();
				if (reader.Read())
				{
					stats = new UserStats
					{
						Name = reader.GetString(0),
						Elo = reader.GetInt32(1),
						Wins = reader.GetInt32(2),
						Losses = reader.GetInt32(3)
					};
				}
			}

			dbConnection.Close();

			// should never occur
			// stats get initialized upon user creation
			if (stats is null)
			{
				return Text.HttpResponse_500_InternalServerError;
			}

			return string.Format(Text.HttpResponse_200_OK_WithContent, JsonConvert.SerializeObject(stats, Formatting.Indented));
		}
	}
}
