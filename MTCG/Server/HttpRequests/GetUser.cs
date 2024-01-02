using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class GetUser : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request))
			{
				return Text.Res_401_Unauthorized;
			}

			string? user;
			if ((user = HttpRequestUtility.ExtractPathAddOns(request)) is null)
			{
				return Text.Res_400_BadRequest;
			}

			string tokenUser = HttpRequestUtility.RetrieveUsernameFromToken(HttpRequestUtility.ExtractBearerToken(request));
			if (tokenUser != "admin" && tokenUser != user)
			{
				return Text.Res_401_Unauthorized;
			}

			if (!DoesUserExist(user))
			{
				return Text.Res_404_User;
			}

			return RetrieveUserData(user);
		}

		private static string RetrieveUserData(string user)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			NpgsqlCommand command = new("SELECT name, bio, image FROM userdata INNER JOIN users ON users.id = userdata.userid WHERE users.username = @username;", dbConnection);
			command.Parameters.AddWithValue("username", user);

			UserData userData;
			using (NpgsqlDataReader reader = command.ExecuteReader())
			{
				reader.Read();
				userData = new()
				{
					Name = reader.GetString(0),
					Bio = reader.GetString(1),
					Image = reader.GetString(2)
				};
			}

			dbConnection.Close();

			string userDataJson = JsonConvert.SerializeObject(userData, Formatting.Indented);
			return String.Format(Text.Res_GetUser_200, userDataJson);
		}

		// TODO: db layer?
		private static bool DoesUserExist(string username)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new("SELECT COUNT(*) FROM users WHERE username = @username", dbConnection);
			command.Parameters.AddWithValue("username", username);

			if (Convert.ToInt32(command.ExecuteScalar()) <= 0)
			{
				dbConnection.Close();
				return false;
			}

			dbConnection.Close();
			return true;
		}
	}
}
