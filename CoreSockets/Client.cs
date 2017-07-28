using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace CoreSockets.Service
{
    public class Client
    {
        public string ID;

        public WebSocketServer service;

        public TcpState state;

        public int index;

        public Client(string ID, WebSocketServer service)
        {
            this.ID = ID;
            this.service = service;
        }
    }
}
