using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Database.Schemas
{
	public class Card
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public float Damage { get; set; }
	}
}
