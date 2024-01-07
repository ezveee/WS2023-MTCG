using MTCG.Interfaces;

namespace MTCG.Cards;

public class CardKnight : Card
{
	public override float GetDamageAgainst(ICard card)
	{
		return card.Type == CardType.Spell && card.Element == ElementType.Water ? -1 : base.GetDamageAgainst(card);
	}
}
