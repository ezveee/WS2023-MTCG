using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server.HttpRequests
{
	public class PostTrading : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request, out string? authToken))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken!);

			string? tradeId = HttpRequestUtility.ExtractPathAddOns(request);
			if (tradeId is not null)
			{
				return new CarryOutTrade(tradeId, username).GetResponse(request);
			}

			TradingDeal trade;
			try
			{
				trade = HttpRequestUtility.DeserializeJson<TradingDeal>(request);
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is JsonSerializationException)
			{
				return Text.HttpResponse_400_BadRequest;
			}

			return CreateTrade(trade, username);
		}

		private static string CreateTrade(TradingDeal trade, string username)
		{
			using var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using var transaction = dbConnection.BeginTransaction();

			try
			{
				if (HttpRequestUtility.DoesDealIdAlreadyExist(trade.Id))
				{
					return string.Format(Text.HttpResponse_409_Conflict, Text.Description_PostTrading_409);
				}

				if (!HttpRequestUtility.DoesCardBelongToUser(trade.CardToTrade, username))
				{
					return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostTrading_403);
				}

				if (HttpRequestUtility.IsCardInUserDeck(trade.CardToTrade, username))
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
	}
}
