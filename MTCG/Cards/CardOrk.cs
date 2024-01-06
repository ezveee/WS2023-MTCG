using MTCG.Interfaces;

namespace MTCG.Cards
{
	public class CardOrk : Card
	{
		public override float GetDamageAgainst(ICard card)
		{
			if (card.Type == CardType.Wizard)
			{
				return 0;
			}

			return base.GetDamageAgainst(card);
		}
	}
}
