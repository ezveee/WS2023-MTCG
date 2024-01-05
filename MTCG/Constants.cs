using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
	static class Constants
	{
		// http server
		public const int HttpServerPort = 10001;

		// login
		public const int SessionTimeoutInMinutes = 5000;

		public const int PackageCost = 5;

		// battle logic
		public const int MaxRounds = 100;

		public const float EffectiveMultiplier = 2f;
		public const float NotEffectiveMultiplier = .5f;
	}
}
