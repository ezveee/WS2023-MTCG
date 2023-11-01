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
		private List<Card> _deck = new List<Card>();

		public List<Card> Deck { get { return _deck; } }
	}
}
