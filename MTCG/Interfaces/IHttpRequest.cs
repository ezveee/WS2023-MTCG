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

			//Console.WriteLine($"\nReceived JSON payload:\n{jsonPayload}");

			return jsonPayload;
		}

		static string? ExtractPathAddOns(string request)
		{
			string[] lines = request.Split('\n');
			string[] tokens = lines[0].Split(' ');
			string fullPath = tokens[1];
			string[] pathComponents = fullPath.Split('/');

			if (pathComponents.Length < 3)
				return null;

			return pathComponents[2];
		}
	}
}
