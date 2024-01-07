using MTCG.Interfaces;

namespace MTCG.Cards;

public class CardGoblin : Card
{
	public override float GetDamageAgainst(ICard card)
	{
		return card.Type == CardType.Dragon ? 0 : base.GetDamageAgainst(card);
	}
}
