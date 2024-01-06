using MTCG.Database;
using MTCG.Interfaces;
using MTCG.Server;

namespace MTCG
{
	public static class Program
	{
		static void Main(string[] args)
		{
			IDataAccess dataAccess = new DataAccess();
			dataAccess.DbSetup();

			//DBManager dbManager = new(dataAccess);
			HttpServer server = new(dataAccess);

			try
			{
				server.Start();
			}
			finally
			{
				server.Stop();
				dataAccess.CloseConnection();
			}
		}
	}
}