using MTCG.Interfaces.IHttpRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server.HttpRequests
{
	internal class Hello : IHttpRequest
	{
		public string GetResponse(string request)
		{
			return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, World!";
		}
	}
}
