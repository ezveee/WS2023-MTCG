using MTCG.Cards;
using MTCG.Cards.MonsterCards;
using MTCG.Cards.SpellCards;

namespace MTCG
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			var battleHandler = new BattleHandler();
			var playerA = new Player();
			var playerB = new Player();

			// temporary hardcoded values
			// plan on using db for this later
			Card[] cards =
			{
				new MonsterCard(CardType.Goblin, ElementType.Water, 10),
				new MonsterCard(CardType.Dragon, ElementType.Fire, 10),
				new MonsterCard(CardType.Wizard, ElementType.Regular, 10),
				new MonsterCard(CardType.Ork, ElementType.Water, 10),
				new MonsterCard(CardType.Knight, ElementType.Fire, 10),
				new MonsterCard(CardType.Kraken, ElementType.Regular, 10),
				new MonsterCard(CardType.Elf, ElementType.Water, 10),
				new MonsterCard(CardType.Troll, ElementType.Fire, 10),
				new SpellCard(ElementType.Water, 10),
				new SpellCard(ElementType.Fire, 10),
				new SpellCard(ElementType.Regular, 10)
			};
			playerA.Deck.AddRange(cards);
			playerB.Deck.AddRange(cards);

			//Console.WriteLine();
			//Console.WriteLine(playerA.Deck[battleHandler.GetRandomCard(playerA.Deck)].Name);
			//Console.WriteLine(playerB.Deck[battleHandler.GetRandomCard(playerB.Deck)].Name);
		}
	}
}