using MTCG.Database;
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
	internal class PostTransaction : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (HttpRequestUtility.ExtractPathAddOns(request) != "packages")
				return Text.Res_404;




			return "";
		}

		private static string AquirePackage(List<Database.Schemas.Card> package, string authToken)
		{
			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			// TODO: make it so that chekcing if token exists is seperate from ExtractBearerToken method
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				dbConnection.Close();
				return Text.Res_PostPackage_401;
			}

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);

			using var transaction = dbConnection.BeginTransaction();

			try
			{
				using (var command = new NpgsqlCommand())
				{
					command.Connection = dbConnection;
					command.Transaction = transaction;

					command.CommandText = "SELECT TOP 1  FROM packages;";
					command.ExecuteNonQuery();








					// Example SQL commands

					command.CommandText = "UPDATE mytable SET column2 = 'newvalue' WHERE column1 = 1";
					command.ExecuteNonQuery();
				}

				transaction.Commit();
				Console.WriteLine("Transaction committed successfully");
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
			}

			return string.Empty;

		}
	}
}
