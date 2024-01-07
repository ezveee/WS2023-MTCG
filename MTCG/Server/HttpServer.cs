using MTCG.Interfaces;
using MTCG.Server.HttpRequests;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MTCG.Server;

public class HttpServer
{
	private readonly TcpListener _listener;
	private readonly IDataAccess _dataAccess;
	private static readonly Dictionary<string, IHttpRequest> _routeTable = new();
	private readonly bool _keepRunning = true;

	public HttpServer(IDataAccess dataAccess)
	{
		_dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
		_listener = new TcpListener(IPAddress.Loopback, Constants.HttpServerPort);
		InitializRoutes();
	}

	private void InitializRoutes()
	{
		_routeTable["POST /users"] = new PostUser(_dataAccess);
		_routeTable["POST /sessions"] = new PostSession(_dataAccess);
		_routeTable["POST /packages"] = new PostPackage(_dataAccess);
		_routeTable["GET /users"] = new GetUser(_dataAccess);
		_routeTable["PUT /users"] = new PutUser(_dataAccess);
		_routeTable["POST /transactions"] = new PostTransaction(_dataAccess);
		_routeTable["GET /cards"] = new GetStack(_dataAccess);
		_routeTable["GET /deck"] = new GetDeck(_dataAccess);
		_routeTable["PUT /deck"] = new PutDeck(_dataAccess);
		_routeTable["GET /stats"] = new GetStats(_dataAccess);
		_routeTable["GET /scoreboard"] = new GetScoreboard(_dataAccess);
		_routeTable["POST /battles"] = new PostBattle(_dataAccess);
		_routeTable["GET /tradings"] = new GetTradings(_dataAccess);
		_routeTable["POST /tradings"] = new PostTrading(_dataAccess);
		_routeTable["DELETE /tradings"] = new DeleteTrading(_dataAccess);
		_routeTable["POST /campfire"] = new PostCampfire(_dataAccess); // mandatory unique feature
	}

	public void Start()
	{
		_listener.Start();
		Console.WriteLine($"Server started on localhost:{Constants.HttpServerPort}\n");

		// didn't quite work since program gets stuck on AcceptTcpClient()
		//Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)
		//{
		//	e.Cancel = true;
		//	_keepRunning = false;
		//};

		while (_keepRunning)
		{
			// accept client connection
			TcpClient clientSocket = _listener.AcceptTcpClient();
			Console.WriteLine("--------------------------------------------------");
			Console.WriteLine("Client connected.");

			// create and start client thread
			Thread clientThread = new(new ParameterizedThreadStart(HandleClient));
			clientThread.Start(clientSocket);
		}
	}

	public void Stop()
	{
		_listener.Stop();
	}

	private static void HandleClient(object obj)
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

	private static string HandleRequest(string request)
	{
		string route = GetRoute(request);

		return !_routeTable.ContainsKey(route)
			? "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n404 Not Found"
			: _routeTable[route].GetResponse(request);
	}

	private static string GetRoute(string request)
	{
		string[] lines = request.Split('\n');
		string[] tokens = lines[0].Split(' ');
		string method = tokens[0];
		string fullPath = tokens[1];
		string[] pathComponents = fullPath.Split('/');

		return method + " /" + pathComponents[1];
	}
}
