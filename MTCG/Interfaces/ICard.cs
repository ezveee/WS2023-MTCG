using MTCG.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Interfaces.ICard
{
	internal interface ICard
	{
		public Guid Id { get; }

		public string Name { get; }

		public int Damage { get; }

		public CardType Type { get; }

		public ElementType Element { get; }

		int GetDamageAgainst(ICard card);
	}
}
