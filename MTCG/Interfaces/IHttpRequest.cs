using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Interfaces.IHttpRequest
{
	interface IHttpRequest
	{
		// add to be implemented functions
		// execution of request
		string GetResponse(string request);
	}
}
