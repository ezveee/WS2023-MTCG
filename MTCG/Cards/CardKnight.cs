using MTCG.Interfaces.ICard;

namespace MTCG.Cards
{
	public class CardKnight : Card
	{
		public override float GetDamageAgainst(ICard card)
		{
			if (card.Type == CardType.Spell && card.Element == ElementType.Water)
			{
				return -1;
			}

			return base.GetDamageAgainst(card);
		}
	}
}
