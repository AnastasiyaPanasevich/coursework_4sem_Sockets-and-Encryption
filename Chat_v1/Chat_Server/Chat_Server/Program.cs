using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using MyRSALibrary_v1;

class Server
{
    //private static readonly List<ClientHandler> clients = new List<ClientHandler>();
    private static readonly List<User> clients = new List<User>();
    private static TcpListener server;
    private static bool isRunning = true;
    private static readonly object clientsLock = new object();
    private static int userCount = 0; // Счетчик пользователей
    public static User serverUser; // Declaring serverUser variable
    public static string activeClients="";
    public static int countUsersActive;
    static void Main()
    {
        // Задайте IP-адрес и порт для прослушивания
        //IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

        IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

        int port = 8888;

        server = new TcpListener(ipAddress, port);

        // Запуск прослушивания
        server.Start();

        // Вывод информации о слушающем сокете
        IPEndPoint iPEndPoint = (IPEndPoint)server.Server.LocalEndPoint;
        Console.WriteLine("Дескриптор сокета: {0}\nIP-адрес: {1}\nПорт: {2}",
            server.Server.Handle, iPEndPoint.Address, iPEndPoint.Port);

        Console.WriteLine("Сервер запущен. Ожидание подключений...");

        // Прослушивание подключений в отдельном потоке
        Thread listenThread = new Thread(ListenForClients);
        listenThread.Start();


        //////////////////////////////////////////////////////////////////////////////
        //var cryptoServiceProvider = new RSACryptoServiceProvider(512); //512 - Długość klucza
        //var privateKey = cryptoServiceProvider.ExportParameters(true); //Generowanie klucza prywatnego
        //var publicKey = cryptoServiceProvider.ExportParameters(false); //Generowanie klucza publiczny
        //string openKey = MyRSALibrary_v1.RSA.GetKeyString(publicKey);
        //string secretKey = MyRSALibrary_v1.RSA.GetKeyString(privateKey);
        // Создание сервера в виде пользователя
        //serverUser = new User()
        //{
        //    Index = userCount,
        //    Login = "server",
        //    openKey = openKey
        //};
        //Console.WriteLine($"server open key {serverUser.openKey}");
        ////clients.Add(serverUser);
        ////userCount++;
        //foreach (var user in clients)
        //{
        //    Console.WriteLine($"Индекс: {user.Index}, Логин: {user.Login}");
        //}
        //////////////////////////////////////////////////////////////////////////////
        

        // Обработка команд администратора в основном потоке
        while (true)
        {
            string command = Console.ReadLine();
            if (command.ToLower() == "exit")
            {
                isRunning = false;
                break;
            }
        }

        // Завершение работы сервера
        server.Stop();
        Console.WriteLine("Сервер остановлен.");
    }


    // Метод для прослушивания подключений клиентов
    static void ListenForClients()
    {
        while (isRunning)
        {
            TcpClient tcpClient = server.AcceptTcpClient();
            Console.WriteLine("Новое подключение.");

            // Создание нового обработчика клиента
            ClientHandler clientHandler = new ClientHandler(tcpClient);

            //lock (clientsLock)
            //{
            //    clients.Add(clientHandler);
            //}

            // Запуск обработки клиента в отдельном потоке
            Thread clientThread = new Thread(clientHandler.HandleClient);
            clientThread.Start();
        }
    }

    public static void AddUserAndPrintClients(ClientHandler clientHandler)
    {
        // Создание нового пользователя
        User newUser = new User();
        newUser.Index = userCount;
        newUser.Login = clientHandler.GetClientName();
        //newUser.openKey = clientHandler.GetClientOpenKey();
        newUser.ClientHandler = clientHandler;
        clients.Add(newUser);
        userCount++;
        // Вывод массива клиентов на консоль
        if (clients.Count > 0)
        {
            Console.WriteLine("Массив клиентов:");
            activeClients = "";
            foreach (var user in clients)
            {
                Console.WriteLine($"Индекс: {user.Index}, Логин: {user.Login}");
                ActiveUsersList(user.Index.ToString(), user.Login.ToString());

                //Console.WriteLine($"Индекс: {user.Index}, Логин: {user.Login}, Публичный ключ: {user.openKey}");
                //ActiveUsersList(user.Index.ToString(), user.openKey);
            }
        }
        else
        {
            Console.WriteLine("Массив клиентов пуст.");
        }
    }
    public static void ActiveUsersList(string index, string login)
    {
        activeClients += $";{index};{login}";
        Console.WriteLine($"\n\n#########################\namount of active users: {clients.LongCount()}\n#########################\n\n");
        ActiveAmount();
    }
    public static void ActiveAmount()
    {
        countUsersActive = clients.Count;
    }

    // Метод для удаления клиента из списка при отключении
    public static void RemoveClient(ClientHandler clientHandler)
    {
        //lock (clientsLock)
        //{
        //    clients.Remove(clientHandler);
        //}

        lock (clientsLock)
        {
            User userToRemove = clients.Find(user => user.Login == clientHandler.GetClientName());
            if (userToRemove != null)
            {
                clients.Remove(userToRemove);
            }
        }
    }

    // Метод для рассылки сообщения всем клиентам
    public static void BroadcastMessage(string command, string message, string senderName)
    {
        string formattedMessage;
        if (!string.IsNullOrEmpty(senderName))
        {
            formattedMessage = $"{command};\n{DateTime.Now.ToString("HH/mm/ss")} - {senderName};\n{message}";
        }
        else formattedMessage = $"{command};{message}";

        lock (clientsLock)
        {
            foreach (var client in clients)
            {
                ClientHandler clientHandler = client.ClientHandler;
                clientHandler.SendMessage(formattedMessage);
                if (!string.IsNullOrEmpty(senderName))
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {senderName} отправил сообщение в чат");
                    Console.WriteLine($"\n\n сообщение\n\n {message} \n\n");
                }
                else Console.WriteLine($"\n\n обновлено количество онлайн для пользователей \n\n");

            }
        }
    }





}

class ClientHandler
{
    private TcpClient client;
    private NetworkStream stream;
    private string clientName;
    private string openKey;
    public User clients;

    public ClientHandler(TcpClient tcpClient)
    {
        client = tcpClient;
    }


    public string GetClientName()
    {
        return clientName;
    }

    public string GetClientOpenKey()
    {
        return openKey;
    }
    public void setUserLogin(string message)
    {
        clientName = message;
        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {clientName} присоединился к чату");
        // Отправка сообщения о подключении пользователя другим клиентам
        Server.BroadcastMessage("J",$"присоединился к чату.\n", clientName);
    }

    // Метод для обработки клиента
    public void HandleClient()
    {
        stream = client.GetStream();


        while (true)
        {
            byte[] buffer = new byte[client.ReceiveBufferSize];
            string command = "\0", cmnd = "";
            string message = string.Empty, number = string.Empty, openKey = string.Empty;
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    // Клиент отключился
                    break;
                }

                // Преобразование принятых данных в строку
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Разделение сообщения на номер и текст
                string[] parts = receivedMessage.Split(';');

                command = parts[0];

                switch (command)
                {
                    case "L":
                        Console.WriteLine("Command " + command + " login");
                        message = parts[1];
                        //openKey = parts[2];
                        setUserLogin(message);
                        //SendMessage($"OpenKServer;{Server.serverUser.openKey}");
                        Server.AddUserAndPrintClients(this); // Вызов функции после обработки команды "L"
                        cmnd = "U";
                        Server.BroadcastMessage(cmnd,$"{Server.countUsersActive}","");
                        break;

                    case "M":
                        Console.WriteLine("Command " + command + " message");
                        message = parts[1];
                        cmnd = "M";
                        Server.BroadcastMessage(cmnd, message, clientName);
                        break;

                    case "E":
                        Console.WriteLine("Command " + command + " exit");
                        message = parts[1];
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {message} покинул чат");
                        break;
                    default:
                        Console.WriteLine("Command " + command + " doesn't exist in this context");
                        break;


                }
                // Очистка буфера
                Array.Clear(buffer, 0, buffer.Length);

            }
            catch
            {
                // Ошибка чтения данных, клиент отключается
                break;
            }
        }

        // Закрытие соединения
        stream.Close();
        client.Close();

        // Удаление клиента из списка
        Server.RemoveClient(this);
    }

    // Метод для отправки сообщения клиенту
    public void SendMessage(string message)
    {
        //string {<message>=... <key>=key}
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        try
        {
            stream.Write(buffer, 0, buffer.Length);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Ошибка при записи данных: " + ex.Message);
        }
    }
}

class User
{
    public int Index { get; set; }
    public string Login { get; set; }
    public ClientHandler ClientHandler { get; set; }
}
