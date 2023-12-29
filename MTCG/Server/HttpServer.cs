using MTCG.Database;
using MTCG.Interfaces.IHttpRequest;
using MTCG.Server.HttpRequests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
namespace MTCG.Server
{
	internal class HttpServer
	{
		private static HttpServer _instance;
		private readonly TcpListener listener;
		private static readonly Dictionary<string, IHttpRequest> routeTable = new();

		private HttpServer()
		{
			listener = new TcpListener(IPAddress.Loopback, Constants.HttpServerPort);
			InitializRoutes();
		}

		public static HttpServer Instance
		{
			get
			{
				_instance ??= new HttpServer();
				return _instance;
			}
		}

		private static void InitializRoutes()
		{
			// add keys and instances of classes to dictionary
			routeTable["POST /users"] = new PostUser();
			routeTable["POST /sessions"] = new PostSession();
			routeTable["POST /packages"] = new PostPackage();
			// implementation still missing
			routeTable["GET /users"] = new GetUser(); // TODO: change cause it gets a specific username
			routeTable["PUT /users"] = new PutUser(); // TODO: change cause it gets a specific username
			routeTable["POST /transactions"] = new PostTransaction();
			routeTable["GET /cards"] = new GetStack();
			routeTable["GET /deck"] = new GetDeck();
			routeTable["PUT /deck"] = new PutDeck();
			routeTable["GET /stats"] = new GetStats();
			routeTable["GET /scoreboard"] = new GetScoreboard();
			routeTable["POST /battles"] = new PostBattle();
			routeTable["GET /tradings"] = new GetTradings();
			routeTable["POST /tradings"] = new PostTrading(); // TODO: change cause it gets a specific trade deal id
			routeTable["DELETE /tradings"] = new DeleteTrading(); // TODO: change cause it gets a specific trade deal id

		}

		public void Start()
		{
			listener.Start();
			Console.WriteLine($"Server started on localhost:{Constants.HttpServerPort}\n");

			while (true)
			{
				// accept client connection
				var clientSocket = listener.AcceptTcpClient();
				Console.WriteLine("--------------------------------------------------");
				Console.WriteLine("Client connected.");

				// create and start client thread
				var clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
				clientThread.Start(clientSocket);

			}
		}

		public void Stop()
		{
			listener.Stop();
		}

		static void HandleClient(object obj)
		{
			TcpClient client = (TcpClient)obj;
			// check if client socket is connected
			if (!client.Connected)
			{
				Console.WriteLine("Client not connected.");
				return;
			}

			NetworkStream stream = client.GetStream();

			// get http request
			StringBuilder requestBuilder = new();
			byte[] buffer = new byte[1024];
			int bytesRead;

			do
			{
				bytesRead = stream.Read(buffer, 0, buffer.Length);
				requestBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

			} while (bytesRead == buffer.Length);

			string request = requestBuilder.ToString();

			if (!string.IsNullOrEmpty(request))
			{
				Console.WriteLine(request);

				// get response based on request
				string response = HandleRequest(request);
				byte[] responseBytes = Encoding.ASCII.GetBytes(response);
				stream.Write(responseBytes, 0, responseBytes.Length);
			}

			client.Close();
			Console.WriteLine("Client disconnected.");
			Console.WriteLine("--------------------------------------------------");
		}

		static string HandleRequest(string request)
		{
			string route = GetRoute(request);

			if (!routeTable.ContainsKey(route))
				return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n404 Not Found";

			return routeTable[route].GetResponse(request);
		}

		static string GetRoute(string request)
		{
			string[] lines = request.Split('\n');
			string[] tokens = lines[0].Split(' ');
			string method = tokens[0];
			string fullPath = tokens[1];
			string[] pathComponents = fullPath.Split('/');

			return method + " /" + pathComponents[1];
		}

	}

}
