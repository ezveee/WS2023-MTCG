using Moq;
using MTCG.Database.Schemas;
using MTCG.Interfaces;
using MTCG.Server;
using T = System.String;

namespace MTCG_UnitTests.Server;
/// <summary>
/// Unit tests for the type <see cref="HttpRequestUtility"/>.
/// </summary>
[TestFixture]
public static class HttpRequestUtilityTests
{
	/// <summary>
	/// Checks that the DeserializeJson method functions correctly.
	/// </summary>
	[Test]
	public static void CanCallDeserializeJson()
	{
		// Arrange
		TradingDeal expected = new()
		{
			Id = new Guid("6cd85277-4590-49d4-b0cf-ba0a921faad0"),
			CardToTrade = new Guid("1cb6ab86-bdb2-47e5-b6e4-68c5ab389334"),
			MinimumDamage = 15,
			Type = "monster"
		};

		string request = "POST /tradings HTTP/1.1\r\nContent-Type: application/json\r\nAuthorization: Bearer kienboec-mtcgToken\r\n" +
			"User-Agent: PostmanRuntime/7.36.0\r\nAccept: */*\r\nPostman-Token: 02a18fab-d686-419f-9ab7-828347ecab46\r\n" +
			"Host: localhost:10001\r\nAccept-Encoding: gzip, deflate, br\r\nConnection: keep-alive\r\nContent-Length: 141\r\n\r\n" +
			"{\"Id\": \"" + expected.Id + "\", \"CardToTrade\": \"" + expected.CardToTrade + "\", \"Type\": \"" + expected.Type + "\", \"MinimumDamage\": " + expected.MinimumDamage + "}\r\n\r\n";

		// Act
		TradingDeal result = HttpRequestUtility.DeserializeJson<TradingDeal>(request);

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(result.Id, Is.EqualTo(expected.Id));
			Assert.That(result.CardToTrade, Is.EqualTo(expected.CardToTrade));
			Assert.That(result.MinimumDamage, Is.EqualTo(expected.MinimumDamage));
			Assert.That(result.Type, Is.EqualTo(expected.Type));
		});
	}

	/// <summary>
	/// Checks that the DeserializeJson method throws when the request parameter is null, empty or white space.
	/// </summary>
	/// <param name="value">The parameter that receives the test case values.</param>
	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallDeserializeJsonWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.DeserializeJson<T>(value));
	}

	/// <summary>
	/// Checks that the ExtractJsonPayload method functions correctly.
	/// </summary>
	[Test]
	public static void CanCallExtractJsonPayload()
	{
		// Arrange
		string request = "TestValue1648038943";

		// Act
		string result = HttpRequestUtility.ExtractJsonPayload(request);

		// Assert
		Assert.Fail("Create or modify test");
	}

	/// <summary>
	/// Checks that the ExtractJsonPayload method throws when the request parameter is null, empty or white space.
	/// </summary>
	/// <param name="value">The parameter that receives the test case values.</param>
	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallExtractJsonPayloadWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.ExtractJsonPayload(value));
	}

	/// <summary>
	/// Checks that the ExtractPathAddOns method functions correctly.
	/// </summary>
	[Test]
	public static void CanCallExtractPathAddOns()
	{
		// Arrange
		string request = "TestValue1418664328";

		// Act
		string? result = HttpRequestUtility.ExtractPathAddOns(request);

		// Assert
		Assert.Fail("Create or modify test");
	}

	/// <summary>
	/// Checks that the ExtractPathAddOns method throws when the request parameter is null, empty or white space.
	/// </summary>
	/// <param name="value">The parameter that receives the test case values.</param>
	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallExtractPathAddOnsWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.ExtractPathAddOns(value));
	}

	/// <summary>
	/// Checks that the ExtractBearerToken method functions correctly.
	/// </summary>
	[Test]
	public static void CanCallExtractBearerToken()
	{
		// Arrange
		string request = "TestValue1880331100";

		// Act
		string result = HttpRequestUtility.ExtractBearerToken(request);

		// Assert
		Assert.Fail("Create or modify test");
	}

	/// <summary>
	/// Checks that the ExtractBearerToken method throws when the request parameter is null, empty or white space.
	/// </summary>
	/// <param name="value">The parameter that receives the test case values.</param>
	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallExtractBearerTokenWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.ExtractBearerToken(value));
	}

	/// <summary>
	/// Checks that the IsUserAccessValid method functions correctly.
	/// </summary>
	[Test]
	public static void CanCallIsUserAccessValid()
	{
		// Arrange
		IDataAccess dataAccess = new Mock<IDataAccess>().Object;
		string request = "TestValue1543242818";

		// Act
		bool result = HttpRequestUtility.IsUserAccessValid(dataAccess, request, out string? authToken);

		// Assert
		Assert.Fail("Create or modify test");
	}

	/// <summary>
	/// Checks that the IsUserAccessValid method throws when the dataAccess parameter is null.
	/// </summary>
	[Test]
	public static void CannotCallIsUserAccessValidWithNullDataAccess()
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.IsUserAccessValid(default, "TestValue1468502941", out _));
	}

	/// <summary>
	/// Checks that the IsUserAccessValid method throws when the request parameter is null, empty or white space.
	/// </summary>
	/// <param name="value">The parameter that receives the test case values.</param>
	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallIsUserAccessValidWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.IsUserAccessValid(new Mock<IDataAccess>().Object, value, out _));
	}
}