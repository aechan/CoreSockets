using System;
using System.Collections.Generic;
using System.Text;

namespace CoreSockets
{
    class TestProgram
    {
        public static void Main(string[] args)
        {
            WebSocketServer server = new WebSocketServer(8080);
            server.Start();

            server.OnMessage += PrintMessage;

            System.Console.ReadLine();
        }


        public static void PrintMessage(string message)
        {
            Console.WriteColor("[Client] ", ConsoleColor.Black, ConsoleColor.Magenta);
            System.Console.WriteLine(message);
        }
    }
}
