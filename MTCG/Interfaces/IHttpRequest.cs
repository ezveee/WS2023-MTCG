using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Interfaces.IHttpRequest
{
	interface IHttpRequest
	{
		string GetResponse(string request);

		static string ExtractJsonPayload(string request)
		{
			int bodyStartIndex = request.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4;
			string jsonPayload = request[bodyStartIndex..].Trim(); // .. range operator instead of request.Substring(bodyStartIndex)
			Console.WriteLine($"Received JSON payload:\n{jsonPayload}");

			return jsonPayload;
		}
	}
}
