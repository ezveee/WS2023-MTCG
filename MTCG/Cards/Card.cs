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

		protected Card(CardType cardType, ElementType elementType, string name, int attack)
		{
			_cardType = cardType;
			_elementType = elementType;
			_name = name;
			_attack = attack;
		}

		public CardType CardType
		{
			get { return _cardType; }
		}
		public ElementType ElementType
		{
			get { return _elementType; }
		}
		public string Name
		{
			get { return _name; }
		}
		public int Attack
		{
			get { return _attack; }
		}

	}
}
