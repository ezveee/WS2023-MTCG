using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards.SpellCards
{
	internal class SpellCard : Card
	{
		public SpellCard(CardType cardType, ElementType elementType, string name, int attack)
			: base(cardType, elementType, name, attack)
		{

		}
	}
}
