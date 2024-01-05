using MTCG.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Interfaces.ICard
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
