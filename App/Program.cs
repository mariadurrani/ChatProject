using System;


class Chat
{
    static Dictionary<string, string> users = new Dictionary<string, string>();
    static List<string> loggedInUsers = new List<string>();
    
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Login or register?");
            Console.WriteLine("1. Register");
            Console.WriteLine("2. Login");
            
            int val;
            if (int.TryParse(Console.ReadLine(), out val))
            {
                switch (val)
                {
                    case 1:
                        CreateAccount();
                        break;
                    case 2:
                        Login();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Enter a number.");
            }
            
            Console.WriteLine();
        }
    }

    static void CreateAccount()
    {
        Console.WriteLine("Enter a username:");
        string name = Console.ReadLine();
        
        Console.WriteLine("Enter a password:");
        string password = Console.ReadLine();

        if (!users.ContainsKey(name))
        {
            users.Add(name, password);
            Console.WriteLine("Account registered");
        }
        else
        {
            Console.WriteLine("This account already exists");
        }
    }

    static void Login()
    {
        Console.WriteLine("Enter your username");
        string name = Console.ReadLine();
        
        Console.WriteLine("Enter your password");
        string password = Console.ReadLine();


        if (users.ContainsKey(name) && users[name] == password)
        {
            if (!loggedInUsers.Contains(name))
            {
                loggedInUsers.Add(name);
                Console.WriteLine($"{name} has logged in!");
            }
            else
            {
                Console.WriteLine($"{name} has already logged in");
            }
        }
        else
        {
            Console.WriteLine("Invalid username or password");
        }
    }
    
}