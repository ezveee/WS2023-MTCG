using Moq;
using MTCG.Battle;
using MTCG.Interfaces;
using System.Text;

namespace MTCG_UnitTests.Battle;
[TestFixture]
public class BattleManagerTests
{
	private BattleManager _testClass;
	private Mock<IDataAccess> _dataAccess;

	[SetUp]
	public void SetUp()
	{
		_dataAccess = new Mock<IDataAccess>();
		_testClass = new BattleManager(_dataAccess.Object);
	}

	[Test]
	public void CanConstruct()
	{
		// Act
		BattleManager instance = new(_dataAccess.Object);

		// Assert
		Assert.That(instance, Is.Not.Null);
	}

	[Test]
	public void CannotConstructWithNullDataAccess()
	{
		Assert.Throws<ArgumentNullException>(() => new BattleManager(default));
	}

	[Test]
	public void CanCallHandleBattle()
	{
		// Arrange
		Player player = new("TestValue1059601645", new List<ICard>());

		_dataAccess.Setup(mock => mock.UpdateUserStats(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

		// Act
		string result = _testClass.HandleBattle(player);

		// Assert
		_dataAccess.Verify(mock => mock.UpdateUserStats(It.IsAny<string>(), It.IsAny<string>()));

		Assert.Fail("Create or modify test");
	}

	[Test]
	public void CannotCallHandleBattleWithNullPlayer()
	{
		Assert.Throws<ArgumentNullException>(() => _testClass.HandleBattle(default));
	}

	[Test]
	public void CallBattleWithEmptyDeck_ThrowsArgumentOutOfRangeException()
	{
		// Arrange
		Player player1 = new("TestValue210876246", new List<ICard>());
		Player player2 = new("TestValue921621522", new List<ICard>());
		StringBuilder log = new();

		// Act & Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => BattleManager.Battle(player1, player2, log, out FightResult battleResult));
	}

	[Test]
	public void CannotCallBattleWithNullPlayer1()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.Battle(default, new Player("TestValue1273151873", new List<ICard>()), new StringBuilder(), out _));
	}

	[Test]
	public void CannotCallBattleWithNullPlayer2()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.Battle(new Player("TestValue682487348", new List<ICard>()), default, new StringBuilder(), out _));
	}

	[Test]
	public void CannotCallBattleWithNullLog()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.Battle(new Player("TestValue1712899707", new List<ICard>()), new Player("TestValue2091168813", new List<ICard>()), default, out _));
	}

	[Test]
	public void CanCallPlayRound()
	{
		// Arrange
		List<ICard> player1Deck = new();
		List<ICard> player2Deck = new();
		StringBuilder log = new();

		// Act
		BattleManager.PlayRound(player1Deck, player2Deck, log);

		// Assert
		Assert.Fail("Create or modify test");
	}

	[Test]
	public void CannotCallPlayRoundWithNullPlayer1Deck()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.PlayRound(default, new List<ICard>(), new StringBuilder()));
	}

	[Test]
	public void CannotCallPlayRoundWithNullPlayer2Deck()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.PlayRound(new List<ICard>(), default, new StringBuilder()));
	}

	[Test]
	public void CannotCallPlayRoundWithNullLog()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.PlayRound(new List<ICard>(), new List<ICard>(), default));
	}

	[Test]
	public void CanCallCompareCards()
	{
		// Arrange
		ICard player1 = new Mock<ICard>().Object;
		ICard player2 = new Mock<ICard>().Object;
		StringBuilder log = new();

		// Act
		FightResult result = BattleManager.CompareCards(player1, player2, log);

		// Assert
		Assert.Fail("Create or modify test");
	}

	[Test]
	public void CannotCallCompareCardsWithNullPlayer1()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.CompareCards(default, new Mock<ICard>().Object, new StringBuilder()));
	}

	[Test]
	public void CannotCallCompareCardsWithNullPlayer2()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.CompareCards(new Mock<ICard>().Object, default, new StringBuilder()));
	}

	[Test]
	public void CannotCallCompareCardsWithNullLog()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.CompareCards(new Mock<ICard>().Object, new Mock<ICard>().Object, default));
	}

	[Test]
	public void CanCallGetRandomCard()
	{
		// Arrange
		List<ICard> deck = new();

		// Act
		int result = BattleManager.GetRandomCard(deck);

		// Assert
		Assert.Fail("Create or modify test");
	}

	[Test]
	public void CannotCallGetRandomCardWithNullDeck()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.GetRandomCard(default));
	}

	[Test]
	public void CanCallIsPureMonsterFight()
	{
		// Arrange
		ICard player1 = new Mock<ICard>().Object;
		ICard player2 = new Mock<ICard>().Object;

		// Act
		bool result = BattleManager.IsPureMonsterFight(player1, player2);

		// Assert
		Assert.Fail("Create or modify test");
	}

	[Test]
	public void CannotCallIsPureMonsterFightWithNullPlayer1()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.IsPureMonsterFight(default, new Mock<ICard>().Object));
	}

	[Test]
	public void CannotCallIsPureMonsterFightWithNullPlayer2()
	{
		Assert.Throws<ArgumentNullException>(() => BattleManager.IsPureMonsterFight(new Mock<ICard>().Object, default));
	}
}