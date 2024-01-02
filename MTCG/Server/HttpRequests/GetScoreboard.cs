using MTCG.Cards;
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
using System.Xml.Linq;

namespace MTCG.Server.HttpRequests
{
	public class GetScoreboard : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request, out string? authToken))
			{
				return Text.Res_401_Unauthorized;
			}

			return RetrieveScoreboard(HttpRequestUtility.RetrieveUsernameFromToken(authToken!));
		}

		private static string RetrieveScoreboard(string username)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			List<UserStats> statList = new();

			using (NpgsqlCommand command = new("SELECT userdata.name, userstats.elo, userstats.wins, userstats.losses FROM users INNER JOIN userstats ON userstats.userid = users.id INNER JOIN userdata ON userdata.userid = users.id ORDER BY userstats.elo DESC;", dbConnection))
			{
				command.Parameters.AddWithValue("username", username);

				using NpgsqlDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					UserStats stats = new()
					{
						Name = reader.GetString(0),
						Elo = reader.GetInt32(1),
						Wins = reader.GetInt32(2),
						Losses = reader.GetInt32(3)
					};

					statList.Add(stats);
				}
			}

			dbConnection.Close();

			return string.Format(Text.Res_200_WithContent, JsonConvert.SerializeObject(statList, Formatting.Indented));
		}
	}
}
