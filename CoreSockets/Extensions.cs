using System;
using System.Collections.Generic;
using System.Text;

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
}
