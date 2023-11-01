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
		private MonsterType _monsterType;
		public MonsterCard(MonsterType monsterType, ElementType elementType, int attack)
			: base(elementType, attack)
		{
			CardType = CardType.Monster;
			Name = Enum.GetName(typeof(ElementType), elementType) + Enum.GetName(typeof(MonsterType), monsterType);
			_monsterType = monsterType;
		}

		public MonsterType MonsterType { get { return _monsterType; } }
	}
}
