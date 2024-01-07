using MTCG.Cards;
using MTCG.Interfaces;

namespace MTCG_UnitTests.Cards;
[TestFixture]
public class CardTests
{
	private class TestCard : Card
	{
	}

	private TestCard _testClass;

	[SetUp]
	public void SetUp()
	{
		_testClass = new TestCard();
	}

	[TestCase("FireSpell", CardType.Spell)]
	[TestCase("WaterSpell", CardType.Spell)]
	[TestCase("RegularSpell", CardType.Spell)]
	[TestCase("FireGoblin", CardType.Goblin)]
	[TestCase("WaterGoblin", CardType.Goblin)]
	[TestCase("RegularGoblin", CardType.Goblin)]
	[TestCase("Dragon", CardType.Dragon)]
	[TestCase("Wizard", CardType.Wizard)]
	[TestCase("Ork", CardType.Ork)]
	[TestCase("Knight", CardType.Knight)]
	[TestCase("Kraken", CardType.Kraken)]
	[TestCase("FireElf", CardType.Elf)]
	[TestCase("WaterElf", CardType.Elf)]
	[TestCase("RegularElf", CardType.Elf)]
	[TestCase("FireTroll", CardType.Troll)]
	[TestCase("WaterTroll", CardType.Troll)]
	[TestCase("RegularTroll", CardType.Troll)]
	public void CanCallGetCardTypeByName(string name, CardType expected)
	{
		// Act
		CardType result = TestCard.GetCardTypeByName(name);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public void CannotCallGetCardTypeByNameWithInvalidName(string value)
	{
		Assert.Throws<ArgumentNullException>(() => TestCard.GetCardTypeByName(value));
	}

	[TestCase("FireSpell", ElementType.Fire)]
	[TestCase("WaterSpell", ElementType.Water)]
	[TestCase("RegularSpell", ElementType.Regular)]
	[TestCase("FireGoblin", ElementType.Fire)]
	[TestCase("WaterGoblin", ElementType.Water)]
	[TestCase("RegularGoblin", ElementType.Regular)]
	[TestCase("Dragon", ElementType.Fire)]
	[TestCase("Wizard", ElementType.Regular)]
	[TestCase("Ork", ElementType.Regular)]
	[TestCase("Knight", ElementType.Regular)]
	[TestCase("Kraken", ElementType.Water)]
	[TestCase("FireElf", ElementType.Fire)]
	[TestCase("WaterElf", ElementType.Water)]
	[TestCase("RegularElf", ElementType.Regular)]
	[TestCase("FireTroll", ElementType.Fire)]
	[TestCase("WaterTroll", ElementType.Water)]
	[TestCase("RegularTroll", ElementType.Regular)]
	public void CanCallGetElementTypeByName(string name, ElementType expected)
	{
		// Act
		ElementType result = TestCard.GetElementTypeByName(name);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public void CannotCallGetElementTypeByNameWithInvalidName(string value)
	{
		Assert.Throws<ArgumentNullException>(() => TestCard.GetElementTypeByName(value));
	}

	[TestCase("FireSpell", CardType.Spell, ElementType.Fire)]
	[TestCase("WaterSpell", CardType.Spell, ElementType.Water)]
	[TestCase("RegularSpell", CardType.Spell, ElementType.Regular)]
	[TestCase("FireGoblin", CardType.Goblin, ElementType.Fire)]
	[TestCase("WaterGoblin", CardType.Goblin, ElementType.Water)]
	[TestCase("RegularGoblin", CardType.Goblin, ElementType.Regular)]
	[TestCase("Dragon", CardType.Dragon, ElementType.Fire)]
	[TestCase("Wizard", CardType.Wizard, ElementType.Regular)]
	[TestCase("Ork", CardType.Ork, ElementType.Regular)]
	[TestCase("Knight", CardType.Knight, ElementType.Regular)]
	[TestCase("Kraken", CardType.Kraken, ElementType.Water)]
	[TestCase("FireElf", CardType.Elf, ElementType.Fire)]
	[TestCase("WaterElf", CardType.Elf, ElementType.Water)]
	[TestCase("RegularElf", CardType.Elf, ElementType.Regular)]
	[TestCase("FireTroll", CardType.Troll, ElementType.Fire)]
	[TestCase("WaterTroll", CardType.Troll, ElementType.Water)]
	[TestCase("RegularTroll", CardType.Troll, ElementType.Regular)]
	public void CanCallCreateInstance(string name, CardType expectedType, ElementType expectedElement)
	{
		// Arrange
		Guid id = new("547f0ba1-22f9-409c-9c53-db117ff674b9");
		float damage = 11859.73F;

		// Act
		ICard result = TestCard.CreateInstance(id, name, damage)!;

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(result.Id, Is.EqualTo(id));
			Assert.That(result.Name, Is.EqualTo(name));
			Assert.That(result.Type, Is.EqualTo(expectedType));
			Assert.That(result.Element, Is.EqualTo(expectedElement));
			Assert.That(result.Damage, Is.EqualTo(damage));
		});
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("   ")]
	public void CannotCallCreateInstanceWithInvalidName(string value)
	{
		Assert.Throws<ArgumentNullException>(() => TestCard.CreateInstance(new Guid("c08328bc-8608-4ec0-bff9-5fb688dbdd74"), value, 19759.0332F));
	}

	[TestCase("WaterGoblin", "Dragon", 0)]
	[TestCase("FireGoblin", "Dragon", 0)]
	[TestCase("RegularGoblin", "Dragon", 0)]
	[TestCase("Ork", "Wizard", 0)]
	[TestCase("Knight", "WaterSpell", -1)]
	[TestCase("WaterSpell", "Kraken", 0)]
	[TestCase("FireSpell", "Kraken", 0)]
	[TestCase("RegularSpell", "Kraken", 0)]
	[TestCase("Dragon", "FireElf", 0)]
	public void CanCallGetDamageAgainst(string card1Name, string card2Name, float expected)
	{
		// Arrange
		Guid id = new("547f0ba1-22f9-409c-9c53-db117ff674b9");
		float damage = 10;

		ICard card1 = TestCard.CreateInstance(id, card1Name, damage)!;
		ICard card2 = TestCard.CreateInstance(id, card2Name, damage)!;

		// Act
		float result = card1.GetDamageAgainst(card2);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
	}

	[Test]
	public void CannotCallGetDamageAgainstWithNullCard()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => _testClass.GetDamageAgainst(default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[TestCase("WaterGoblin", "WaterGoblin", 1)]
	[TestCase("WaterGoblin", "FireGoblin", 2)]
	[TestCase("WaterGoblin", "RegularGoblin", 0.5f)]
	public void CanCallGetElementalFactorAgainst(string card1Name, string card2Name, float expected)
	{
		// Arrange
		Guid id = new("547f0ba1-22f9-409c-9c53-db117ff674b9");
		float damage = 10;

		ICard card1 = TestCard.CreateInstance(id, card1Name, damage)!;
		ICard card2 = TestCard.CreateInstance(id, card2Name, damage)!;

		// Act
		float result = card1.GetElementalFactorAgainst(card2);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
	}

	[Test]
	public void CannotCallGetElementalFactorAgainstWithNullCard()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Assert.Throws<ArgumentNullException>(() => _testClass.GetElementalFactorAgainst(default));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[Test]
	public void CanGetId()
	{
		// Assert
		Assert.That(_testClass.Id, Is.InstanceOf<Guid>());
	}

	[Test]
	public void CanGetName()
	{
		// Assert
		Assert.That(_testClass.Name, Is.InstanceOf<string>());
	}

	[Test]
	public void CanGetDamage()
	{
		// Assert
		Assert.That(_testClass.Damage, Is.InstanceOf<float>());
	}

	[Test]
	public void CanGetType()
	{
		// Assert
		Assert.That(_testClass.Type, Is.InstanceOf<CardType>());
	}

	[Test]
	public void CanGetElement()
	{
		// Assert
		Assert.That(_testClass.Element, Is.InstanceOf<ElementType>());
	}
}