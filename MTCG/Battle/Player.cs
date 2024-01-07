using MTCG.Interfaces;

namespace MTCG.Battle;

public sealed class Player
{
	public Player(string username, List<ICard> deck)
	{
		Username = username;
		Deck = deck;
	}

	public string Username { get; }
	public List<ICard> Deck { get; }
}
