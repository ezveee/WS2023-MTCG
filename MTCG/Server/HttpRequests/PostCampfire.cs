using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class PostCampfire : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request, out string? authToken))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken!);
			Guid cardId = new(HttpRequestUtility.ExtractJsonPayload(request)); ;

			if (!HttpRequestUtility.DoesCardBelongToUser(cardId, username)
				|| HttpRequestUtility.IsCardInUserDeck(cardId, username)
				|| HttpRequestUtility.IsCardEngagedInTrade(cardId))
			{
				return string.Format(Text.HttpResponse_403_Forbidden, Text.Description_PostCampfire_403);
			}

			return UpgradeCard(cardId);
		}

		private static string UpgradeCard(Guid cardId)
		{
			using var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			using NpgsqlCommand command = dbConnection.CreateCommand();
			if (DidUpgradeSucceed())
			{
				command.CommandText = "UPDATE cards SET damage = damage + @increase WHERE id = @cardId;";
				command.Parameters.AddWithValue("increase", Constants.CampfireDamageIncrease);
				command.Parameters.AddWithValue("cardId", cardId);
				command.ExecuteNonQuery();

				dbConnection.Close();
				return string.Format(Text.HttpResponse_200_OK, Text.Description_PostCampfire_200_Success);
			}

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
				return Text.HttpResponse_500_InternalServerError;
			}

			dbConnection.Close();
			return string.Format(Text.HttpResponse_200_OK, Text.Description_PostCampfire_200_Fail);
		}

		private static bool DidUpgradeSucceed()
		{
			Random random = new();
			float randomNumber = (float)random.NextDouble();

			return randomNumber < Constants.CampfireOdds;
		}
	}
}
