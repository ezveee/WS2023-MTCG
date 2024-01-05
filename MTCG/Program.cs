using MTCG.Battle;
using MTCG.Database;
using MTCG.Server;

namespace MTCG
{
	public static class Program
	{
		static void Main(string[] args)
		{
			DBManager dbManager = DBManager.Instance;
			HttpServer server = HttpServer.Instance;
			BattleManager battleManager = BattleManager.Instance;

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