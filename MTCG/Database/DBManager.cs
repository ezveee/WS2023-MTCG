//using MTCG.Interfaces;
//using Npgsql;

//namespace MTCG.Database;

//public class DBManager
//{
//	readonly IDataAccess _dataAccess;

//	public DBManager(IDataAccess dataAccess)
//	{
//		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));

//		Console.WriteLine("DB setup start.");
//		Setup();
//		Console.WriteLine("DB setup finished.\n");
//	}

//	private static readonly string connectionString = "Host=localhost;Username=vee;Password=1234";
//	private readonly NpgsqlConnection connection = new(connectionString);

//	private void Setup()
//	{
//		connection.Open();

//		NpgsqlConnection dbConnection = GetDbConnection();
//		dbConnection.Open();

//		foreach (KeyValuePair<string, (string, string?)> entry in dbObjectInitCommands)
//		{
//			InitDbObject(dbConnection, entry.Key, entry.Value.Item1, entry.Value.Item2);
//		}

//		dbConnection.Close();
//	}

//	public static NpgsqlConnection GetDbConnection()
//	{
//		return new NpgsqlConnection(connectionString + "; Database=mtcg");
//	}

//	public void CloseConnection()
//	{
//		connection?.Close();
//	}

//	// initial db object creation
//	private void InitDbObject(NpgsqlConnection connection, string tableName, string createStatement, string? insertStatement)
//	{
//		bool tableExists = _dataAccess.TableExists(connection, tableName);

//		if (tableExists)
//		{
//			Console.WriteLine($"Database object '{tableName}' already exists.");
//			return;
//		}

//		_dataAccess.CreateDbObject(connection, tableName, createStatement);
//		if (insertStatement != null)
//		{
//			_dataAccess.InsertValues(connection, tableName, insertStatement);
//		}
//	}
//}
