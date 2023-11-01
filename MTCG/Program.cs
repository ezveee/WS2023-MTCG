using MTCG.Cards;
using MTCG.Cards.MonsterCards;
using MTCG.Cards.SpellCards;

namespace MTCG
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello, World!");

			Player playerA = new Player();
			Player playerB = new Player();

			// temporary hardcoded values
			Card[] cards =
			{
				new MonsterCard(MonsterType.Goblin, ElementType.Water, 10),
				new MonsterCard(MonsterType.Dragon, ElementType.Fire, 10),
				new MonsterCard(MonsterType.Wizard, ElementType.Regular, 10),
				new MonsterCard(MonsterType.Ork, ElementType.Water, 10),
				new MonsterCard(MonsterType.Knight, ElementType.Fire, 10),
				new MonsterCard(MonsterType.Kraken, ElementType.Regular, 10),
				new MonsterCard(MonsterType.Elf, ElementType.Water, 10),
				new MonsterCard(MonsterType.Troll, ElementType.Fire, 10),
				new SpellCard(ElementType.Water, 10),
				new SpellCard(ElementType.Fire, 10),
				new SpellCard(ElementType.Regular, 10)
			};

			playerA.Deck.AddRange(cards);
			playerB.Deck.AddRange(cards);

			foreach (var card in playerA.Deck)
			{
				Console.WriteLine(card.Name);
			}
		}
	}
}