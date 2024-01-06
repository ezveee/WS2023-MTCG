using MTCG.Cards;

namespace MTCG.Interfaces
{
	public interface ICard
	{
		public Guid Id { get; }

		public string Name { get; }

		public float Damage { get; }

		public CardType Type { get; }

		public ElementType Element { get; }

		float GetDamageAgainst(ICard card);
		float GetElementalFactorAgainst(ICard card);
	}
}
