using Moq;
using MTCG.Battle;
using MTCG.Cards;
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => new BattleManager(default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallHandleBattleWithNullPlayer()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => _testClass.HandleBattle(default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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
	public void CanCallBattle_ResultInDraw()
	{
		// Arrange
		ICard card1 = Card.CreateInstance(Guid.NewGuid(), "WaterGoblin", 10)!;
		ICard card2 = Card.CreateInstance(Guid.NewGuid(), "WaterGoblin", 10)!;

		Player player1 = new("TestValue210876246", new List<ICard>() { card1 });
		Player player2 = new("TestValue921621522", new List<ICard>() { card2 });
		StringBuilder log = new();

		// Act
		BattleManager.Battle(player1, player2, log, out FightResult result);

		// Assert
		Assert.That(result, Is.EqualTo(FightResult.Draw));
	}

	[Test]
	public void CannotCallBattleWithNullPlayer1()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.Battle(default, new Player("TestValue1273151873", new List<ICard>()), new StringBuilder(), out _));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallBattleWithNullPlayer2()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.Battle(new Player("TestValue682487348", new List<ICard>()), default, new StringBuilder(), out _));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallBattleWithNullLog()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.Battle(new Player("TestValue1712899707", new List<ICard>()), new Player("TestValue2091168813", new List<ICard>()), default, out _));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CanCallPlayRound()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		float damage = 20;

		ICard card1 = Card.CreateInstance(id, "WaterGoblin", damage)!;
		ICard card2 = Card.CreateInstance(id, "FireSpell", damage)!;

		List<ICard> player1Deck = new()
		{
			card1
		};
		List<ICard> player2Deck = new()
		{
			card2
		};
		StringBuilder log = new();

		List<ICard> expected = new()
		{
			card1,
			card2
		};

		// Act
		BattleManager.PlayRound(player1Deck, player2Deck, log);

		// Assert
		Assert.That(player1Deck, Is.EqualTo(expected));
	}

	[Test]
	public void CannotCallPlayRoundWithNullPlayer1Deck()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.PlayRound(default, new List<ICard>(), new StringBuilder()));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallPlayRoundWithNullPlayer2Deck()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.PlayRound(new List<ICard>(), default, new StringBuilder()));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallPlayRoundWithNullLog()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.PlayRound(new List<ICard>(), new List<ICard>(), default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
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
		Assert.That(result, Is.EqualTo(FightResult.Draw));
	}

	[Test]
	public void CannotCallCompareCardsWithNullPlayer1()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.CompareCards(default, new Mock<ICard>().Object, new StringBuilder()));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallCompareCardsWithNullPlayer2()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.CompareCards(new Mock<ICard>().Object, default, new StringBuilder()));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallCompareCardsWithNullLog()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.CompareCards(new Mock<ICard>().Object, new Mock<ICard>().Object, default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallGetRandomCardWithNullDeck()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.GetRandomCard(default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[TestCase("WaterSpell", "WaterSpell", false)]
	[TestCase("WaterSpell", "FireSpell", false)]
	[TestCase("WaterSpell", "RegularSpell", false)]
	[TestCase("FireSpell", "FireSpell", false)]
	[TestCase("FireSpell", "RegularSpell", false)]
	[TestCase("RegularSpell", "RegularSpell", false)]
	[TestCase("WaterSpell", "WaterGoblin", false)]
	[TestCase("WaterSpell", "FireGoblin", false)]
	[TestCase("WaterSpell", "RegularGoblin", false)]
	[TestCase("FireSpell", "FireGoblin", false)]
	[TestCase("FireSpell", "RegularGoblin", false)]
	[TestCase("RegularSpell", "RegularGoblin", false)]
	[TestCase("WaterSpell", "Dragon", false)]
	[TestCase("FireSpell", "Dragon", false)]
	[TestCase("RegularSpell", "Dragon", false)]
	[TestCase("WaterSpell", "Wizard", false)]
	[TestCase("FireSpell", "Wizard", false)]
	[TestCase("RegularSpell", "Wizard", false)]
	[TestCase("WaterSpell", "Ork", false)]
	[TestCase("FireSpell", "Ork", false)]
	[TestCase("RegularSpell", "Ork", false)]
	[TestCase("WaterSpell", "Knight", false)]
	[TestCase("FireSpell", "Knight", false)]
	[TestCase("RegularSpell", "Knight", false)]
	[TestCase("WaterSpell", "Kraken", false)]
	[TestCase("FireSpell", "Kraken", false)]
	[TestCase("RegularSpell", "Kraken", false)]
	[TestCase("WaterSpell", "WaterElf", false)]
	[TestCase("WaterSpell", "FireElf", false)]
	[TestCase("WaterSpell", "RegularElf", false)]
	[TestCase("FireSpell", "FireElf", false)]
	[TestCase("FireSpell", "RegularElf", false)]
	[TestCase("RegularSpell", "RegularElf", false)]
	[TestCase("WaterSpell", "WaterTroll", false)]
	[TestCase("WaterSpell", "FireTroll", false)]
	[TestCase("WaterSpell", "RegularTroll", false)]
	[TestCase("FireSpell", "FireTroll", false)]
	[TestCase("FireSpell", "RegularTroll", false)]
	[TestCase("RegularSpell", "RegularTroll", false)]
	[TestCase("WaterGoblin", "WaterGoblin", true)]
	[TestCase("WaterGoblin", "FireGoblin", true)]
	[TestCase("WaterGoblin", "RegularGoblin", true)]
	[TestCase("FireGoblin", "FireGoblin", true)]
	[TestCase("FireGoblin", "RegularGoblin", true)]
	[TestCase("RegularGoblin", "RegularGoblin", true)]
	[TestCase("WaterGoblin", "Dragon", true)]
	[TestCase("FireGoblin", "Dragon", true)]
	[TestCase("RegularGoblin", "Dragon", true)]
	[TestCase("WaterGoblin", "Wizard", true)]
	[TestCase("FireGoblin", "Wizard", true)]
	[TestCase("RegularGoblin", "Wizard", true)]
	[TestCase("WaterGoblin", "Ork", true)]
	[TestCase("FireGoblin", "Ork", true)]
	[TestCase("RegularGoblin", "Ork", true)]
	[TestCase("WaterGoblin", "Knight", true)]
	[TestCase("FireGoblin", "Knight", true)]
	[TestCase("RegularGoblin", "Knight", true)]
	[TestCase("WaterGoblin", "Kraken", true)]
	[TestCase("FireGoblin", "Kraken", true)]
	[TestCase("RegularGoblin", "Kraken", true)]
	[TestCase("WaterGoblin", "WaterElf", true)]
	[TestCase("WaterGoblin", "FireElf", true)]
	[TestCase("WaterGoblin", "RegularElf", true)]
	[TestCase("FireGoblin", "FireElf", true)]
	[TestCase("FireGoblin", "RegularElf", true)]
	[TestCase("RegularGoblin", "RegularElf", true)]
	[TestCase("WaterGoblin", "WaterTroll", true)]
	[TestCase("WaterGoblin", "FireTroll", true)]
	[TestCase("WaterGoblin", "RegularTroll", true)]
	[TestCase("FireGoblin", "FireTroll", true)]
	[TestCase("FireGoblin", "RegularTroll", true)]
	[TestCase("RegularGoblin", "RegularTroll", true)]
	[TestCase("Dragon", "Dragon", true)]
	[TestCase("Dragon", "Wizard", true)]
	[TestCase("Dragon", "Ork", true)]
	[TestCase("Dragon", "Knight", true)]
	[TestCase("Dragon", "Kraken", true)]
	[TestCase("Dragon", "WaterElf", true)]
	[TestCase("Dragon", "FireElf", true)]
	[TestCase("Dragon", "RegularElf", true)]
	[TestCase("Dragon", "WaterTroll", true)]
	[TestCase("Dragon", "FireTroll", true)]
	[TestCase("Dragon", "RegularTroll", true)]
	[TestCase("Wizard", "Wizard", true)]
	[TestCase("Wizard", "Ork", true)]
	[TestCase("Wizard", "Knight", true)]
	[TestCase("Wizard", "Kraken", true)]
	[TestCase("Wizard", "WaterElf", true)]
	[TestCase("Wizard", "FireElf", true)]
	[TestCase("Wizard", "RegularElf", true)]
	[TestCase("Wizard", "WaterTroll", true)]
	[TestCase("Wizard", "FireTroll", true)]
	[TestCase("Wizard", "RegularTroll", true)]
	[TestCase("Ork", "Ork", true)]
	[TestCase("Ork", "Knight", true)]
	[TestCase("Ork", "Kraken", true)]
	[TestCase("Ork", "WaterElf", true)]
	[TestCase("Ork", "FireElf", true)]
	[TestCase("Ork", "RegularElf", true)]
	[TestCase("Ork", "WaterTroll", true)]
	[TestCase("Ork", "FireTroll", true)]
	[TestCase("Ork", "RegularTroll", true)]
	[TestCase("Knight", "Knight", true)]
	[TestCase("Knight", "Kraken", true)]
	[TestCase("Knight", "WaterElf", true)]
	[TestCase("Knight", "FireElf", true)]
	[TestCase("Knight", "RegularElf", true)]
	[TestCase("Knight", "WaterTroll", true)]
	[TestCase("Knight", "FireTroll", true)]
	[TestCase("Knight", "RegularTroll", true)]
	[TestCase("Kraken", "Kraken", true)]
	[TestCase("Kraken", "WaterElf", true)]
	[TestCase("Kraken", "FireElf", true)]
	[TestCase("Kraken", "RegularElf", true)]
	[TestCase("Kraken", "WaterTroll", true)]
	[TestCase("Kraken", "FireTroll", true)]
	[TestCase("Kraken", "RegularTroll", true)]
	[TestCase("WaterElf", "WaterElf", true)]
	[TestCase("WaterElf", "FireElf", true)]
	[TestCase("WaterElf", "RegularElf", true)]
	[TestCase("FireElf", "FireElf", true)]
	[TestCase("FireElf", "RegularElf", true)]
	[TestCase("RegularElf", "RegularElf", true)]
	[TestCase("WaterElf", "WaterTroll", true)]
	public void CanCallIsPureMonsterFight(string card1Name, string card2Name, bool expected)
	{
		// Arrange
		ICard player1 = Card.CreateInstance(Guid.NewGuid(), card1Name, 20)!;
		ICard player2 = Card.CreateInstance(Guid.NewGuid(), card2Name, 20)!;

		// Act
		bool result = BattleManager.IsPureMonsterFight(player1, player2);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
	}

	[Test]
	public void CannotCallIsPureMonsterFightWithNullPlayer1()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.IsPureMonsterFight(default, new Mock<ICard>().Object));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CannotCallIsPureMonsterFightWithNullPlayer2()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => BattleManager.IsPureMonsterFight(new Mock<ICard>().Object, default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}
}