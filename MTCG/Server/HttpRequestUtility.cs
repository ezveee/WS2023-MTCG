using MTCG.Database;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server
{
	public static class HttpRequestUtility
	{
		public static readonly Dictionary<string, (int, int)> cardCategories = new()
		{
			{ "FireGoblin", ( 1, 1 ) },
			{ "WaterGoblin", ( 1, 2 ) },
			{ "RegularGoblin", ( 1, 3 ) },
			{ "FireTroll", ( 1, 1 ) },
			{ "WaterTroll", ( 1, 2 ) },
			{ "RegularTroll", ( 1, 3 ) },
			{ "FireElf", ( 1, 1 ) },
			{ "WaterElf", ( 1, 2 ) },
			{ "RegularElf", ( 1, 3 ) },
			{ "FireSpell", ( 2, 1 ) },
			{ "WaterSpell", ( 2, 2 ) },
			{ "RegularSpell", ( 2, 2 ) },
			{ "Knight", ( 1, 3 ) },
			{ "Dragon", ( 1, 1 ) },
			{ "Ork", ( 1, 3 ) },
			{ "Kraken", ( 1, 2 ) }
		};


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
			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			using NpgsqlCommand command = new($@"SELECT valid_until FROM sessions WHERE token = '{authToken}';", dbConnection);
			DateTime validUntil = (DateTime)command.ExecuteScalar();

			if (validUntil < DateTime.Now)
			{
				dbConnection.Close();
				return false;
			}

			dbConnection.Close();
			return true;
		}
	}
}
