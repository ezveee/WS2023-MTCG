using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Database
{
	internal class DBManager
	{
		private static DBManager _instance;

		private DBManager()
		{
			Console.WriteLine("DB setup start.");
			Setup();
			Console.WriteLine("DB setup finished.\n");
		}

		public static DBManager Instance
		{
			get
			{
				_instance ??= new DBManager();
				return _instance;
			}
		}

		private static readonly string connectionString = "Host=localhost;Username=vee;Password=1234";
		private readonly NpgsqlConnection connection = new(connectionString);

		// 'create' and 'insert' statements for start of program
		// @"" ... verbatim string (escape sequences interpreted as literal characters)
		private readonly Dictionary<string, (string, string?)> dbObjectInitCommands = new()
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
					('RegularSpell', 1, 2),
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
						coins INTEGER,
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
						cardid uuid UNIQUE,
						description VARCHAR(200)
					);",

					null
				)
			}
		};

		private void Setup()
		{
			connection.Open();

			var dbConnection = GetDBConnection();
			dbConnection.Open();

			foreach (var entry in dbObjectInitCommands)
				InitDbObject(dbConnection, entry.Key, entry.Value.Item1, entry.Value.Item2);

			dbConnection.Close();
		}

		public static NpgsqlConnection GetDBConnection()
		{
			return new NpgsqlConnection(connectionString + "; Database=mtcg");
		}

		public void CloseConnection()
		{
			connection?.Close();
		}

		// initial db object creation
		static void InitDbObject(NpgsqlConnection connection, string tableName, string createStatement, string? insertStatement)
		{
			bool tableExists = TableExists(connection, tableName);

			if (tableExists)
			{
				Console.WriteLine($"Database object '{tableName}' already exists.");
				return;
			}

			CreateDbObject(connection, tableName, createStatement);
			if (insertStatement != null)
			{
				InsertValues(connection, tableName, insertStatement);
			}
		}

		static bool TableExists(NpgsqlConnection connection, string tableName)
		{
			// (googled) check if table exists by using select statement
			using NpgsqlCommand command = new($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}')", connection);
			return (bool)command.ExecuteScalar();
		}

		static void CreateDbObject(NpgsqlConnection connection, string tableName, string createStatement)
		{
			using NpgsqlCommand command = new(createStatement, connection);
			command.ExecuteNonQuery();
			Console.WriteLine($"Database Object '{tableName}' created successfully.");
		}

		static void InsertValues(NpgsqlConnection connection, string tableName, string insertStatement)
		{
			using NpgsqlCommand command = new(insertStatement, connection);
			command.ExecuteNonQuery();
			Console.WriteLine($"Values inserted into '{tableName}' successfully.");
		}
	}
}
