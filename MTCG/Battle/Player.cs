using MTCG.Cards;

namespace MTCG.Battle
{
	public sealed class Player
	{
		private string _username;
		private List<Card> _deck = new();

		public string Username { get { return _username; } }
		public List<Card> Deck { get { return _deck; } }
	}
}
