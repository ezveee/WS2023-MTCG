using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MTCG.Cards.MonsterCards
{
	internal class MonsterCard : Card
	{
		public MonsterCard(CardType monsterType, ElementType elementType, int attack)
			: base(elementType, attack)
		{
			CardType = monsterType;
			Name = Enum.GetName(typeof(ElementType), elementType) + Enum.GetName(typeof(CardType), monsterType);
		}

	}
}
