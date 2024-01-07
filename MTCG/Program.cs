using MTCG.Database;
using MTCG.Interfaces;
using MTCG.Server;

namespace MTCG;

public static class Program
{
	private static void Main(string[] args)
	{
		IDataAccess dataAccess = new DataAccess();
		dataAccess.DbSetup();
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