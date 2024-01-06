using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server;

public static class HttpRequestUtility
{
	public static T DeserializeJson<T>(string request)
	{
		string jsonPayload = ExtractJsonPayload(request) ?? throw new InvalidOperationException();
		T obj = JsonConvert.DeserializeObject<T>(jsonPayload) ?? throw new JsonSerializationException();
		return obj;
	}

	public static string ExtractJsonPayload(string request)
	{
		int bodyStartIndex = request.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4;
		string jsonPayload = request[bodyStartIndex..].Trim(); // .. range operator instead of request.Substring(bodyStartIndex)

		return jsonPayload;
	}

	public static string? ExtractPathAddOns(string request)
	{
		string[] lines = request.Split('\n');
		string[] tokens = lines[0].Split(' ');
		string fullPath = tokens[1];
		string[] pathComponents = fullPath.Split('/');

		return pathComponents.Length < 3 ? null : pathComponents[2];
	}

	public static string ExtractBearerToken(string request)
	{
		// Find the Authorization header
		int authHeaderIndex = request.IndexOf("Authorization: ");
		if (authHeaderIndex == -1)
		{
			throw new InvalidOperationException("Authorization header not found");
		}

		// Extract the Authorization header value
		string authHeaderValue = request[(authHeaderIndex + "Authorization: ".Length)..];
		int endIndex = authHeaderValue.IndexOf("\r\n");
		if (endIndex == -1)
		{
			throw new InvalidOperationException("Invalid Authorization header format");
		}

		authHeaderValue = authHeaderValue[..endIndex];

		if (!authHeaderValue.StartsWith("Bearer "))
		{
			throw new InvalidOperationException("Invalid Authorization header format");
		}

		// Extract and return the Bearer token
		return authHeaderValue["Bearer ".Length..];
	}

	public static bool IsUserAccessValid(IDataAccess dataAccess, string request, out string? authToken)
	{
		try
		{
			authToken = ExtractBearerToken(request);
		}
		catch (InvalidOperationException)
		{
			authToken = null;
			return false;
		}

		return dataAccess.IsTokenValid(authToken);
	}
}
