using MTCG.Cards;
using MTCG.Interfaces.ICard;
using System.Numerics;
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

		// private static List<Player> battleLobby = new();

		private static Player? queuedPlayer;
		private static Dictionary<string, string?> resultCache = new();

		public static string HandleBattle(Player player)
		{
			if (queuedPlayer is null)
			{
				queuedPlayer = player;
				resultCache.Add(player.Username, null);
				return CheckIfResultIsAvailable(player.Username);
			}

			Player player1;
			Player player2;

			lock (queuedPlayer)
			{
				player1 = player;
				player2 = queuedPlayer;

				queuedPlayer = null;
			}

			StringBuilder log = new();
			Battle(player1, player2, log);

			resultCache[player2.Username] = log.ToString();

			return log.ToString();
		}

		private static string CheckIfResultIsAvailable(string username)
		{
			while (resultCache[username] is null) ;
			string log = resultCache[username];
			resultCache.Remove(username);
			return log;
		}

		public static void Battle(Player player1, Player player2, StringBuilder log)
		{
			for (int roundCounter = 0; roundCounter < Constants.MaxRounds; ++roundCounter)
			{
				PlayRound(player1.Deck, player2.Deck, log);
				if (!player1.Deck.Any() || !player2.Deck.Any())
				{
					return;
				}
			}
		}


		public static void PlayRound(List<ICard> player1Deck, List<ICard> player2Deck, StringBuilder log)
		{
			ICard player1 = player1Deck[GetRandomCard(player1Deck)];
			ICard player2 = player2Deck[GetRandomCard(player2Deck)];

			FightResult result = CompareCards(player1, player2, log);

			if (result == FightResult.Player1)
			{
				TransferCardToWinner(player2Deck, player1Deck, player2);
			}

			if (result == FightResult.Player2)
			{
				TransferCardToWinner(player1Deck, player1Deck, player1);
			}
		}

		public static FightResult CompareCards(ICard player1, ICard player2, StringBuilder log)
		{
			// TODO: change to actual usernames
			// changed fightlog a bit; original didn't show damage change in pure monster fight
			// used spell fight template for monsters as well in account of specifications
			log.Append($"Player1: {player1.Name} ({player1.Damage} Damage) vs Player2: {player2.Name} ({player2.Damage} Damage) => ");

			float damagePlayer1 = player1.GetDamageAgainst(player2);
			float damagePlayer2 = player2.GetDamageAgainst(player1);

			log.Append($"{player1.Damage} VS {player2.Damage} -> {damagePlayer1} VS {damagePlayer2} => ");

			if (IsPureMonsterFight(player1, player2))
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

		public static int GetRandomCard(List<ICard> deck)
		{
			var random = new Random();
			return random.Next(deck.Count);
		}

		public static bool IsPureMonsterFight(ICard player1, ICard player2)
		{
			if (player1.Type == CardType.Spell || player2.Type == CardType.Spell)
			{
				return false;
			}
			return true;
		}

		private static void TransferCardToWinner(List<ICard> loser, List<ICard> winner, ICard card)
		{
			loser.Remove(card);
			winner.Add(card);
		}
	}
}
