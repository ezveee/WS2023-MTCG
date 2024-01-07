using MTCG.Interfaces;

namespace MTCG.Cards;

public class CardOrk : Card
{
	public override float GetDamageAgainst(ICard card)
	{
		return card.Type == CardType.Wizard ? 0 : base.GetDamageAgainst(card);
	}
}
