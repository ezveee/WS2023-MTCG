using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class PutUser : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request))
			{
				return Text.Res_401_Unauthorized;
			}

			string? user;
			if ((user = HttpRequestUtility.ExtractPathAddOns(request)) is null)
				return Text.Res_400_BadRequest;

			string tokenUser = HttpRequestUtility.RetrieveUsernameFromToken(HttpRequestUtility.ExtractBearerToken(request));
			if (tokenUser != "admin" && tokenUser != user)
			{
				return Text.Res_401_Unauthorized;
			}

			// admin authorized but user not in db
			if (!DoesUserExist(user))
			{
				return Text.Res_404_User;
			}

			string jsonPayload = HttpRequestUtility.ExtractJsonPayload(request);

			if (jsonPayload == null)
			{
				return Text.Res_400_BadRequest;
			}

			UserData? userData = JsonConvert.DeserializeObject<UserData>(jsonPayload);

			if (userData == null)
			{
				return Text.Res_400_BadRequest;
			}

			return InsertUserData(user, userData);
		}

		private static string InsertUserData(string username, UserData userData)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			NpgsqlCommand command = new("UPDATE userdata SET name = @name, bio = @bio, image = @image FROM users WHERE userdata.userid = users.id AND users.username = @username;", dbConnection);
			command.Parameters.AddWithValue("name", userData.Name);
			command.Parameters.AddWithValue("bio", userData.Bio);
			command.Parameters.AddWithValue("image", userData.Image);
			command.Parameters.AddWithValue("username", username);
			command.ExecuteNonQuery();

			dbConnection.Close();

			string userDataJson = JsonConvert.SerializeObject(userData, Formatting.Indented);
			return Text.Res_PutUser_200;
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
