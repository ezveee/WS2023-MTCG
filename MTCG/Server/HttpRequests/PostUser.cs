using MTCG.Interfaces.IHttpRequest;
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
			Console.WriteLine("Ab hier dann uwu");
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine(request);
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Hier nimmer >:3");

			return "";
		}
	}
}
