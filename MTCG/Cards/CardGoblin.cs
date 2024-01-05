using MTCG.Interfaces.ICard;

namespace MTCG.Cards
{
	public class CardGoblin : Card
	{
		public override float GetDamageAgainst(ICard card)
		{
			if (card.Type == CardType.Dragon)
			{
				return 0;
			}

			return base.GetDamageAgainst(card);
		}
	}
}
