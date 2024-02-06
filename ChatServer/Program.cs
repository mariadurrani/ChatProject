using ChatServer;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

class Program
{
    static TcpListener server;
    static List<ChatClient> clients = new List<ChatClient>();
    static IMongoCollection<BsonDocument> collection;
    static IMongoCollection<BsonDocument> userCollection;

    static void Main(string[] args)
    {
        // Connect to MongoDB
        var mongoClient = new MongoClient("mongodb://localhost:27017");
        var database = mongoClient.GetDatabase("chat");
        collection = database.GetCollection<BsonDocument>("chatmessages");
        userCollection = database.GetCollection<BsonDocument>("user");

        // Start server
        server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Console.WriteLine("Server started, waiting for connections...");

        while (true)
        {
            // Accept client connection
            TcpClient client = server.AcceptTcpClient();
            var chatClient = new ChatClient();
            chatClient.TcpClient = client;

            clients.Add(chatClient);

            Console.WriteLine("Client connected.");

            // Start a separate thread to handle client communication
            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(chatClient);
        }
    }

    static void HandleClient(object obj)
    {
        ChatClient chatClient = (ChatClient)obj;
        TcpClient client = chatClient.TcpClient;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        while (true)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                clients.Remove(chatClient);
                Console.WriteLine("Client disconnected.");
                break;
            }

            string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            Console.WriteLine("Client: " + receivedMessage);

            var split = receivedMessage.Split(' ');
            string messageToSend;

            if (split.Length > 0)
            {
                var action = split[0];
                switch (action)
                {
                    case "register":
                        Register(split[1], split[2]);
                        SendMessageClient(chatClient, "You are now registered");
                        break;
                    case "login":
                        var isLoggedIn = Login(chatClient, split[1], split[2]);
                        if (isLoggedIn)
                        {
                            SendMessageClient(chatClient, "Login successful. Welcome, " + chatClient.Username);
                            SendMessageClient(chatClient, "Enter 'send <your message>' to send a message to everyone.");
                            SendMessageClient(chatClient, "Enter 'private <your message> <user>' to send a private message to a user.");

                        }
                        else
                        {
                            SendMessageClient(chatClient, "Login failed. Invalid username or password.");
                        }
                        break;
                    case "send":

                        //Remove first word and join the rest with space
                        messageToSend = string.Join(" ", split.Skip(1));

                        // Send message to all clients
                        SendMessageAll(chatClient, messageToSend);
                        break;
                    case "private":
                        //Remove first 2 words and join the rest with space
                        messageToSend = string.Join(" ", split.Skip(2));

                        // Send message to one client
                        SendMessageAll(chatClient, messageToSend, split[1]);
                        break;
                }
            }
        }

        // Close client connection
        client.Close();
    }

    private static bool Login(ChatClient chatClient, string username, string password)
    {
        //load data from db
        bool validated = ValidateUser(username, password);
        if (validated)
        {
            chatClient.Username = username;
        }
        else
        {
            chatClient.Username = "";
        }

        return validated;
    }

    private static void Register(string username, string password)
    {
        var document = new BsonDocument
        {
            { "username", username },
            { "password", password },
            { "timestamp", DateTime.Now }
        };
        userCollection.InsertOne(document);
    }

    private static void SendMessageClient(ChatClient chatClient, string changedMessage)
    {
        var c = chatClient.TcpClient;

        NetworkStream clientStream = c.GetStream();
        byte[] buffer = Encoding.ASCII.GetBytes(changedMessage);
        clientStream.Write(buffer, 0, buffer.Length);
        clientStream.Flush();

        StoreMessage(changedMessage);
    }

    private static void SendMessageAll(ChatClient chatClient, string receivedMessage, string privateDisplayName = "")
    {
        foreach (ChatClient cc in clients)
        {
            if (string.IsNullOrWhiteSpace(cc.Username))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(chatClient.Username))
            {
                var changedMessage = chatClient.Username + ": " + receivedMessage;

                if (!string.IsNullOrWhiteSpace(privateDisplayName))
                {
                    if (cc.Username == privateDisplayName)
                    {
                        SendMessageClient(cc, changedMessage);
                    }
                }
                else
                {
                    SendMessageClient(cc, changedMessage);
                }
            }
        }
    }

    static void StoreMessage(string message)
    {
        var document = new BsonDocument
        {
            { "message", message },
            { "timestamp", DateTime.Now }
        };
        collection.InsertOne(document);
    }

    static bool ValidateUser(string username, string password)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("username", username);
        var user = userCollection.Find(filter).FirstOrDefault();
        if (user != null)
        {
            string storedPassword = user["password"].AsString;
            return password == storedPassword;
        }
        return false;
    }

}