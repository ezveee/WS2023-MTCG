using MTCG.Database;
using MTCG.Server;

namespace MTCG
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			DBManager dbManager = DBManager.Instance;
			HttpServer server = HttpServer.Instance;

			try
			{
				server.Start();
			}
			finally
			{
				server.Stop();
				dbManager.CloseConnection();
			}
		}
	}
}