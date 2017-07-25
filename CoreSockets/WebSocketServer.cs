using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace CoreSockets
{
    class WebSocketServer
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
            _server = new TcpListener(ip, port);
        }


        public void Start()
        {
            
            System.Console.WriteLine("WebSocketServer started on {0}:{1}", ip, port);
            StartListener();
        }

        private async void StartListener()
        {
            _server.Start();
            var tcpClient = await _server.AcceptTcpClientAsync();
            Console.WriteColor("[Server] ", ConsoleColor.Black, ConsoleColor.Cyan);
            Console.WriteLineColor("Client has connected");
            using (var networkStream = tcpClient.GetStream())
            {
                
                var buffer = new byte[4096];
                Console.WriteColor("[Server] ", ConsoleColor.Black, ConsoleColor.Cyan);
                Console.WriteLineColor("Reading from client");
                var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
                if (new Regex("^GET").IsMatch(request))
                {
                    Console.WriteColor("[Server] ", ConsoleColor.Black, ConsoleColor.Cyan);
                    Console.WriteLineColor("Client wrote" + Environment.NewLine + request);
                    await networkStream.WriteAsync(ServerResponseBytes(request), 0, ServerResponseBytes(request).Length);
                    Console.WriteColor("[Server] ", ConsoleColor.Black, ConsoleColor.Cyan);
                    Console.WriteLineColor("Response has been written", ConsoleColor.Black, ConsoleColor.Yellow);
                }   
            }
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
                }
            }
        }

    }
}
