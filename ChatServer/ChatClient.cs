using System.Net.Sockets;

namespace ChatServer
{
    public class ChatClient
    {
        public TcpClient TcpClient { get; set; }
        public string Username { get; set; }
    }
}