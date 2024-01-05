using MTCG.Cards;
using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.ICard;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server
{
	public static class HttpRequestUtility
	{
		public static T DeserializeJson<T>(string request)
		{
			string jsonPayload = ExtractJsonPayload(request) ?? throw new InvalidOperationException();
			T obj = JsonConvert.DeserializeObject<T>(jsonPayload) ?? throw new JsonSerializationException();
			return obj;
		}

		public static string ExtractJsonPayload(string request)
		{
			int bodyStartIndex = request.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4;
			string jsonPayload = request[bodyStartIndex..].Trim(); // .. range operator instead of request.Substring(bodyStartIndex)

			return jsonPayload;
		}

		public static string? ExtractPathAddOns(string request)
		{
			string[] lines = request.Split('\n');
			string[] tokens = lines[0].Split(' ');
			string fullPath = tokens[1];
			string[] pathComponents = fullPath.Split('/');

			if (pathComponents.Length < 3)
			{
				return null;
			}

			return pathComponents[2];
		}

		public static string ExtractBearerToken(string request)
		{
			// Find the Authorization header
			int authHeaderIndex = request.IndexOf("Authorization: ");
			if (authHeaderIndex == -1)
			{
				throw new InvalidOperationException("Authorization header not found");
			}

			// Extract the Authorization header value
			string authHeaderValue = request[(authHeaderIndex + "Authorization: ".Length)..];
			int endIndex = authHeaderValue.IndexOf("\r\n");
			if (endIndex == -1)
			{
				throw new InvalidOperationException("Invalid Authorization header format");
			}

			authHeaderValue = authHeaderValue[..endIndex];

			if (!authHeaderValue.StartsWith("Bearer "))
			{
				throw new InvalidOperationException("Invalid Authorization header format");
			}

			// Extract and return the Bearer token
			return authHeaderValue["Bearer ".Length..];
		}

		public static bool IsTokenValid(string authToken)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new("SELECT valid_until FROM sessions WHERE token = @token;", dbConnection);
			command.Parameters.AddWithValue("token", authToken);
			object? result = command.ExecuteScalar();

			if (result is null)
			{
				return false;
			}

			DateTime? validUntil = result as DateTime?;

			if (validUntil < DateTime.Now)
			{
				dbConnection.Close();
				return false;
			}

			dbConnection.Close();
			return true;
		}

		public static string RetrieveUsernameFromToken(string authToken)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new($@"SELECT username FROM sessions WHERE token = '{authToken}';", dbConnection);

			return (string)command.ExecuteScalar();
		}

		public static List<ICard> RetrieveUserCards(string userName, string tableName, NpgsqlConnection dbConnection)
		{
			List<ICard> cardList = new();

			using NpgsqlCommand command = new(
				@$"SELECT cards.id, cards.name, cards.damage
					FROM {tableName}
					JOIN users ON {tableName}.userid = users.id
					JOIN cards ON {tableName}.cardid = cards.id
					WHERE users.username = @user;", dbConnection);

			command.Parameters.AddWithValue("user", userName);

			using NpgsqlDataReader reader = command.ExecuteReader();

			while (reader.Read())
			{
				ICard card = (ICard)Cards.Card.CreateInstance(reader.GetGuid(0), reader.GetString(1), (float)reader.GetDouble(2));

				if (card is null)
				{
					continue;
				}

				cardList.Add(card);
			}

			return cardList;
		}
		public static bool IsUserAccessValid(string request, out string? authToken)
		{
			try
			{
				authToken = ExtractBearerToken(request);
			}
			catch (InvalidOperationException)
			{
				authToken = null;
				return false;
			}

			if (!IsTokenValid(authToken))
			{
				return false;
			}

			return true;
		}

		public static bool DoesCardBelongToUser(Guid cardid, string username)
		{
			using var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM stacks INNER JOIN users ON stacks.userid = users.id WHERE users.username = @username AND stacks.cardid = @id;", dbConnection);
			command.Parameters.AddWithValue("username", username);
			command.Parameters.AddWithValue("id", cardid);
			int count = Convert.ToInt32(command.ExecuteScalar());

			dbConnection.Close();
			return count > 0;
		}

		public static bool DoesDealIdAlreadyExist(Guid tradeId)
		{
			using var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM trades WHERE id = @id;", dbConnection);
			command.Parameters.AddWithValue("id", tradeId);
			int count = Convert.ToInt32(command.ExecuteScalar());

			dbConnection.Close();
			return count > 0;
		}

		public static bool IsCardInUserDeck(Guid cardid, string username)
		{
			using var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM decks INNER JOIN users ON decks.userid = users.id WHERE users.username = @username AND decks.cardid = @id;", dbConnection);
			command.Parameters.AddWithValue("username", username);
			command.Parameters.AddWithValue("id", cardid);
			int count = Convert.ToInt32(command.ExecuteScalar());

			dbConnection.Close();
			return count > 0;
		}

		public static Guid? RetrieveCardidFromTradeid(Guid tradeId)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			Guid cardId;
			using (NpgsqlCommand command = new("SELECT cardid FROM trades WHERE id = @id;", dbConnection))
			{
				command.Parameters.AddWithValue("id", tradeId);
				object? obj = command.ExecuteScalar();
				if (obj is null)
				{
					return null;
				}
				cardId = (Guid)obj;
			}

			dbConnection.Close();
			return cardId;
		}

		public static bool IsCardEngagedInTrade(Guid cardid)
		{
			using var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM trades WHERE cardid = @id;", dbConnection);
			command.Parameters.AddWithValue("id", cardid);
			int count = Convert.ToInt32(command.ExecuteScalar());

			dbConnection.Close();
			return count > 0;
		}
	}
}
