using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	public class DeleteTrading : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (!HttpRequestUtility.IsUserAccessValid(request, out string? authToken))
			{
				return string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401);
			}

			Guid tradeId;
			try
			{
				tradeId = new(HttpRequestUtility.ExtractPathAddOns(request));
			}
			catch
			{
				// TODO: maybe change to just text. ... cause no description is needed
				return string.Format(Text.HttpResponse_400_BadRequest);
			}

			return DeleteTrade(tradeId, HttpRequestUtility.RetrieveUsernameFromToken(authToken!));
		}

		private static string DeleteTrade(Guid tradeId, string username)
		{
			var dbConnection = DBManager.GetDbConnection();
			dbConnection.Open();

			// TODO: snapshot?
			using (var transaction = dbConnection.BeginTransaction())
			{
				try
				{
					if (!HttpRequestUtility.DoesDealIdAlreadyExist(tradeId))
					{
						return string.Format(Text.HttpResponse_404_NotFound, Text.Description_DeleteTrading_404);
					}

					Guid cardId = (Guid)HttpRequestUtility.RetrieveCardidFromTradeid(tradeId)!;

					if (!HttpRequestUtility.DoesCardBelongToUser(cardId, username))
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
	}
}
