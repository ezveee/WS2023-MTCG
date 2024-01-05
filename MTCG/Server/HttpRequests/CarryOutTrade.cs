using MTCG.Cards;
using MTCG.Database;
using MTCG.Database.Schemas;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server.HttpRequests
{
	public class CarryOutTrade : IHttpRequest
	{
		private readonly Guid tradeId;
		private readonly string username;

		public CarryOutTrade(string tId, string user)
		{
			tradeId = new Guid(tId);
			username = user;
		}

		public string GetResponse(string request)
		{
			string jsonPayload = HttpRequestUtility.ExtractJsonPayload(request).Trim('\"');
			Guid offeredCardId = new(jsonPayload);

			if (!HttpRequestUtility.DoesDealIdAlreadyExist(tradeId))
			{
				return string.Format(Text.HttpResponse_404_NotFound, Text.Description_PostTrading_CarryOutTrade_404);
			}

			if (!HttpRequestUtility.DoesCardBelongToUser(offeredCardId, username)
				|| HttpRequestUtility.IsCardInUserDeck(offeredCardId, username)
				|| !DoesCardMeetRequirements(offeredCardId))
			{
				return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostTrading_CarryOutTrade_403);
			}

			// TODO: idk man, dk which one looks less shit
			//if (HttpRequestUtility.IsCardInUserDeck(cardId, username))
			//{
			//	return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostTrading_CarryOutTrade_403);
			//}

			//if (!DoesCardMeetRequirements(cardId))
			//{
			//	return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostTrading_CarryOutTrade_403);
			//}

			return ExecuteTrade(offeredCardId);
		}

		private string ExecuteTrade(Guid offeredCardId)
		{
			var dbConnection = DBManager.GetDbConnection();
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

		private bool DoesCardMeetRequirements(Guid offeredCardId)
		{
			Tuple<string, float> requiredStats;
			Tuple<string, float> offeredStats;

			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using (NpgsqlCommand command = new("SELECT type, minimumDamage FROM trades WHERE id = @tradeId;", dbConnection))
			{
				command.Parameters.AddWithValue("tradeId", tradeId);

				using NpgsqlDataReader reader = command.ExecuteReader();

				reader.Read();
				requiredStats = new Tuple<string, float>(reader.GetString(0), (float)reader.GetDouble(1));
			}

			using (NpgsqlCommand command = new("SELECT cardtype, damage FROM cards WHERE id = @cardId;", dbConnection))
			{
				command.Parameters.AddWithValue("cardId", offeredCardId);

				using NpgsqlDataReader reader = command.ExecuteReader();

				reader.Read();

				string type = (CardType)reader.GetInt32(0) == CardType.Spell ? "spell" : "monster";
				offeredStats = new Tuple<string, float>(type, (float)reader.GetDouble(1));
			}

			dbConnection.Close();

			if (offeredStats.Item1 != requiredStats.Item1)
			{
				return false;
			}

			if (offeredStats.Item2 < requiredStats.Item2)
			{
				return false;
			}

			return true;
		}
	}
}