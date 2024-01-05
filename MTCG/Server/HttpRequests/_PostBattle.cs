using MTCG.Battle;
using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class PostBattle : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request, out string? authToken))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken!);

			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using (NpgsqlCommand command = new(
				@"SELECT cards.name, cards.cardtype, cards.element, cards.damage
					FROM decks
					JOIN cards ON decks.cardid = cards.id
					JOIN users ON decks.userId = users.id
					WHERE username = @username;", dbConnection))
			{
				using NpgsqlDataReader reader = command.ExecuteReader();

				while (reader.Read())
				{

				}
			}


			dbConnection.Close();



			// create Player object from user deck/userdata
			Player player = new();


			return BattleManager.HandleBattle(player);
		}
	}
}
