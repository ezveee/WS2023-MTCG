using MTCG.Interfaces;
using Newtonsoft.Json;

namespace MTCG.Server;

public static class HttpRequestUtility
{
	public static T DeserializeJson<T>(string request)
	{
		if (string.IsNullOrWhiteSpace(request))
		{
			throw new ArgumentNullException(nameof(request));
		}

		string jsonPayload = ExtractJsonPayload(request) ?? throw new InvalidOperationException();
		if (string.IsNullOrWhiteSpace(jsonPayload))
		{
			throw new InvalidOperationException();
		}

		T obj = JsonConvert.DeserializeObject<T>(jsonPayload) ?? throw new JsonSerializationException();
		return obj;
	}

	public static string ExtractJsonPayload(string request)
	{
		if (string.IsNullOrWhiteSpace(request))
		{
			throw new ArgumentNullException(nameof(request));
		}

		int bodyStartIndex = request.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4;
		string jsonPayload = request[bodyStartIndex..].Trim(); // .. range operator instead of request.Substring(bodyStartIndex)

		return jsonPayload;
	}

	public static string? ExtractPathAddOns(string request)
	{
		if (string.IsNullOrWhiteSpace(request))
		{
			throw new ArgumentNullException(nameof(request));
		}

		string[] lines = request.Split('\n');
		string[] tokens = lines[0].Split(' ');
		string fullPath = tokens[1];
		string?[] pathComponents = fullPath.Split('/');
		if (pathComponents.Length >= 3 && pathComponents[2] == string.Empty)
		{
			pathComponents[2] = null;
		}
		return pathComponents.Length < 3 ? null : pathComponents[2];
	}

	public static string ExtractBearerToken(string request)
	{
		if (string.IsNullOrWhiteSpace(request))
		{
			throw new ArgumentNullException(nameof(request));
		}

		// find authorization header
		int authHeaderIndex = request.IndexOf("Authorization: ");
		if (authHeaderIndex == -1)
		{
			throw new InvalidOperationException("Authorization header not found");
		}

		// extract authorization header value
		string authHeaderValue = request[(authHeaderIndex + "Authorization: ".Length)..];
		if (string.IsNullOrWhiteSpace(authHeaderValue))
		{
			throw new InvalidOperationException("Invalid Authorization header value");
		}

		// check authorization header format
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
		ArgumentNullException.ThrowIfNull(dataAccess);

		if (string.IsNullOrWhiteSpace(request))
		{
			throw new ArgumentNullException(nameof(request));
		}

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
