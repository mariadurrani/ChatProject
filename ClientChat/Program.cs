using System;
using System.Net.Sockets;
using System.Text;

class Program
{
    static TcpClient client;
    static NetworkStream stream;

    static void Main(string[] args)
    {
        // Connect to server
        client = new TcpClient("127.0.0.1", 8888);
        Console.WriteLine("Connected to server.");

        // Get server stream
        stream = client.GetStream();

        // Start a separate thread to receive messages from server
        System.Threading.Thread receiveThread = new System.Threading.Thread(ReceiveMessages);
        receiveThread.Start();

        Console.Write("Username: ");
        var username = Console.ReadLine();
        Console.Write("Password: ");
        var password = Console.ReadLine();


        SendMessage("register " + username + " " + password);



        // Send messages to server
        string message;
        while (true)
        {
            message = Console.ReadLine();
            SendMessage(message);
        }
    }

    static void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
                break;

            string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine(receivedMessage);
        }
    }

    static void SendMessage(string message)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }
}