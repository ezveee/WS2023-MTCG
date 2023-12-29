using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Database.Schemas
{
	public class TradingDeal
	{
		public string Id { get; set; }
		public string CardToTrade { get; set; }
		public string Type { get; set; }
		public float MinimumDamage { get; set; }

	}
}
