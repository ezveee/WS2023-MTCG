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

		public abstract void Fight();
		// example for getters and setters
		//public string FirstName
		//{
		//	get { return firstName; }
		//	set { firstName = value; }
		//}

		public Card(CardType cardType, ElementType elementType, string name, int attack)
		{
			_cardType = cardType;
			_elementType = elementType;
			_name = name;
			_attack = attack;
		}

	}
}
