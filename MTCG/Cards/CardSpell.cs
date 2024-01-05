using MTCG.Interfaces.ICard;

namespace MTCG.Cards
{
	public class CardSpell : Card
	{
		public override float GetDamageAgainst(ICard card)
		{
			if (card.Type == CardType.Kraken)
			{
				return 0;
			}

			return base.GetDamageAgainst(card);
		}
	}
}
