using Moq;
using MTCG;
using MTCG.Database.Schemas;
using MTCG.Interfaces;
using MTCG.Server.HttpRequests;

namespace MTCG_UnitTests.Server.HttpRequests;
[TestFixture]
public class PostTradingTests
{
	private PostTrading _testClass;
	private Mock<IDataAccess> _dataAccess;

	[SetUp]
	public void SetUp()
	{
		_dataAccess = new Mock<IDataAccess>();
		_testClass = new PostTrading(_dataAccess.Object);
	}

	[Test]
	public void CanConstruct()
	{
		// Act
		PostTrading instance = new(_dataAccess.Object);

		// Assert
		Assert.That(instance, Is.Not.Null);
	}

	[Test]
	public void CannotConstructWithNullDataAccess()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => new PostTrading(default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallGetResponse_401Unauthorized()
	{
		// Arrange
		string request = "HTTPMETHOD /path\r\n" +
			"Authorization: Bearer token" +
			"\r\n\r\n";

		_dataAccess
			.Setup(mock => mock.IsTokenValid(It.IsAny<string>()))
			.Returns(false);

		// Act
		string result = _testClass.GetResponse(request);

		// Assert
		_dataAccess.Verify(mock => mock.IsTokenValid(It.IsAny<string>()));

		Assert.That(result, Is.EqualTo(string.Format(Text.HttpResponse_401_Unauthorized, Text.Description_Default_401)));
	}

	[Test]
	public void CanCallGetResponse()
	{
		// Arrange
		string request = "HTTPMETHOD /path\r\n" +
			"Authorization: Bearer token\r\n" +
			"Content-Type: json/application\r\n\r\n" +
			"{\"Id\": \"6cd85277-4590-49d4-b0cf-ba0a921faad0\", " +
			"\"CardToTrade\": \"1cb6ab86-bdb2-47e5-b6e4-68c5ab389334\", " +
			"\"Type\": \"monster\", \"MinimumDamage\": 15}\r\n\r\n";

		_dataAccess
			.Setup(mock => mock.IsTokenValid(It.IsAny<string>()))
			.Returns(true);
		_dataAccess
			.Setup(mock => mock.RetrieveUsernameFromToken(It.IsAny<string>()))
			.Returns("TestValue1664166520");
		_dataAccess
			.Setup(mock => mock.CreateTrade(It.IsAny<TradingDeal>(), It.IsAny<string>()))
			.Returns(string.Format(Text.HttpResponse_201_Created, Text.Description_PostTrading_201));

		// Act
		string result = _testClass.GetResponse(request);

		// Assert
		_dataAccess.Verify(mock => mock.RetrieveUsernameFromToken(It.IsAny<string>()));
		_dataAccess.Verify(mock => mock.CreateTrade(It.IsAny<TradingDeal>(), It.IsAny<string>()));

		Assert.That(result, Is.EqualTo(string.Format(Text.HttpResponse_201_Created, Text.Description_PostTrading_201)));
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public void CannotCallGetResponseWithInvalidRequest(string value)
	{
		Assert.Throws<ArgumentNullException>(() => _testClass.GetResponse(value));
	}
}