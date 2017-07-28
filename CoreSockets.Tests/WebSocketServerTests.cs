using CoreSockets.Service;
using System.Text;
using Xunit;

namespace CoreSockets.Tests
{
    public class WebSocketServerTests
    {
        WebSocketServer _server;

        [Fact]
        public void GetDecodedData_ReturnNullGivenEmptyBuffer()
        {
            // byte array with no data
            var buf = new byte[4096];
            var len = buf.Length;

            Assert.Null(WebSocketServer.GetDecodedData(buf, len));
        }
        [Fact]
        public void GetDecodedData_ReturnNullGivenLengthLessThanDataLength()
        {
            int len = 1;
            byte[] buf = Encoding.UTF8.GetBytes("some data");

            Assert.Null(WebSocketServer.GetDecodedData(buf, len));
        }
        [Fact]
        public void GetDecodedData_ReturnGivenStringFromBytes()
        {
            string message = "Hello World!";
            byte[] buf = new byte[4096];
            buf = Encoding.UTF8.GetBytes(message);
            int len = buf.Length;

            Assert.Equal(message, WebSocketServer.GetDecodedData(buf, len));
        }

        [Fact]
        public void WebSocketServerOnMessage_RaisedOnClientMessage()
        {
            _server.Start();
            WebSocketServer.ConnectAsTcpClient(8080);

            
        }
    }
}
