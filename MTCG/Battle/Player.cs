using MTCG.Interfaces;

namespace MTCG.Battle
{
	public sealed class Player
	{
		public Player(string username, List<ICard> deck)
		{
			_username = username;
			_deck = deck;
		}

		private readonly string _username;
		private readonly List<ICard> _deck;

		public string Username { get { return _username; } }
		public List<ICard> Deck { get { return _deck; } }
	}
}
