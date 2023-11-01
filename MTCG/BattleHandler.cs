using MTCG.Cards;
using MTCG.Cards.SpellCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
	internal class BattleHandler
	{
		public SpellEffect CheckEffectiveness(Card cardA, Card cardB)
		{
			ElementType ownType = cardA.ElementType;
			ElementType opposingType = cardB.ElementType;

			if (ownType == opposingType) { return SpellEffect.NoEffect; }

			switch (ownType)
			{
				case ElementType.Water when opposingType == ElementType.Fire:
				case ElementType.Fire when opposingType == ElementType.Regular:
				case ElementType.Regular when opposingType == ElementType.Water:
					return SpellEffect.Effective;
			}

			return SpellEffect.NotEffective;
		}

	}
}
