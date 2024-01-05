using MTCG.Cards;
using MTCG.Database;
using System.Text;

namespace MTCG.Battle
{
	public class BattleManager
	{
		private static BattleManager _instance;

		public static BattleManager Instance
		{
			get
			{
				_instance ??= new BattleManager();
				return _instance;
			}
		}

		private static List<Player> battleLobby = new();

		public static string HandleBattle(Player player)
		{
			// TODO: fix battle
			lock (battleLobby)
			{
				battleLobby.Add(player);

				if (battleLobby.Count >= 2)
				{
					Player playerA = battleLobby[0];
					Player playerB = battleLobby[1];

					string battleLog = Battle(playerA, playerB);

					battleLobby.Remove(playerA);
					battleLobby.Remove(playerB);

					return battleLog;
				}

				return string.Empty;
			}
		}

		public static string Battle(Player playerA, Player playerB)
		{
			StringBuilder battleLog = new();

			Card cardA = playerA.Deck[GetRandomCard(playerA.Deck)];
			Card cardB = playerB.Deck[GetRandomCard(playerB.Deck)];

			bool playerAWon;

			for (int roundCounter = 0; roundCounter < Constants.MaxRounds; ++roundCounter)
			{
				if (playerA.Deck.Count <= 0 || playerB.Deck.Count <= 0)
				{
					break;
				}

				battleLog.Append($"{playerA.Username}: {cardA.Name}({cardA.Damage}) vs {playerA.Username}: {cardB.Name}({cardB.Damage}) => ");

				if (IsPureMonsterFight(cardA, cardB))
				{
					playerAWon = cardA.Damage > cardB.Damage;

					if (playerAWon)
					{
						battleLog.Append($"{cardA.Name} defeats {cardB.Name}");
						TransferCardToWinner(playerB, playerA, cardB);
						continue;
					}

					battleLog.Append($"{cardB.Name} defeats {cardA.Name}");
					TransferCardToWinner(playerA, playerB, cardA);

					continue;
				}

				float damageA = CalculateDamage(cardA, cardB);
				float damageB = CalculateDamage(cardB, cardA);

				battleLog.Append($"{cardA.Damage} VS {cardB.Damage} -> {damageA} VS {damageB} => ");
				playerAWon = damageA > damageB;

				if (playerAWon)
				{
					battleLog.Append($"{cardA.Name} wins");
					TransferCardToWinner(playerB, playerA, cardB);
					continue;
				}

				battleLog.Append($"{cardB.Name} wins");
				TransferCardToWinner(playerA, playerB, cardA);
			}

			return battleLog.ToString();
		}

		public static SpellEffect CheckElementEffectiveness(Card cardA, Card cardB)
		{
			ElementType ownType = cardA.Element;
			ElementType opposingType = cardB.Element;

			if (ownType == opposingType)
			{
				return SpellEffect.NoEffect;
			}

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
					when opposingType == CardType.Elf && cardB.Element == ElementType.Fire:
					return CardSpecialty.CantAttack;

				case CardType.Knight
					when opposingType == CardType.Spell && cardB.Element == ElementType.Water:
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
			{
				return false;
			}
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

		private static void TransferCardToWinner(Player loser, Player winner, Card card)
		{
			loser.Deck.Remove(card);
			winner.Deck.Add(card);
		}
	}
}
