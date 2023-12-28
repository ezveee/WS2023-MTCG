using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			using (NpgsqlCommand command = new(
				@"SELECT users.id,
					cards.id,
					cards.name,
					cards.cardtype,
					cards.element,
					cards.damage
				FROM stacks
				JOIN users ON stacks.userid = users.id
				JOIN cards ON stacks.cardid = cards.id
				WHERE users.username = @user;", dbConnection))
			{
				command.Parameters.AddWithValue("user", username);

				int coins = Convert.ToInt32(command.ExecuteScalar());

				if (coins < Constants.PackageCost)
				{
					return Text.Res_PostTransaction_403;
				}
			}

			// DROP TABLE users; DROP TABLE sessions; DROP TABLE stacks; DROP TABLE packages; DROP TABLE cards; DROP TABLE cardcategories; DROP TABLE cardtypes; DROP TABLE elements;

			return String.Empty;
		}
	}
}
