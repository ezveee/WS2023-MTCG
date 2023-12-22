using MTCG.Interfaces.IHttpRequest;
using MTCG.Server.Schemas.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server.HttpRequests
{
	internal class PostUser : IHttpRequest
	{
		public string GetResponse(string request)
		{
			Console.WriteLine();
			Console.WriteLine("This is getting called in PostUser.GetResponse()");
			string jsonPayload = IHttpRequest.ExtractJsonPayload(request);
			Console.WriteLine("Stops here");
			Console.WriteLine();

			if (jsonPayload != null)
			{
				User user = JsonConvert.DeserializeObject<User>(jsonPayload);
				Console.WriteLine($"Username: {user.Username}, Password: {user.Password}");
			}


			return "";
		}

		private static void CreateDbUser(User user)
		{

		}
	}
}
