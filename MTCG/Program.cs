using MTCG.Database;
using MTCG.Server;

namespace MTCG
{
	// TODO: check if responses in resource file are even necessary
	// do descriptions from yaml get sent back in response or are they just for clarification?
	public static class Program
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