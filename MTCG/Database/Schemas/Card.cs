using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Database.Schemas
{
	internal class Card
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public float Damage { get; set; }
	}
}
