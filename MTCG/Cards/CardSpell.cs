using MTCG.Interfaces;

namespace MTCG.Cards;

public class CardSpell : Card
{
	public override float GetDamageAgainst(ICard card)
	{
		return card.Type == CardType.Kraken ? 0 : base.GetDamageAgainst(card);
	}
}
