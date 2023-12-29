﻿using MTCG.Cards;
using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server.HttpRequests
{
	public class PostTransaction : IHttpRequest
	{
		public string GetResponse(string request)
		{
			if (HttpRequestUtility.ExtractPathAddOns(request) != "packages")
				return Text.Res_404_NotFound;

			string response;
			try
			{
				response = AquirePackage(HttpRequestUtility.ExtractBearerToken(request));
			}
			catch (InvalidOperationException)
			{
				return Text.Res_401_Unauthorized;
			}

			return response;
		}

		private static string AquirePackage(string authToken)
		{
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				return Text.Res_401_Unauthorized;
			}

			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);
			using (NpgsqlCommand command = new($@"SELECT coins FROM users WHERE username = @user;", dbConnection))
			{
				command.Parameters.AddWithValue("user", username);

				int coins = Convert.ToInt32(command.ExecuteScalar());

				if (coins < Constants.PackageCost)
				{
					return Text.Res_PostTransaction_403;
				}
			}

			using (NpgsqlCommand command = new($@"SELECT COUNT(*) FROM packages;", dbConnection))
			{
				int entries = Convert.ToInt32(command.ExecuteScalar());

				if (entries <= 0)
				{
					return Text.Res_PostTransaction_404;
				}
			}

			using var transaction = dbConnection.BeginTransaction();
			List<Card> cardList = new();

			try
			{
				using (var command = new NpgsqlCommand())
				{
					command.Connection = dbConnection;
					command.Transaction = transaction;

					command.CommandText = "SELECT id FROM packages LIMIT 1;";
					int packageID = Convert.ToInt32(command.ExecuteScalar());

					command.CommandText = "SELECT id FROM users WHERE username = @username";
					command.Parameters.AddWithValue("username", username);
					int userId = (int)command.ExecuteScalar();

					command.CommandText = @"SELECT cards.id, cards.name, cards.damage
						FROM packages
						JOIN cards ON packages.cardid = cards.id
						WHERE packages.id = @id;";
					command.Parameters.Clear();
					command.Parameters.AddWithValue("id", packageID);

					using (NpgsqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							Card card = (Card)Card.CreateInstance(reader.GetGuid(0), reader.GetString(1), (float)reader.GetDouble(2));

							if (card is null)
							{
								continue;
							}

							cardList.Add(card);
						}
					}

					command.CommandText = "INSERT INTO stacks (userid, cardid) SELECT @userId, cardid FROM packages WHERE id = @packageId";
					command.Parameters.Clear();
					command.Parameters.AddWithValue("userId", userId);
					command.Parameters.AddWithValue("packageId", packageID);
					command.ExecuteNonQuery();

					command.CommandText = "DELETE FROM packages WHERE id = @packageId;";
					command.Parameters.Clear();
					command.Parameters.AddWithValue("packageId", packageID);
					command.ExecuteNonQuery();

					command.CommandText = "UPDATE users SET coins = coins - @price WHERE id = @userId;";
					command.Parameters.Clear();
					command.Parameters.AddWithValue("price", Constants.PackageCost);
					command.Parameters.AddWithValue("userId", userId);
					command.ExecuteNonQuery();
				}

				transaction.Commit();
				Console.WriteLine("Transaction committed successfully");
			}
			catch (Exception ex)
			{
				transaction.Rollback();
				Console.WriteLine("Transaction rolled back due to exception: " + ex.Message);
				return Text.Res_500_ServerError;
			}

			dbConnection.Close();

			string cardsJson = JsonConvert.SerializeObject(cardList, Formatting.Indented);
			return String.Format(Text.Res_PostTransaction_200, cardsJson);
		}
	}
}