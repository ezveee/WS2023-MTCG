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
		private readonly Dictionary<string, (string, string?)> tableInitCommands = new()
		{
			{
				"users",
				(
					@"CREATE TABLE IF NOT EXISTS users (
						id SERIAL PRIMARY KEY,
						username VARCHAR(50) UNIQUE,
						password VARCHAR(255),
						coins INTEGER,
						elo INTEGER
					);",

					null
				)
			},

			{
				"packages",
				(
					@"CREATE TABLE IF NOT EXISTS packages (
						id SERIAL PRIMARY KEY,
						cards INTEGER[],
						CONSTRAINT checkPackageSize CHECK (cardinality(cards) = 5)
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
				"cardtypes",
				(
					@"CREATE TABLE IF NOT EXISTS cardtypes (
						id INTEGER PRIMARY KEY,
						type VARCHAR(25)
					);",

					@"INSERT INTO cardtypes (id, type) VALUES
						(1, 'Monster'),
						(2, 'Spell');"
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
				"cards",
				(
					@"CREATE TABLE IF NOT EXISTS cards (
						id SERIAL PRIMARY KEY,
						name VARCHAR(50) UNIQUE,
						cardtype_f INTEGER REFERENCES cardtypes(id),
						element_f INTEGER REFERENCES elements(id),
						damage FLOAT,
						CHECK (cardtype_f IN (1, 2)),
						CHECK (element_f BETWEEN 1 AND 3)
					);",

					@"INSERT INTO cards (name, cardtype_f, element_f, damage) VALUES
					('FireGoblin', 1, 1, 25.0),
					('WaterGoblin', 1, 2, 25.0),
					('RegularGoblin', 1, 3, 25.0),
					('FireTroll', 1, 1, 27.0),
					('WaterTroll', 1, 2, 27.0),
					('RegularTroll', 1, 3, 27.0),
					('FireElf', 1, 1, 30.0),
					('WaterElf', 1, 2, 30.0),
					('RegularElf', 1, 3, 30.0),
					('FireSpell', 2, 1, 25.0),
					('WaterSpell', 2, 2, 25.0),
					('RegularSpell', 2, 2, 25.0),
					('Knight', 1, 3, 30.0),
					('Dragon', 1, 1, 35.0),
					('Ork', 1, 3, 28.5),
					('Kraken', 1, 2, 35.0);"
				)
			}

		};

		private void Setup()
		{
			connection.Open();

			var dbConnection = GetDBConnection();
			dbConnection.Open();

			foreach (var entry in tableInitCommands)
				InitTable(dbConnection, entry.Key, entry.Value.Item1, entry.Value.Item2);

			dbConnection.Close();
		}

		public static NpgsqlConnection GetDBConnection()
		{
			return new NpgsqlConnection(connectionString + ";Database=mtcg");
		}

		public void CloseConnection()
		{
			connection?.Close();
		}

		// initial table creation
		static void InitTable(NpgsqlConnection connection, string tableName, string createStatement, string insertStatement)
		{
			bool tableExists = TableExists(connection, tableName);

			if (tableExists)
			{
				Console.WriteLine($"Table '{tableName}' already exists.");
				return;
			}

			CreateTable(connection, tableName, createStatement);
			if (insertStatement != null)
				InsertValues(connection, tableName, insertStatement);

		}

		static bool TableExists(NpgsqlConnection connection, string tableName)
		{
			// (googled) check if table exists by using select statement
			using NpgsqlCommand command = new($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}')", connection);
			return (bool)command.ExecuteScalar();
		}

		static void CreateTable(NpgsqlConnection connection, string tableName, string createStatement)
		{
			using NpgsqlCommand command = new(createStatement, connection);
			command.ExecuteNonQuery();
			Console.WriteLine($"Table '{tableName}' created successfully.");
		}

		static void InsertValues(NpgsqlConnection connection, string tableName, string insertStatement)
		{
			using NpgsqlCommand command = new(insertStatement, connection);
			command.ExecuteNonQuery();
			Console.WriteLine($"Values inserted into '{tableName}' successfully.");
		}
	}
}
