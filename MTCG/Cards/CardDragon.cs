using MTCG.Interfaces.ICard;

namespace MTCG.Cards
{
	public class CardDragon : Card
	{
		public override float GetDamageAgainst(ICard card)
		{
			if (card.Type == CardType.Elf && card.Element == ElementType.Fire)
			{
				return 0;
			}

			return base.GetDamageAgainst(card);
		}
	}
}
