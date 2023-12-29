using MTCG.Cards;
using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using Newtonsoft.Json;
using Npgsql;

namespace MTCG.Server.HttpRequests
{
	internal class PutDeck : IHttpRequest
	{
		public string GetResponse(string request)
		{
			string jsonPayload = HttpRequestUtility.ExtractJsonPayload(request);

			if (jsonPayload == null)
			{
				return Text.Res_400_BadRequest;
			}

			List<Guid>? cardIds = JsonConvert.DeserializeObject<List<Guid>>(jsonPayload);

			if (cardIds == null)
			{
				return Text.Res_400_BadRequest;
			}

			if (cardIds.Count != 4)
			{
				return Text.Res_PutDeck_400;
			}

			string response;
			try
			{
				response = ConfigureDeck(cardIds, HttpRequestUtility.ExtractBearerToken(request));
			}
			catch (InvalidOperationException)
			{
				return Text.Res_401_Unauthorized;
			}

			return response;
		}

		private static string ConfigureDeck(List<Guid> cardIds, string authToken)
		{
			if (!HttpRequestUtility.IsTokenValid(authToken))
			{
				return Text.Res_401_Unauthorized;
			}

			var dbConnection = DBManager.GetDBConnection();
			dbConnection.Open();

			string username = HttpRequestUtility.RetrieveUsernameFromToken(authToken);
			using (NpgsqlCommand command = new(@"SELECT COUNT(*) FROM stacks JOIN users ON stacks.userid = users.id WHERE users.username = @username AND stacks.cardid = ANY(@ids);", dbConnection))
			{
				command.Parameters.AddWithValue("username", username);
				command.Parameters.AddWithValue("ids", cardIds.ToArray());
				int count = Convert.ToInt32(command.ExecuteScalar());

				if (count != 4)
				{
					return Text.Res_PutDeck_403;
				}
			}

			dbConnection.Close();
			return String.Empty;
		}







		private static void InsertIntoCardsTable(List<Database.Schemas.Card> package, NpgsqlCommand command)
		{
			foreach (var card in package)
			{
				command.CommandText = $@"INSERT INTO cards (id, name, cardtype, element, damage) VALUES (@id, @name, @cardtype, @element, @damage);";

				command.Parameters.Clear();
				command.Parameters.AddWithValue("id", card.Id);
				command.Parameters.AddWithValue("name", card.Name);
				command.Parameters.AddWithValue("cardtype", (int)Cards.Card.GetCardTypeByName(card.Name));
				command.Parameters.AddWithValue("element", (int)Cards.Card.GetElementTypeByName(card.Name));
				command.Parameters.AddWithValue("damage", card.Damage);

				command.ExecuteNonQuery();

				Console.WriteLine("Card inserted successfully");
			}
		}
	}
}
