using MTCG.Battle;
using MTCG.Interfaces.ICard;
using System.Collections.Immutable;
using System.Text;

namespace MTCG.Cards
{
	public abstract class Card : ICard
	{
		private static readonly ImmutableDictionary<string, (Type ObjectType, CardType CardType, ElementType ElementType)> cardCategories =
			new Dictionary<string, (Type ObjectType, CardType CardType, ElementType ElementType)>()
		{
			{ "FireSpell", ( typeof(CardSpell), CardType.Spell, ElementType.Fire ) },
			{ "WaterSpell", ( typeof(CardSpell), CardType.Spell, ElementType.Water ) },
			{ "RegularSpell", ( typeof(CardSpell), CardType.Spell, ElementType.Regular ) },
			{ "FireGoblin", ( typeof(CardGoblin), CardType.Goblin, ElementType.Fire ) },
			{ "WaterGoblin", ( typeof(CardGoblin), CardType.Goblin, ElementType.Water ) },
			{ "RegularGoblin", ( typeof(CardGoblin), CardType.Goblin, ElementType.Regular ) },
			{ "Dragon", ( typeof(CardDragon), CardType.Dragon, ElementType.Fire ) },
			{ "Wizard", ( typeof(CardWizard), CardType.Wizard, ElementType.Regular ) },
			{ "Ork", ( typeof(CardOrk), CardType.Ork, ElementType.Regular ) },
			{ "Knight", ( typeof(CardKnight), CardType.Knight, ElementType.Regular ) },
			{ "Kraken", ( typeof(CardKraken), CardType.Kraken, ElementType.Water ) },
			{ "FireElf", ( typeof(CardElf), CardType.Elf, ElementType.Fire ) },
			{ "WaterElf", ( typeof(CardElf), CardType.Elf, ElementType.Water ) },
			{ "RegularElf", ( typeof(CardElf), CardType.Elf, ElementType.Regular ) },
			{ "FireTroll", ( typeof(CardTroll), CardType.Troll, ElementType.Fire ) },
			{ "WaterTroll", ( typeof(CardTroll), CardType.Troll, ElementType.Water ) },
			{ "RegularTroll", ( typeof(CardTroll), CardType.Troll, ElementType.Regular ) }
		}.ToImmutableDictionary();

		public Guid Id { get; private set; }

		public string Name { get; private set; } = string.Empty;

		public float Damage { get; private set; }

		public CardType Type { get; private set; }

		public ElementType Element { get; private set; }

		public static CardType GetCardTypeByName(string name) => cardCategories[name].CardType;

		public static ElementType GetElementTypeByName(string name) => cardCategories[name].ElementType;

		public static ICard? CreateInstance(Guid id, string name, float damage)
		{
			Type cardType = cardCategories[name].ObjectType;
			if (Activator.CreateInstance(cardType) is not Card card)
			{
				return null;
			}
			card.Id = id;
			card.Name = name;
			card.Damage = damage;
			card.Type = cardCategories[name].CardType;
			card.Element = cardCategories[name].ElementType;

			return card;
		}

		// TODO: put in BattleManager ?
		public static FightResult CompareCards(ICard player1, ICard player2, StringBuilder log)
		{
			// TODO: change to actual usernames
			// changed fightlog a bit; original didn't show damage change in pure monster fight
			// used spell fight template for monsters as well in account of specifications
			log.Append($"Player1: {player1.Name} ({player1.Damage} Damage) vs Player2: {player2.Name} ({player2.Damage} Damage) => ");
			log.Append($"Player1: {player1.Name} ({player1.Damage} Damage) vs Player2: {player2.Name} ({player2.Damage} Damage) => ");

			float damagePlayer1 = player1.GetDamageAgainst(player2);
			float damagePlayer2 = player2.GetDamageAgainst(player1);

			log.Append($"{player1.Damage} VS {player2.Damage} -> {damagePlayer1} VS {damagePlayer2} => ");

			// pure monster fight
			if (player1.Type != CardType.Spell && player2.Type != CardType.Spell)
			{
				if (damagePlayer1 > damagePlayer2)
				{
					log.Append($"{player1.Name} defeats {player2.Name}");
					return FightResult.Player1;
				}

				if (damagePlayer1 < damagePlayer2)
				{
					log.Append($"{player2.Name} defeats {player1.Name}");
					return FightResult.Player2;
				}

				log.Append("Draw (no action)");
				return FightResult.Draw;
			}

			// fight including spells
			float elementalDamagePlayer1 = damagePlayer1 * player1.GetElementalFactorAgainst(player2);
			float elementalDamagePlayer2 = damagePlayer2 * player2.GetElementalFactorAgainst(player1);

			if (elementalDamagePlayer1 > elementalDamagePlayer2)
			{
				log.Append($"{player1.Name} wins");
				return FightResult.Player1;
			}

			if (elementalDamagePlayer1 < elementalDamagePlayer2)
			{
				log.Append($"{player2.Name} wins");
				return FightResult.Player2;
			}

			log.Append("Draw (no action)");
			return FightResult.Draw;
		}

		public virtual float GetDamageAgainst(ICard card)
		{
			return Damage;
		}

		public float GetElementalFactorAgainst(ICard card)
		{
			ElementType ownType = this.Element;
			ElementType opposingType = card.Element;

			if (ownType == opposingType)
			{
				return 1;
			}

			switch (ownType)
			{
				case ElementType.Water when opposingType == ElementType.Fire:
				case ElementType.Fire when opposingType == ElementType.Regular:
				case ElementType.Regular when opposingType == ElementType.Water:
					return 2;
			}

			return 0.5f;
		}
	}
}
