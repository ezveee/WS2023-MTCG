using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class GetTradings : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request, out _))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			return RetrieveTrades();
		}

		private static string RetrieveTrades()
		{
			var dbConnection = DBManager.GetDbConnection();
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

			if (tradesList.Count <= 0)
			{
				return string.Format(Text.HttpResponse_204_NoContent, Text.Description_GetTradings_204);
			}

			string tradesJson = JsonConvert.SerializeObject(tradesList, Formatting.Indented);
			return string.Format(Text.HttpResponse_200_OK_WithContent, Text.Description_GetTradings_200, tradesJson);
		}
	}
}
