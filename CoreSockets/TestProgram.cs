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

            

            System.Console.ReadLine();
        }
    }
}
