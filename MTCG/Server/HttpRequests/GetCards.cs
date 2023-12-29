using MTCG.Cards;
using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	internal class GetCards : IHttpRequest
	{
		public string GetResponse(string request)
		{
			string response;
			try
			{
				response = RetrieveCards(HttpRequestUtility.ExtractBearerToken(request));
			}
			catch (InvalidOperationException)
			{
				return Text.Res_401_Unauthorized;
			}

			return response;
		}

		private static string RetrieveCards(string authToken)
		{
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				return Text.Res_401_Unauthorized;
			}

			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);
			List<Card> cardList = new List<Card>();

			using (NpgsqlCommand command = new(
				@"SELECT cards.id,
					cards.name,
					cards.damage
				FROM stacks
				JOIN users ON stacks.userid = users.id
				JOIN cards ON stacks.cardid = cards.id
				WHERE users.username = @user;", dbConnection))
			{
				command.Parameters.AddWithValue("user", username);

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

				if (cardList.Count <= 0)
				{
					return Text.Res_GetCards_204;
				}
			}

			string cardsJson = JsonConvert.SerializeObject(cardList, Formatting.Indented);
			return String.Format(Text.Res_GetCards_200, cardsJson);
		}
	}
}
