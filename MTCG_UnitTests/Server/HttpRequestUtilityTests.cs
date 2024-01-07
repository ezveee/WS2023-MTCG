using Moq;
using MTCG.Database.Schemas;
using MTCG.Interfaces;
using MTCG.Server;

namespace MTCG_UnitTests.Server;

[TestFixture]
public class HttpRequestUtilityTests
{
	private Mock<IDataAccess> _dataAccess;

	[SetUp]
	public void SetUp()
	{
		_dataAccess = new Mock<IDataAccess>();
	}

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

		string request = "POST /tradings HTTP/1.1\r\n" +
			"Content-Type: application/json\r\n" +
			"Authorization: Bearer kienboec-mtcgToken\r\n" +
			"User-Agent: PostmanRuntime/7.36.0\r\nAccept: */*\r\n" +
			"Postman-Token: 02a18fab-d686-419f-9ab7-828347ecab46\r\n" +
			"Host: localhost:10001\r\nAccept-Encoding: gzip, deflate, br\r\n" +
			"Connection: keep-alive\r\nContent-Length: 141\r\n\r\n" +
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

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallDeserializeJsonWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.DeserializeJson<string>(value));
	}

	[Test]
	public static void CanCallExtractJsonPayload()
	{
		// Arrange
		string expected = "{\"Id\": \"abcd-efgh\", \"Name\": \"test\", \"Password\": \"1234\"}";

		string request = "HTTPMETHOD /path HTTP/1.1\r\n" +
			"Content-Type: application/json\r\n\r\n" +
			expected;

		// Act
		string result = HttpRequestUtility.ExtractJsonPayload(request);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallExtractJsonPayloadWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.ExtractJsonPayload(value));
	}

	[Test]
	public static void CanCallExtractPathAddOns()
	{
		// Arrange
		string request = "HTTPMETHOD /path/addon";
		string expected = "addon";

		// Act
		string? result = HttpRequestUtility.ExtractPathAddOns(request);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
	}

	[Test]
	public static void CanCallExtractPathAddOns_ShouldReturnNull()
	{
		// Arrange
		string request = "HTTPMETHOD /path/";

		// Act
		string? result = HttpRequestUtility.ExtractPathAddOns(request);

		// Assert
		Assert.That(result, Is.Null);
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallExtractPathAddOnsWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.ExtractPathAddOns(value));
	}

	[Test]
	public static void CanCallExtractBearerToken()
	{
		// Arrange
		string expected = "token";
		string request = "HTTPMETHOD /path\r\n" +
			"Authorization: Bearer " + expected +
			"\r\n\r\n";

		// Act
		string result = HttpRequestUtility.ExtractBearerToken(request);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
	}

	[Test]
	public static void CannotCallExtractBearerToken_NoAuthorizationHeader_ThrowsInvalidOperationException()
	{
		// Arrange
		string request = "HTTPMETHOD /path\r\n" +
			"Content-Type: json/application" +
			"\r\n\r\n";

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => HttpRequestUtility.ExtractBearerToken(request));
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallExtractBearerToken_NoAuthorizationHeaderValue_ThrowsInvalidOperationException(string value)
	{
		// Arrange
		string request = "HTTPMETHOD /path\r\n" +
			"Authorization: " + value +
			"\r\n\r\n";

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => HttpRequestUtility.ExtractBearerToken(request));
	}

	[Test]
	public static void CannotCallExtractBearerToken_InvalidHeaderFormatNoNewLine_ThrowsInvalidOperationException()
	{
		// Arrange
		string request = "HTTPMETHOD /path\r\n" +
			"Authorization: Bearer token";

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => HttpRequestUtility.ExtractBearerToken(request));
	}

	[Test]
	public static void CannotCallExtractBearerToken_InvalidHeaderFormatNoBearer_ThrowsInvalidOperationException()
	{
		// Arrange
		string request = "HTTPMETHOD /path\r\n" +
			"Authorization: token" +
			"\r\n\r\n";

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => HttpRequestUtility.ExtractBearerToken(request));
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallExtractBearerTokenWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.ExtractBearerToken(value));
	}

	[Test]
	public void CanCallIsUserAccessValid()
	{
		// Arrange
		string request = "HTTPMETHOD /path\r\n" +
			"Authorization: Bearer token" +
			"\r\n\r\n";

		_dataAccess
			.Setup(mock => mock.IsTokenValid(It.IsAny<string>()))
			.Returns(true);

		// Act
		bool result = HttpRequestUtility.IsUserAccessValid(_dataAccess.Object, request, out string? authToken);

		// Assert
		Assert.That(result, Is.True);
	}

	[Test]
	public static void CannotCallIsUserAccessValidWithNullDataAccess()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.IsUserAccessValid(default, "TestValue1468502941", out _));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public static void CannotCallIsUserAccessValidWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => HttpRequestUtility.IsUserAccessValid(new Mock<IDataAccess>().Object, value, out _));
	}
}