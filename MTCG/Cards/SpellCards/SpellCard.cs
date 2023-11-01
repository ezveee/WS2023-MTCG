using MTCG.Cards.MonsterCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Cards.SpellCards
{
	internal class SpellCard : Card
	{
		public SpellCard(ElementType elementType, int attack)
			: base(elementType, attack)
		{
			CardType = CardType.Spell;
			Name = Enum.GetName(typeof(ElementType), elementType) + "Spell";
		}
	}
}
