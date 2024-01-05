using MTCG.Interfaces.ICard;

namespace MTCG.Battle
{
	public sealed class Player
	{
		public Player(string username, List<ICard> deck)
		{
			_username = username;
			_deck = deck;
		}

		private string _username;
		private List<ICard> _deck = new();

		public string Username { get { return _username; } }
		public List<ICard> Deck { get { return _deck; } }
	}
}
