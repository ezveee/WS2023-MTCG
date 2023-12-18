using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
	internal class HttpServer
	{
		private readonly TcpListener listener;
		private static readonly Dictionary<string, IHttpRequest> routeTable = new();

		public HttpServer()
		{
			this.listener = new TcpListener(IPAddress.Loopback, Constants.HttpServerPort);
			InitializRoutes();
		}

		private static void InitializRoutes()
		{
			// add keys and instances of classes to dictionary
			routeTable["GET /hello"] = new Hello();
			routeTable["GET /goodbye"] = new Goodbye();
			routeTable["GET /users"] = new GetUsers();
		}

		interface IHttpRequest
		{
			// add to be implemented functions
			// execution of request
			string GetResponse();
		}

		// temp classes
		class Hello : IHttpRequest
		{
			public string GetResponse()
			{
				return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, World!";
			}
		}

		class Goodbye : IHttpRequest
		{
			public string GetResponse()
			{
				return "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nGoodbye, World!";
			}
		}

		class GetUsers : IHttpRequest
		{
			public string GetResponse()
			{
				// get users
				return "";
			}
		}

		public void Start()
		{
			this.listener.Start();
			Console.WriteLine($"Server started on localhost:{Constants.HttpServerPort}");

			while (true)
			{
				// accept client connection
				var clientSocket = this.listener.AcceptTcpClient();
				Console.WriteLine("Client connected.");

				// create and start client thread
				var clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
				clientThread.Start(clientSocket);

			}
		}

		public void Stop()
		{
			this.listener.Stop();
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
			StringBuilder requestBuilder = new StringBuilder();
			byte[] buffer = new byte[1024];
			int bytesRead;

			do
			{
				bytesRead = stream.Read(buffer, 0, buffer.Length);
				requestBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

			} while (bytesRead == buffer.Length);

			string request = requestBuilder.ToString();

			if (!String.IsNullOrEmpty(request))
			{
				Console.WriteLine(request);

				// get response based on request
				string response = HandleRequest(request);
				byte[] responseBytes = Encoding.ASCII.GetBytes(response);
				stream.Write(responseBytes, 0, responseBytes.Length);
			}

			client.Close();
			Console.WriteLine("Client disconnected.");

		}

		static string HandleRequest(string request)
		{
			string route = GetRoute(request);

			if (!HttpServer.routeTable.ContainsKey(route))
				return "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n404 Not Found";

			return HttpServer.routeTable[route].GetResponse();
		}

		static string GetRoute(string request)
		{
			string[] lines = request.Split('\n');
			string[] tokens = lines[0].Split(' ');
			string method = tokens[0];
			string path = tokens[1];
			return method + " " + path;
		}
	}

}
