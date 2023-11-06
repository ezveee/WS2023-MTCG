using MTCG.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
	internal abstract class Card
	{
		private CardType _cardType;
		private ElementType _elementType;
		private string _name;
		private int _attack;

		protected Card(ElementType elementType, int attack)
		{
			_elementType = elementType;
			_attack = attack;
		}

		public CardType CardType { get; set; }
		public ElementType ElementType { get { return _elementType; } }
		public string Name { get; set; }
		public int Attack { get { return _attack; } }

	}
}
