﻿using MTCG.Cards;
using MTCG.Cards.MonsterCards;
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
		public static SpellEffect CheckElementEffectiveness(Card cardA, Card cardB)
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

		public static CardSpecialty CheckSpecialty(Card cardA, Card cardB)
		{
			CardType ownType = cardA.CardType;
			CardType opposingType = cardB.CardType;

			switch (ownType)
			{
				case CardType.Goblin when opposingType == CardType.Dragon:
				case CardType.Ork when opposingType == CardType.Wizard:
				case CardType.Spell when opposingType == CardType.Kraken:
				case CardType.Dragon
					when (opposingType == CardType.Elf && cardB.ElementType == ElementType.Fire):
					return CardSpecialty.CantAttack;

				case CardType.Knight
					when (opposingType == CardType.Spell && cardB.ElementType == ElementType.Water):
					return CardSpecialty.Dies;
			}

			return CardSpecialty.None;
		}

		public static int GetRandomCard(List<Card> deck)
		{
			var random = new Random();
			return random.Next(deck.Count);
		}

		public static bool IsPureMonsterFight(Card cardA, Card cardB)
		{
			if (cardA.CardType == CardType.Spell || cardB.CardType == CardType.Spell)
				return false;
			return true;
		}

		/// <summary>
		/// Checks if card A's damage changes somehow, based on specialties and effects regarding card B.
		/// </summary>
		/// <param name="cardA"></param>
		/// <param name="cardB"></param>
		/// <returns>Returns card A's final calculated damage. (-1: Dies, 0: Can't attack, Other: calculated Damage)</returns>
		public static float CalculateDamage(Card cardA, Card cardB)
		{
			// specialty check
			var specialty = CheckSpecialty(cardA, cardB);
			switch (specialty)
			{
				case CardSpecialty.CantAttack:
					return 0;
				case CardSpecialty.Dies:
					return -1;
			}

			// fight based on elements (at least one spell)
			if (!IsPureMonsterFight(cardA, cardB))
			{
				var effect = CheckElementEffectiveness(cardA, cardB);
				switch (effect)
				{
					case SpellEffect.Effective:
						return cardA.Attack * Constants.EffectiveMultiplier;
					case SpellEffect.NotEffective:
						return cardA.Attack * Constants.NotEffectiveMultiplier;
				}
			}

			// pure monster fight or no effect
			return cardA.Attack;
		}

		public void Battle(Player playerA, Player playerB)
		{
			var cardA = playerA.Deck[GetRandomCard(playerA.Deck)];
			var cardB = playerB.Deck[GetRandomCard(playerB.Deck)];



		}

	}
}
