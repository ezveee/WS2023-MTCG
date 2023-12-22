using MTCG.Interfaces.IHttpRequest;
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
			if (IHttpRequest.ExtractPathAddOns(request) != "packages")
				return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n404 Not Found";

			return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nPath /transactions/packages reached";
		}
	}
}
