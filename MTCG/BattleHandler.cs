using MTCG.Cards;

namespace MTCG
{
	internal class BattleHandler
	{
		public static SpellEffect CheckElementEffectiveness(Card cardA, Card cardB)
		{
			ElementType ownType = cardA.Element;
			ElementType opposingType = cardB.Element;

			if (ownType == opposingType) { return SpellEffect.NoEffect; }

			switch (ownType)
			{
				case ElementType.Water when opposingType == ElementType.Fire:
				case ElementType.Fire when opposingType == ElementType.Regular:
				case ElementType.Regular when opposingType == ElementType.Water:
					return SpellEffect.Effective;
			}

			return SpellEffect.NotEffective;
		}

		public static CardSpecialty CheckSpecialty(Card cardA, Card cardB)
		{
			CardType ownType = cardA.Type;
			CardType opposingType = cardB.Type;

			switch (ownType)
			{
				case CardType.Goblin when opposingType == CardType.Dragon:
				case CardType.Ork when opposingType == CardType.Wizard:
				case CardType.Spell when opposingType == CardType.Kraken:
				case CardType.Dragon
					when (opposingType == CardType.Elf && cardB.Element == ElementType.Fire):
					return CardSpecialty.CantAttack;

				case CardType.Knight
					when (opposingType == CardType.Spell && cardB.Element == ElementType.Water):
					return CardSpecialty.Dies;
			}

			return CardSpecialty.None;
		}

		public static int GetRandomCard(List<Card> deck)
		{
			var random = new Random();
			return random.Next(deck.Count);
		}

		public static bool IsPureMonsterFight(Card cardA, Card cardB)
		{
			if (cardA.Type == CardType.Spell || cardB.Type == CardType.Spell)
				return false;
			return true;
		}

		/// <summary>
		/// Checks if card A's damage changes somehow, based on specialties and effects regarding card B.
		/// </summary>
		/// <param name="cardA"></param>
		/// <param name="cardB"></param>
		/// <returns>Returns card A's final calculated damage. (-1: Dies, 0: Can't attack, Other: calculated Damage)</returns>
		public static float CalculateDamage(Card cardA, Card cardB)
		{
			// specialty check
			var specialty = CheckSpecialty(cardA, cardB);
			switch (specialty)
			{
				case CardSpecialty.CantAttack:
					return 0;
				case CardSpecialty.Dies:
					return -1;
			}

			// fight based on elements (at least one spell)
			if (!IsPureMonsterFight(cardA, cardB))
			{
				var effect = CheckElementEffectiveness(cardA, cardB);
				switch (effect)
				{
					case SpellEffect.Effective:
						return cardA.Damage * Constants.EffectiveMultiplier;
					case SpellEffect.NotEffective:
						return cardA.Damage * Constants.NotEffectiveMultiplier;
				}
			}

			// pure monster fight or no effect
			return cardA.Damage;
		}

		public void Battle(Player playerA, Player playerB)
		{
			var cardA = playerA.Deck[GetRandomCard(playerA.Deck)];
			var cardB = playerB.Deck[GetRandomCard(playerB.Deck)];

			var damageA = CalculateDamage(cardA, cardB);
			var damageB = CalculateDamage(cardB, cardA);

			// TODO: change temporary winner output
			// varies from fight to fight (see specification)
			// TODO: add special lines if specialties apply
			// TODO: dont display -1 as damage if knight drowns
			Console.WriteLine($"PlayerA: {cardA.Name} ({damageA}) vs PlayerB: {cardB.Name} ({damageB}) => ");
			// check who won
			if (damageA > damageB)
			{
				Console.WriteLine($"{cardA.Name} defeats {cardB.Name} (Player A won!)");
				return;
			}

			if (damageA < damageB)
			{
				Console.WriteLine($"{cardB.Name} defeats {cardA.Name} (Player B won!)");

				return;
			}

			Console.WriteLine("It's a tie!");
		}

	}
}
