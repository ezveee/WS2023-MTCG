using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
	internal sealed class Player
	{
		private string _username;
		private float _elo;
		private float _level;
		/*
		 * different from elo -> elo for competitive games and ranking
		 * level -> gain exp based on how much you played
		 * rewards for gaining a level
		 */
		private List<Card> _deck = new List<Card>();

		public List<Card> Deck { get { return _deck; } }
	}
}
