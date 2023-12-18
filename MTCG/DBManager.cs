using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
	internal class DBManager
	{
		private readonly string connectionString = "Host=localhost;Username=vee;Password=1234";

		public void Setup()
		{
			using (NpgsqlConnection connection = new(connectionString))
			{
				connection.Open();

				// database already created
				// establish db connection
				using (NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString + ";Database=mtcg"))
				{
					dbConnection.Open();

					// create tables
					CreateTables(dbConnection);

					// fill tables
					FillTables(dbConnection);

					dbConnection.Close();
				}

				connection.Close();
			}
		}

		interface IInitTable
		{
			static void Create(NpgsqlConnection connection);
			static void Fill(NpgsqlConnection connection);
		}

		class UserTable : IInitTable
		{
			public static void Create(NpgsqlConnection connection)
			{
				string query =
					@"CREATE TABLE IF NOT EXISTS users (
						id SERIAL PRIMARY KEY,
						username VARCHAR(50) UNIQUE,
						password VARCHAR(255),
						coins INTEGER
					);";

				using (NpgsqlCommand command = new(query, connection))
				{
					int affectedRows = command.ExecuteNonQuery();

					if (affectedRows <= 0)
						return;

					Fill(connection);
				}
			}

			public static void Fill(NpgsqlConnection connection)
			{
				string query =
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
					('Kraken', 1, 2, 35.0);";

				using (NpgsqlCommand command = new(query, connection))
					command.ExecuteNonQuery();
			}
		}

		static void CreateTables(NpgsqlConnection connection)
		{
			// @"" ... verbatim string (escape sequences interpreted as literal characters)
			string[] queries =
			{
				@"CREATE TABLE IF NOT EXISTS users (
					id SERIAL PRIMARY KEY,
					username VARCHAR(50) UNIQUE,
					password VARCHAR(255),
					coins INTEGER
				);",

				@"CREATE TABLE IF NOT EXISTS cardtypes (
					id INTEGER PRIMARY KEY,
					type VARCHAR(25)
				);",

				@"CREATE TABLE IF NOT EXISTS elements (
					id INTEGER PRIMARY KEY,
					element VARCHAR(25)
				);",

				@"CREATE TABLE IF NOT EXISTS cards (
					id SERIAL PRIMARY KEY,
					name VARCHAR(50) UNIQUE,
					cardtype_f INTEGER REFERENCES cardtypes(id),
					element_f INTEGER REFERENCES elements(id),
					damage FLOAT,
					CHECK (cardtype_f IN (1, 2)),
					CHECK (element_f BETWEEN 1 AND 3)
				);",

				@"CREATE TABLE IF NOT EXISTS packages (
					id SERIAL PRIMARY KEY,
					cards INTEGER[],
					CONSTRAINT checkPackageSize CHECK (cardinality(cards) = 5)
				);"

				// TODO: stacks & decks
				// one entry per card?
				// TODO: (possibly) rework packages table (based on stack/deck design)

			};

			foreach (string query in queries)
			{
				using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		static void FillTables(NpgsqlConnection connection)
		{
			string[] queries =
			{
				@"INSERT INTO cardtypes (id, type) VALUES
					(1, 'Monster'),
					(2, 'Spell');",

				@"INSERT INTO elements (id, element) VALUES
					(1, 'Fire'),
					(2, 'Water'),
					(3, 'Regular');",

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
			};

			foreach (string query in queries)
			{
				using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
				{
					command.ExecuteNonQuery();
				}
			}
		}
	}
}
