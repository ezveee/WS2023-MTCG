using MTCG.Cards;
using MTCG.Database.Schemas;
using MTCG.Interfaces;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Database;

public class DataAccess : IDataAccess
{
	private static readonly string connectionString = "Host=localhost;Username=vee;Password=1234";
	private readonly NpgsqlConnection connection = new(connectionString);

	public void CloseConnection()
	{
		connection?.Close();
	}

	public static NpgsqlConnection GetDbConnection()
	{
		return new NpgsqlConnection(connectionString + "; Database=mtcg");
	}

	// initial db object creation
	public void DbSetup()
	{
		connection.Open();

		foreach (KeyValuePair<string, (string, string?)> entry in dbObjectInitCommands)
		{
			InitDbObject(entry.Key, entry.Value.Item1, entry.Value.Item2);
		}
	}

	private void InitDbObject(string tableName, string createStatement, string? insertStatement)
	{
		bool tableExists = TableExists(tableName);

		if (tableExists)
		{
			Console.WriteLine($"Database object '{tableName}' already exists.");
			return;
		}

		CreateDbObject(tableName, createStatement);
		if (insertStatement != null)
		{
			InsertValues(tableName, insertStatement);
		}
	}

	public void UpdateUserStats(string winner, string loser)
	{
		using var dbConnection = GetDbConnection();
		dbConnection.Open();

		using var transaction = dbConnection.BeginTransaction();

		try
		{
			using (var command = new NpgsqlCommand())
			{
				command.Connection = dbConnection;
				command.Transaction = transaction;

				command.CommandText =
					@"UPDATE userstats
							SET elo = elo - 5, losses = losses + 1
							FROM users
							WHERE userstats.userid = users.id AND users.username = @loser";
				command.Parameters.AddWithValue("loser", loser);
				command.ExecuteNonQuery();

				command.Parameters.Clear();

				command.CommandText =
					@"UPDATE userstats
							SET elo = elo + 3, wins = wins + 1
							FROM users
							WHERE userstats.userid = users.id AND users.username = @winner";
				command.Parameters.AddWithValue("winner", winner);
				command.ExecuteNonQuery();
			}

			transaction.Commit();
			Console.WriteLine("Transaction committed successfully");
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
		}

		dbConnection.Close();
	}

	public string InsertUserData(string username, UserData userData)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		NpgsqlCommand command = new("UPDATE userdata SET name = @name, bio = @bio, image = @image FROM users WHERE userdata.userid = users.id AND users.username = @username;", dbConnection);
		command.Parameters.AddWithValue("name", userData.Name);
		command.Parameters.AddWithValue("bio", userData.Bio);
		command.Parameters.AddWithValue("image", userData.Image);
		command.Parameters.AddWithValue("username", username);
		command.ExecuteNonQuery();

		dbConnection.Close();

		string userDataJson = JsonConvert.SerializeObject(userData, Formatting.Indented);
		return string.Format(Text.HttpResponse_200_OK, Text.Description_PutUser_200);
	}

	public string ConfigureDeck(List<Guid> cardIds, string authToken)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		string username = RetrieveUsernameFromToken(authToken);
		using (NpgsqlCommand command = new(@"SELECT COUNT(*) FROM stacks JOIN users ON stacks.userid = users.id WHERE users.username = @username AND stacks.cardid = ANY(@ids);", dbConnection))
		{
			command.Parameters.AddWithValue("username", username);
			command.Parameters.AddWithValue("ids", cardIds.ToArray());
			int count = Convert.ToInt32(command.ExecuteScalar());

			if (count != 4)
			{
				return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PutDeck_403);
			}
		}

		using var transaction = dbConnection.BeginTransaction();

		try
		{
			using (var command = new NpgsqlCommand())
			{
				command.Connection = dbConnection;
				command.Transaction = transaction;

				command.CommandText = "DELETE FROM decks USING users WHERE decks.userid = users.id AND users.username = @username;";
				command.Parameters.Clear();
				command.Parameters.AddWithValue("username", username);
				command.ExecuteNonQuery();

				foreach (Guid id in cardIds)
				{
					command.CommandText = "INSERT INTO decks (userid, cardid) SELECT id, @cardid FROM users WHERE username = @username";
					command.Parameters.Clear();
					command.Parameters.AddWithValue("username", username);
					command.Parameters.AddWithValue("cardid", id);
					command.ExecuteNonQuery();
				}
			}

			transaction.Commit();
			Console.WriteLine("Transaction committed successfully");
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
			return Text.HttpResponse_500_InternalServerError;
		}

		dbConnection.Close();
		return string.Format(Text.HttpResponse_200_OK, Text.Description_PutDeck_200);
	}

	// TODO:	when trying to insert user with preexisting username, it doesn't create a new entry
	//			but the id goes up. find problem and fix
	//			possible solution: do query to check if exists if not -> insert
	public bool CreateDbUser(UserCredentials user)
	{
		var dbConnection = GetDbConnection();
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

	public string AquirePackage(string authToken)
	{
		var dbConnection = GetDbConnection();
		dbConnection.Open();

		string username = RetrieveUsernameFromToken(authToken);
		using (NpgsqlCommand command = new($@"SELECT coins FROM users WHERE username = @user;", dbConnection))
		{
			command.Parameters.AddWithValue("user", username);

			int coins = Convert.ToInt32(command.ExecuteScalar());

			if (coins < Constants.PackageCost)
			{
				return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostTransaction_403);
			}
		}

		using (NpgsqlCommand command = new($@"SELECT COUNT(*) FROM packages;", dbConnection))
		{
			int entries = Convert.ToInt32(command.ExecuteScalar());

			if (entries <= 0)
			{
				return string.Format(Text.HttpResponse_404_NotFound, Text.Description_PostTransaction_404);
			}
		}

		using var transaction = dbConnection.BeginTransaction();
		List<ICard> cardList = new();

		try
		{
			using (var command = new NpgsqlCommand())
			{
				command.Connection = dbConnection;
				command.Transaction = transaction;

				command.CommandText = "SELECT id FROM packages LIMIT 1;";
				int packageID = Convert.ToInt32(command.ExecuteScalar());

				command.CommandText = "SELECT id FROM users WHERE username = @username";
				command.Parameters.AddWithValue("username", username);
				int userId = (int)command.ExecuteScalar();

				command.CommandText = @"SELECT cards.id, cards.name, cards.damage
						FROM packages
						JOIN cards ON packages.cardid = cards.id
						WHERE packages.id = @id;";
				command.Parameters.Clear();
				command.Parameters.AddWithValue("id", packageID);

				using (NpgsqlDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						ICard card = Cards.Card.CreateInstance(reader.GetGuid(0), reader.GetString(1), (float)reader.GetDouble(2));

						if (card is null)
						{
							continue;
						}

						cardList.Add(card);
					}
				}

				command.CommandText = "INSERT INTO stacks (userid, cardid) SELECT @userId, cardid FROM packages WHERE id = @packageId";
				command.Parameters.Clear();
				command.Parameters.AddWithValue("userId", userId);
				command.Parameters.AddWithValue("packageId", packageID);
				command.ExecuteNonQuery();

				command.CommandText = "DELETE FROM packages WHERE id = @packageId;";
				command.Parameters.Clear();
				command.Parameters.AddWithValue("packageId", packageID);
				command.ExecuteNonQuery();

				command.CommandText = "UPDATE users SET coins = coins - @price WHERE id = @userId;";
				command.Parameters.Clear();
				command.Parameters.AddWithValue("price", Constants.PackageCost);
				command.Parameters.AddWithValue("userId", userId);
				command.ExecuteNonQuery();
			}

			transaction.Commit();
			Console.WriteLine("Transaction committed successfully");
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
			return Text.HttpResponse_500_InternalServerError;
		}

		dbConnection.Close();

		string cardsJson = JsonConvert.SerializeObject(cardList, Formatting.Indented);
		return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_PostTransaction_200, cardsJson);
	}

	public string CreateTrade(TradingDeal trade, string username)
	{
		using var dbConnection = GetDbConnection();
		dbConnection.Open();

		using var transaction = dbConnection.BeginTransaction();

		try
		{
			if (DoesDealIdAlreadyExist(trade.Id))
			{
				return string.Format(Text.HttpResponse_409_Conflict, Text.Description_PostTrading_409);
			}

			if (!DoesCardBelongToUser(trade.CardToTrade, username))
			{
				return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostTrading_403);
			}

			if (IsCardInUserDeck(trade.CardToTrade, username))
			{
				return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostTrading_403);
			}

			using (NpgsqlCommand command = new())
			{
				command.Connection = dbConnection;
				command.Transaction = transaction;

				command.CommandText = @"INSERT INTO trades (id, cardid, type, minimumDamage) VALUES (@id, @cardid, @type, @minimumDamage)";
				command.Parameters.AddWithValue("id", trade.Id);
				command.Parameters.AddWithValue("cardid", trade.CardToTrade);
				command.Parameters.AddWithValue("type", trade.Type);
				command.Parameters.AddWithValue("minimumDamage", trade.MinimumDamage);

				try
				{
					command.ExecuteNonQuery();
				}
				catch (PostgresException ex)
				{
					// not in api-specification
					// cardid needs to be unique -> no two trades available for one card
					if (ex.SqlState == "23505")
					{
						dbConnection.Close();
						return string.Format(Text.HttpResponse_409_Conflict, Text.Description_PostTrading_409_Custom);
					}
				}
			}

			transaction.Commit();
			Console.WriteLine("Transaction committed successfully");
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
			return Text.HttpResponse_500_InternalServerError;
		}

		dbConnection.Close();
		return string.Format(Text.HttpResponse_201_Created, Text.Description_PostTrading_201);
	}

	public void CreateSession(UserCredentials user, out string authToken)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new(@"SELECT COUNT(*) FROM sessions WHERE username = @username;", dbConnection);
		command.Parameters.AddWithValue("username", user.Username);
		int count = Convert.ToInt32(command.ExecuteScalar());

		// token generation would be here
		// foundation for it implemented with 'out' parameters
		authToken = $"{user.Username}-mtcgToken";

		if (count <= 0)
		{
			using NpgsqlCommand insertStatement = new("INSERT INTO sessions (username, token, valid_until) VALUES (@username, @token, @validUntil);", dbConnection);

			insertStatement.Parameters.AddWithValue("username", user.Username);
			insertStatement.Parameters.AddWithValue("token", authToken);
			insertStatement.Parameters.AddWithValue("validUntil", DateTime.Now.AddMinutes(Constants.SessionTimeoutInMinutes));

			insertStatement.ExecuteNonQuery();
			return;
		}

		using NpgsqlCommand updateStatement = new($@"UPDATE sessions SET token = @token, valid_until = @validUntil WHERE username = @username;", dbConnection);

		updateStatement.Parameters.AddWithValue("token", authToken);
		updateStatement.Parameters.AddWithValue("validUntil", DateTime.Now.AddMinutes(Constants.SessionTimeoutInMinutes));
		updateStatement.Parameters.AddWithValue("username", user.Username);

		updateStatement.ExecuteNonQuery();

		dbConnection.Close();
	}

	public int DoUserAndPasswordExist(UserCredentials user)
	{
		var dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new(@"SELECT COUNT(*) FROM users WHERE username = @username AND password = @password;", dbConnection);
		command.Parameters.AddWithValue("username", user.Username);
		command.Parameters.AddWithValue("password", user.Password);
		int count = Convert.ToInt32(command.ExecuteScalar());

		dbConnection.Close();
		return count;
	}

	public string CreatePackage(List<Database.Schemas.Card> package, string authToken)
	{
		var dbConnection = GetDbConnection();
		dbConnection.Open();

		if (DoCardsAlreadyExist(package))
		{
			dbConnection.Close();
			return string.Format(Text.HttpResponse_409_Conflict, Text.Description_PostPackage_409);
		}

		if (!IsAdmin(authToken))
		{
			dbConnection.Close();
			return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostPackage_403);
		}

		if (!IsTokenValid(authToken))
		{
			dbConnection.Close();
			return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
		}

		using var transaction = dbConnection.BeginTransaction();

		try
		{
			using (NpgsqlCommand command = new())
			{
				command.Connection = dbConnection;
				command.Transaction = transaction;

				command.CommandText = @"SELECT nextval('packageids')";
				int packageId = Convert.ToInt32(command.ExecuteScalar());

				command.CommandText = @"INSERT INTO packages (id, cardid) VALUES (@id, @cardid)";

				foreach (Database.Schemas.Card card in package)
				{
					command.Parameters.Clear();
					command.Parameters.AddWithValue("id", packageId);
					command.Parameters.AddWithValue("cardid", card.Id);
					command.ExecuteNonQuery();
				}


				foreach (var card in package)
				{
					command.CommandText = $@"INSERT INTO cards (id, name, cardtype, element, damage) VALUES (@id, @name, @cardtype, @element, @damage);";

					command.Parameters.Clear();
					command.Parameters.AddWithValue("id", card.Id);
					command.Parameters.AddWithValue("name", card.Name);
					command.Parameters.AddWithValue("cardtype", (int)Cards.Card.GetCardTypeByName(card.Name));
					command.Parameters.AddWithValue("element", (int)Cards.Card.GetElementTypeByName(card.Name));
					command.Parameters.AddWithValue("damage", card.Damage);

					command.ExecuteNonQuery();
				}



				//InsertIntoCardsTable(package);
			}

			transaction.Commit();
			Console.WriteLine("Transaction committed successfully");
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
			return Text.HttpResponse_500_InternalServerError;
		}

		dbConnection.Close();
		return string.Format(Text.HttpResponse_201_Created, Text.Description_PostPackage_201);
	}

	public bool DoCardsAlreadyExist(List<Database.Schemas.Card> package)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM cards WHERE id IN (@id1, @id2, @id3, @id4, @id5);", dbConnection);

		command.Parameters.AddWithValue("id1", package[0].Id);
		command.Parameters.AddWithValue("id2", package[1].Id);
		command.Parameters.AddWithValue("id3", package[2].Id);
		command.Parameters.AddWithValue("id4", package[3].Id);
		command.Parameters.AddWithValue("id5", package[4].Id);

		int count = Convert.ToInt32(command.ExecuteScalar());

		dbConnection.Close();
		return count > 0;
	}

	public bool IsAdmin(string authToken)
	{
		var dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new($@"SELECT username FROM sessions WHERE token = '{authToken}';", dbConnection);
		string username = (string)command.ExecuteScalar();

		if (username != "admin")
		{
			dbConnection.Close();
			return false;
		}

		dbConnection.Close();
		return true;
	}

	// TODO: remove
	public void InsertIntoCardsTable(List<Database.Schemas.Card> package)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = dbConnection.CreateCommand();
		foreach (var card in package)
		{
			command.CommandText = $@"INSERT INTO cards (id, name, cardtype, element, damage) VALUES (@id, @name, @cardtype, @element, @damage);";

			command.Parameters.AddWithValue("id", card.Id);
			command.Parameters.AddWithValue("name", card.Name);
			command.Parameters.AddWithValue("cardtype", (int)Cards.Card.GetCardTypeByName(card.Name));
			command.Parameters.AddWithValue("element", (int)Cards.Card.GetElementTypeByName(card.Name));
			command.Parameters.AddWithValue("damage", card.Damage);

			command.ExecuteNonQuery();
		}

		dbConnection.Open();
	}

	public UserStats? RetrieveUserStats(string username)
	{
		UserStats? stats = null;

		var dbConnection = GetDbConnection();
		dbConnection.Open();

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
		return stats;
	}

	public string RetrieveScoreboard(string username)
	{
		var dbConnection = GetDbConnection();
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

		return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetScoreboard_200, JsonConvert.SerializeObject(statList, Formatting.Indented));
	}

	public string DeleteTrade(Guid tradeId, string username)
	{
		var dbConnection = GetDbConnection();
		dbConnection.Open();

		// TODO: snapshot?
		using (var transaction = dbConnection.BeginTransaction())
		{
			try
			{
				if (!DoesDealIdAlreadyExist(tradeId))
				{
					return string.Format(Text.HttpResponse_404_NotFound, Text.Description_DeleteTrading_404);
				}

				Guid cardId = (Guid)RetrieveCardidFromTradeid(tradeId)!;

				if (!DoesCardBelongToUser(cardId, username))
				{
					return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_DeleteTrading_403);
				}

				using (NpgsqlCommand command = new())
				{
					command.Connection = dbConnection;

					command.CommandText = @"DELETE FROM trades
							WHERE id = @id
							AND cardid IN (SELECT cardid FROM stacks
							WHERE userid IN (SELECT id FROM users
							WHERE username = @username));";

					command.Parameters.AddWithValue("id", tradeId);
					command.Parameters.AddWithValue("username", username);
					command.ExecuteNonQuery();
				}

				transaction.Commit();
				Console.WriteLine("Transaction committed successfully");
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
				return Text.HttpResponse_500_InternalServerError;
			}
		}

		dbConnection.Close();
		return string.Format(Text.HttpResponse_200_OK, Text.Description_DeleteTrading_200);
	}

	public string ExecuteTrade(Guid tradeId, Guid offeredCardId, string username)
	{
		var dbConnection = GetDbConnection();
		dbConnection.Open();

		// TODO: snapshot?
		using var transaction = dbConnection.BeginTransaction();

		try
		{
			using (var command = new NpgsqlCommand())
			{
				command.Connection = dbConnection;
				command.Transaction = transaction;

				command.CommandText = "SELECT userid FROM stacks WHERE cardid = (SELECT cardid FROM trades WHERE id = @tradeId);";
				command.Parameters.AddWithValue("tradeId", tradeId);
				int tradeUserId = Convert.ToInt32(command.ExecuteScalar());

				command.Parameters.Clear();
				command.CommandText = "(SELECT id FROM users WHERE username = @username);";
				command.Parameters.AddWithValue("username", username);
				int userId = Convert.ToInt32(command.ExecuteScalar());

				if (tradeUserId == userId)
				{
					transaction.Rollback();
					return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostTrading_CarryOutTrade_403_TradeWithSelf);
				}

				command.Parameters.Clear();
				command.CommandText = "UPDATE stacks SET userid = @userid WHERE cardid = (SELECT cardid FROM trades WHERE id = @tradeId);";
				command.Parameters.AddWithValue("userid", userId);
				command.Parameters.AddWithValue("tradeId", tradeId);
				command.ExecuteNonQuery();

				command.Parameters.Clear();
				command.CommandText = "UPDATE stacks SET userid = @tradeUserId WHERE cardid = @offeredCardId;";
				command.Parameters.AddWithValue("tradeUserId", tradeUserId);
				command.Parameters.AddWithValue("offeredCardId", offeredCardId);
				command.ExecuteNonQuery();

				command.Parameters.Clear();
				command.CommandText = "DELETE FROM trades WHERE id = @tradeId;";
				command.Parameters.AddWithValue("tradeId", tradeId);
				command.ExecuteNonQuery();
			}

			transaction.Commit();
			Console.WriteLine("Transaction committed successfully");
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
			return Text.HttpResponse_500_InternalServerError;
		}

		dbConnection.Close();
		return string.Format(Text.HttpResponse_200_OK, Text.Description_PostTrading_CarryOutTrade_200);
	}

	public Tuple<string, float> GetTradeRequirements(Guid tradeId)
	{
		var dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new("SELECT type, minimumDamage FROM trades WHERE id = @tradeId;", dbConnection);
		command.Parameters.AddWithValue("tradeId", tradeId);

		using NpgsqlDataReader reader = command.ExecuteReader();
		reader.Read();

		Tuple<string, float> requiredStats = new(reader.GetString(0), (float)reader.GetDouble(1));

		dbConnection.Close();
		return requiredStats;
	}

	public Tuple<string, float> GetCardRequirements(Guid cardId)
	{
		var dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new("SELECT cardtype, damage FROM cards WHERE id = @cardId;", dbConnection);
		command.Parameters.AddWithValue("cardId", cardId);

		using NpgsqlDataReader reader = command.ExecuteReader();
		reader.Read();

		string type = (CardType)reader.GetInt32(0) == CardType.Spell ? "spell" : "monster";
		Tuple<string, float> offeredStats = new(type, (float)reader.GetDouble(1));

		dbConnection.Close();
		return offeredStats;
	}

	public bool CampfireCardLoss(Guid cardId)
	{
		using NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = dbConnection.CreateCommand();
		using var transaction = dbConnection.BeginTransaction();
		try
		{
			command.Transaction = transaction;

			command.CommandText = "DELETE FROM stacks WHERE cardid = @cardId;";
			command.Parameters.AddWithValue("cardId", cardId);
			command.ExecuteNonQuery();

			command.Parameters.Clear();
			command.CommandText = "DELETE FROM cards WHERE id = @cardId;";
			command.Parameters.AddWithValue("cardId", cardId);
			command.ExecuteNonQuery();

			transaction.Commit();
			Console.WriteLine("Transaction committed successfully");
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
			return false;
		}

		dbConnection.Close();
		return true;
	}

	public UserData RetrieveUserData(string username)
	{
		var dbConnection = GetDbConnection();
		dbConnection.Open();

		NpgsqlCommand command = new("SELECT name, bio, image FROM userdata INNER JOIN users ON users.id = userdata.userid WHERE users.username = @username;", dbConnection);
		command.Parameters.AddWithValue("username", username);

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

		return userData;
	}

	public void CampfireUpgrade(Guid cardId)
	{
		using NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = dbConnection.CreateCommand();
		command.CommandText = "UPDATE cards SET damage = damage + @increase WHERE id = @cardId;";
		command.Parameters.AddWithValue("increase", Constants.CampfireDamageIncrease);
		command.Parameters.AddWithValue("cardId", cardId);
		command.ExecuteNonQuery();

		dbConnection.Close();
	}

	public bool DoesUserExist(string username)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
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

	public bool DoesCardBelongToUser(Guid cardid, string username)
	{
		using NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM stacks INNER JOIN users ON stacks.userid = users.id WHERE users.username = @username AND stacks.cardid = @id;", dbConnection);
		command.Parameters.AddWithValue("username", username);
		command.Parameters.AddWithValue("id", cardid);
		int count = Convert.ToInt32(command.ExecuteScalar());

		dbConnection.Close();
		return count > 0;
	}

	public bool DoesDealIdAlreadyExist(Guid tradeId)
	{
		using NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM trades WHERE id = @id;", dbConnection);
		command.Parameters.AddWithValue("id", tradeId);
		int count = Convert.ToInt32(command.ExecuteScalar());

		dbConnection.Close();
		return count > 0;
	}

	public bool IsCardInUserDeck(Guid cardid, string username)
	{
		using NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM decks INNER JOIN users ON decks.userid = users.id WHERE users.username = @username AND decks.cardid = @id;", dbConnection);
		command.Parameters.AddWithValue("username", username);
		command.Parameters.AddWithValue("id", cardid);
		int count = Convert.ToInt32(command.ExecuteScalar());

		dbConnection.Close();
		return count > 0;
	}

	public Guid? RetrieveCardidFromTradeid(Guid tradeId)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
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

	public bool IsCardEngagedInTrade(Guid cardid)
	{
		using NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new($@"SELECT COUNT(*) FROM trades WHERE cardid = @id;", dbConnection);
		command.Parameters.AddWithValue("id", cardid);
		int count = Convert.ToInt32(command.ExecuteScalar());

		dbConnection.Close();
		return count > 0;
	}

	public bool IsTokenValid(string authToken)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new("SELECT valid_until FROM sessions WHERE token = @token;", dbConnection);
		command.Parameters.AddWithValue("token", authToken);
		object? result = command.ExecuteScalar();

		if (result is null)
		{
			dbConnection.Close();
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

	public string RetrieveUsernameFromToken(string authToken)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		using NpgsqlCommand command = new($@"SELECT username FROM sessions WHERE token = @token;", dbConnection);
		command.Parameters.AddWithValue("token", authToken);

		return (string)command.ExecuteScalar();
	}

	public List<ICard> RetrieveUserCards(string userName, string tableName)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

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
			ICard? card = Cards.Card.CreateInstance(reader.GetGuid(0), reader.GetString(1), (float)reader.GetDouble(2));

			if (card is null)
			{
				continue;
			}

			cardList.Add(card);
		}

		dbConnection.Close();
		return cardList;
	}

	public bool TableExists(string tableName)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		// (googled) check if table exists by using select statement
		using NpgsqlCommand command = new($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}')", dbConnection);
		object? obj = command.ExecuteScalar();
		dbConnection.Close();

		return (bool)obj!;
	}

	// 'create' and 'insert' statements for start of program
	// @"" ... verbatim string (escape sequences interpreted as literal characters)
	public readonly Dictionary<string, (string, string?)> dbObjectInitCommands = new()
	{
		{
			"cardtypes",
			(
				@"CREATE TABLE IF NOT EXISTS cardtypes (
						id INTEGER PRIMARY KEY,
						type VARCHAR(25)
					);",

				@"INSERT INTO cardtypes (id, type) VALUES
						(1, 'Spell'),
						(2, 'Goblin'),
						(3, 'Dragon'),
						(4, 'Wizard'),
						(5, 'Ork'),
						(6, 'Knight'),
						(7, 'Kraken'),
						(8, 'Elf'),
						(9, 'Troll');"
			)
		},

		{
			"elements",
			(
				@"CREATE TABLE IF NOT EXISTS elements (
						id INTEGER PRIMARY KEY,
						element VARCHAR(25)
					);",

				@"INSERT INTO elements (id, element) VALUES
						(1, 'Fire'),
						(2, 'Water'),
						(3, 'Regular');"
			)
		},

		{
			"cardcategories",
			(
				@"CREATE TABLE IF NOT EXISTS cardcategories (
						id SERIAL PRIMARY KEY,
						name VARCHAR(50) UNIQUE,
						cardtype INTEGER REFERENCES cardtypes(id),
						element INTEGER REFERENCES elements(id),
						CHECK (cardtype BETWEEN 1 AND 9),
						CHECK (element BETWEEN 1 AND 3)
					);",

				@"INSERT INTO cardcategories (name, cardtype, element) VALUES
					('FireSpell', 1, 1),
					('WaterSpell', 1, 2),
					('RegularSpell', 1, 3),
					('FireGoblin', 2, 1),
					('WaterGoblin', 2, 2),
					('RegularGoblin', 2, 3),
					('Dragon', 3, 1),
					('Wizard', 4, 3),
					('Ork', 5, 3),
					('Knight', 6, 3),
					('Kraken', 7, 2),
					('FireElf', 8, 1),
					('WaterElf', 8, 2),
					('RegularElf', 8, 3),
					('FireTroll', 9, 1),
					('WaterTroll', 9, 2),
					('RegularTroll', 9, 3);"
			)
		},

		{
			"packageids",
			(
				@"CREATE SEQUENCE IF NOT EXISTS packageids;",

				null
			)
		},

		{
			"users",
			(
				@"CREATE TABLE IF NOT EXISTS users (
						id SERIAL PRIMARY KEY,
						username VARCHAR(50) UNIQUE,
						password VARCHAR(255),
						coins INTEGER
					);",

				null
			)
		},

		{
			"userdata",
			(
				@"CREATE TABLE IF NOT EXISTS userdata (
						userid INTEGER PRIMARY KEY,
						name VARCHAR(50),
						image VARCHAR(50),
						bio VARCHAR(255)
					);",

				null
			)
		},

		{
			"userstats",
			(
				@"CREATE TABLE IF NOT EXISTS userstats (
						userid INTEGER PRIMARY KEY,
						elo INTEGER,
						wins INTEGER,
						losses INTEGER
					);",

				null
			)
		},

		{
			"packages",
			(
				@"CREATE TABLE IF NOT EXISTS packages (
						id INTEGER,
						cardid uuid
					);",

				null
			)
		},

		{
			"trades",
			(
				@"CREATE TABLE IF NOT EXISTS trades (
						id uuid PRIMARY KEY,
						cardid uuid UNIQUE,
						type VARCHAR(20),
						minimumDamage FLOAT
					);",

				null
			)
		},

		{
			"sessions",
			(
				@"CREATE TABLE IF NOT EXISTS sessions (
						id SERIAL PRIMARY KEY,
						username VARCHAR(50),
						token varchar(100),
						valid_until TIMESTAMP
					);",

				null
			)
		},

		{
			"cards",
			(
				@"CREATE TABLE IF NOT EXISTS cards (
						id uuid PRIMARY KEY,
						name VARCHAR(50),
						cardtype INTEGER REFERENCES cardtypes(id),
						element INTEGER REFERENCES elements(id),
						damage FLOAT,
						CHECK (cardtype BETWEEN 1 AND 9),
						CHECK (element BETWEEN 1 AND 3)
					);",

				null
			)
		},

		{
			"stacks",
			(
				@"CREATE TABLE IF NOT EXISTS stacks (
						id SERIAL PRIMARY KEY,
						userid INTEGER,
						cardid uuid UNIQUE
					);",

				null
			)
		},

		{
			"decks",
			(
				@"CREATE TABLE IF NOT EXISTS decks (
						id SERIAL PRIMARY KEY,
						userid INTEGER,
						cardid uuid UNIQUE
					);",

				null
			)
		}
	};

	public void CreateDbObject(string tableName, string createStatement)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();
		using NpgsqlCommand command = new(createStatement, dbConnection);
		command.ExecuteNonQuery();
		Console.WriteLine($"Database Object '{tableName}' created successfully.");
		dbConnection.Close();
	}

	public void InsertValues(string tableName, string insertStatement)
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();
		using NpgsqlCommand command = new(insertStatement, dbConnection);
		command.ExecuteNonQuery();
		Console.WriteLine($"Values inserted into '{tableName}' successfully.");
		dbConnection.Close();
	}

	public List<TradingDeal> RetrieveTrades()
	{
		NpgsqlConnection dbConnection = GetDbConnection();
		dbConnection.Open();

		List<TradingDeal> tradesList = new();

		using NpgsqlCommand command = new("SELECT * FROM trades;", dbConnection);
		using NpgsqlDataReader reader = command.ExecuteReader();

		while (reader.Read())
		{
			TradingDeal trade = new()
			{
				Id = reader.GetGuid(0),
				CardToTrade = reader.GetGuid(1),
				Type = reader.GetString(2),
				MinimumDamage = (float)reader.GetDouble(3)
			};

			tradesList.Add(trade);
		}

		dbConnection.Close();
		return tradesList;
	}
}
