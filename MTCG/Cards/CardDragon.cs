using MTCG.Interfaces;

namespace MTCG.Cards;

public class CardDragon : Card
{
	public override float GetDamageAgainst(ICard card)
	{
		return card.Type == CardType.Elf && card.Element == ElementType.Fire ? 0 : base.GetDamageAgainst(card);
	}
}
