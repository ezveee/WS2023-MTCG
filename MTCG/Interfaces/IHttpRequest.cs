using MTCG.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Interfaces.IHttpRequest
{
	interface IHttpRequest
	{
		string GetResponse(string request);
	}
}
