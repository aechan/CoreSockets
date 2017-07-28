using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

namespace CoreSockets.Service
{
    public class WebSocketServer
    {
        public int port
        {
            get;
            private set;
        }

        public IPAddress ip
        {
            get;
            private set;
        }

        public List<Client> Clients
        {
            get;
            private set;
        }

        // flag to hop out of the listen loop
        private bool isListening;
        
        public delegate void OnMessageHandler(string msg, Client c);
        public event OnMessageHandler OnMessage = delegate { };


        private TcpListener _server;

        /// <summary>
        /// ClientRequestBytes is the client handshake request used for testing only.
        /// </summary>
        private static readonly string ClientRequestString = "GET / HTTP/1.1"+Environment.NewLine+
        "Host: 127.0.0.1" + Environment.NewLine +
        "Upgrade: websocket" + Environment.NewLine +
        "Connection: Upgrade" + Environment.NewLine +
        "Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==" + Environment.NewLine +
        "Origin: http://example.com" + Environment.NewLine +
        "Sec-WebSocket-Version: 13";

        private static readonly byte[] ClientRequestBytes = Encoding.UTF8.GetBytes(ClientRequestString);

        /// <summary>
        /// Generates a handshake response based on the given handshake request string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] ServerResponseBytes(string data) {
            return Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                + "Connection: Upgrade" + Environment.NewLine
                + "Upgrade: websocket" + Environment.NewLine
                + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                    SHA1.Create().ComputeHash(
                        Encoding.UTF8.GetBytes(
                            new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                        )
                    )
                ) + Environment.NewLine
                + Environment.NewLine);
        }

        //ctors
        public WebSocketServer() : this(IPAddress.Loopback, 80) { }
        public WebSocketServer(int port) : this(IPAddress.Loopback, port) { }
        public WebSocketServer(IPAddress ip, int port)
        {
            this.port = port;
            this.ip = ip;
            Clients = new List<Client>();
            _server = new TcpListener(ip, port);
        }


        public void Start()
        {
            System.Console.WriteLine("WebSocketServer started on {0}:{1}", ip, port);
            _server.Start();
            isListening = true;
            Listen();

        }

        private async void Listen()
        {

            while (isListening)
            {

                var tcpClient = await _server.AcceptTcpClientAsync();
                Console.WriteColor("[Server] ", ConsoleColor.Black, ConsoleColor.Cyan);
                Console.WriteLineColor("Client has connected");
                var networkStream = tcpClient.GetStream();
                
                var buffer = new byte[4096];
                Console.WriteColor("[Server] ", ConsoleColor.Black, ConsoleColor.Cyan);
                Console.WriteLineColor("Reading from client");
                var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                var request = Encoding.UTF8.GetString(buffer, 0, byteCount);

                // if a handshake is requested
                if (new Regex("^GET").IsMatch(request))
                {
                    Console.WriteColor("[Server] ", ConsoleColor.Black, ConsoleColor.Cyan);
                    Console.WriteLineColor("Client wrote" + Environment.NewLine + request);
                    await networkStream.WriteAsync(ServerResponseBytes(request), 0, ServerResponseBytes(request).Length);
                    Console.WriteColor("[Server] ", ConsoleColor.Black, ConsoleColor.Cyan);
                    Console.WriteLineColor("Handshake successful", ConsoleColor.Black, ConsoleColor.Yellow);

                    // after a successful handshake spawn a new thread and keep connection alive indefinitely.
                    var childSocketThread = new Thread(async () =>
                    {
                        // add new client to our collection for management.
                        Client client = new Client(Guid.NewGuid().ToString(), this);
                        Clients.Add(client);

                        while (tcpClient.Connected)
                        {
                            //var c = Clients.Find(r => r.ID.Equals(client.ID));

                            var buf = new byte[4096];
                            var count = await networkStream.ReadAsync(buf, 0, buf.Length);

                            OnMessage(GetDecodedData(buf, count), client);
                        }
                        
                    });
                    childSocketThread.Start();                        
                }

                
            }
        }

        public void Stop()
        {
            isListening = false;
        }

        public static async void ConnectAsTcpClient(int port)
        {
            using (var tcpClient = new TcpClient())
            {
                Console.WriteColor("[Client] ", ConsoleColor.Black, ConsoleColor.Magenta);
                Console.WriteLineColor("Connecting to server");
                await tcpClient.ConnectAsync("127.0.0.1", port);
                Console.WriteColor("[Client] ", ConsoleColor.Black, ConsoleColor.Magenta);
                Console.WriteLineColor("Connected to server");
                using (var networkStream = tcpClient.GetStream())
                {
                    Console.WriteColor("[Client] ", ConsoleColor.Black, ConsoleColor.Magenta);
                    System.Console.WriteLine("Writing request {0}", ClientRequestString);
                    await networkStream.WriteAsync(ClientRequestBytes, 0, ClientRequestBytes.Length);

                    var buffer = new byte[4096];
                    var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    var response = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteColor("[Client] ", ConsoleColor.Black, ConsoleColor.Magenta);
                    System.Console.WriteLine("Server response was {0}", response);

                    var helloMessage = Encoding.UTF8.GetBytes("Hello from client");
                    await networkStream.WriteAsync(helloMessage, 0, helloMessage.Length);
                }
            }
        }
        /// <summary>
        /// Decodes a Websocket message according to RFC 6455
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetDecodedData(byte[] buffer, int length)
        {
            byte b = buffer[1];
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;

            if (b - 128 <= 125)
            {
                dataLength = b - 128;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }

            if (b - 128 == 126)
            {
                dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
                keyIndex = 4;
                totalLength = dataLength + 8;
            }

            if (b - 128 == 127)
            {
                dataLength = (int)BitConverter.ToInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                keyIndex = 10;
                totalLength = dataLength + 14;
            }

            if (totalLength > length)
                return null;

            if (dataLength < 0)
                return null;

            byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };

            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }

            return Encoding.ASCII.GetString(buffer, dataIndex, dataLength);
        }

    }

}
