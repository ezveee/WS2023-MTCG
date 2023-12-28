using MTCG.Cards;
using MTCG.Database;
using MTCG.Database.Schemas;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Server
{
	public static class CardUtility
	{
		private static Dictionary<string, (CardType Type, ElementType Element)> cardCategories = new()
		{
			{ "FireSpell", ( CardType.Spell, ElementType.Fire ) },
			{ "WaterSpell", ( CardType.Spell, ElementType.Water ) },
			{ "RegularSpell", ( CardType.Spell, ElementType.Regular ) },
			{ "FireGoblin", ( CardType.Goblin, ElementType.Fire ) },
			{ "WaterGoblin", ( CardType.Goblin, ElementType.Water ) },
			{ "RegularGoblin", ( CardType.Goblin, ElementType.Regular ) },
			{ "Dragon", ( CardType.Dragon, ElementType.Fire ) },
			{ "Wizard", ( CardType.Wizard, ElementType.Regular ) },
			{ "Ork", ( CardType.Ork, ElementType.Regular ) },
			{ "Knight", ( CardType.Knight, ElementType.Regular ) },
			{ "Kraken", ( CardType.Kraken, ElementType.Water ) },
			{ "FireElf", ( CardType.Elf, ElementType.Fire ) },
			{ "WaterElf", ( CardType.Elf, ElementType.Water ) },
			{ "RegularElf", ( CardType.Elf, ElementType.Regular ) },
			{ "FireTroll", ( CardType.Troll, ElementType.Fire ) },
			{ "WaterTroll", ( CardType.Troll, ElementType.Water ) },
			{ "RegularTroll", ( CardType.Troll, ElementType.Regular ) }
		};

		public static CardType GetCardTypeByName(string name)
		{
			return cardCategories[name].Type;
		}

		public static ElementType GetElementTypeByName(string name)
		{
			return cardCategories[name].Element;
		}
	}
}
