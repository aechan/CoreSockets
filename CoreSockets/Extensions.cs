using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;

namespace CoreSockets
{
    public static class Console
    {
        public static void WriteColor(string message, ConsoleColor backcolor = ConsoleColor.Black, ConsoleColor forecolor = ConsoleColor.White)
        {
            System.Console.ForegroundColor = forecolor;
            System.Console.BackgroundColor = backcolor;
            System.Console.Write(message);
            System.Console.ResetColor();
        }

        public static void WriteLineColor(string message, ConsoleColor backcolor = ConsoleColor.Black, ConsoleColor forecolor = ConsoleColor.White)
        {
            System.Console.ForegroundColor = forecolor;
            System.Console.BackgroundColor = backcolor;
            System.Console.WriteLine(message);
            System.Console.ResetColor();
        }
    }

    public static class TcpExtensions
    {
        public static TcpState GetState(this TcpClient tcpClient)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }
    }
}
