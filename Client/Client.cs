using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _1
{
    class Client
    {
        static void Main(string[] args)
        {
            TcpClient Client = new TcpClient();

            //Client.Connect("localhost", 8888);

            Connect(Client);

            if (Client.Connected)
            {
                Connected(Client);

                Chat(Client);
            }
        }

        private static void Connect(TcpClient Client)
        {
            string IP = AskIP();
            int Port = AskPort();
            Client.Connect(IP, Port);
        }

        private static string AskIP()
        {
            Console.Write("IP to connect to: ");

            string IP = Console.ReadLine();

            while (!IPAddress.TryParse(IP, out _))
            {
                Console.WriteLine("Another!");
                IP = Console.ReadLine();
            }

            return IP;
        }

        private static int AskPort()
        {
            Console.Write("Port to connect to: ");

            string Port = Console.ReadLine();

            while (!int.TryParse(Port, out _))
            {
                Console.WriteLine("Another!");
                Port = Console.ReadLine();
            }

            return Convert.ToInt32(Port);
        }

        private static void Connected(TcpClient Client)
        {
            IPEndPoint Server = (IPEndPoint)Client.Client.RemoteEndPoint;
            Console.WriteLine("Connected to Server: " + Server.Address + " on port number: " + Server.Port + Environment.NewLine);
        }

        private static void Chat(TcpClient Client)
        {
            if (Client.Client.Connected)
            {
                ThreadReceiveData(Client);

                Thread.Sleep(1000);

                while (Client.Connected)
                {
                    SendMessage(Client);
                }
            }
        }

        private static void SendMessage(TcpClient Client)
        {
            if (Client.Connected)
            {
                //Console.WriteLine("Client: ");
                Console.Write("Client: ");
                string message = Console.ReadLine();

                while (string.IsNullOrEmpty(message))
                {
                    //Console.WriteLine("Another! \n");
                    Console.WriteLine("Another!");

                    //Console.WriteLine("Client: ");
                    Console.Write("Client: ");
                    message = Console.ReadLine();
                }

                byte[] BufferMessage = Encoding.ASCII.GetBytes(message);

                Client.GetStream().Write(BufferMessage, 0, BufferMessage.Length);

                if (message.Equals("Quit")) { Disconnect(Client); }

                if (message.Equals("Send File")) { SendFile(Client); }
            }
        }

        private static string ReceiveMessage(TcpClient Client)
        {
            if (Client.Connected)
            {
                NetworkStream networkStream = Client.GetStream();

                byte[] Buffer = new byte[1024];

                int BytesCount = networkStream.Read(Buffer, 0, Buffer.Length);

                //Console.WriteLine(Encoding.ASCII.GetString(Buffer, 0, BytesCount));

                string DataMessage = Encoding.ASCII.GetString(Buffer, 0, BytesCount);

                return DataMessage;
            }
            else
            {
                return "";
            }
        }

        private static void ThreadReceiveData(TcpClient Client)
        {
            if (Client.Client.Connected)
            {
                var thread = new Thread(() => ReceiveData(Client));

                thread.Start();
            }
        }

        private static void ReceiveData(TcpClient Client)
        {
            while (Client.Connected)
            {
                string Message = ReceiveMessage(Client);

                if (Message.Equals("Sending Coverage"))
                {
                    ReceiveFile(Client);
                }
                else if (Message.Equals("Sending List Of Clients"))
                {
                    Console.WriteLine(Environment.NewLine + "Receiving List Of Clients...");
                }
                else
                {
                    Console.WriteLine("Server: " + Message);
                }
            }
        }

        private static void SendFile(TcpClient Client)
        {
            if (Client.Client.Connected)
            {
                string FilePath = @"C:\Users\PhyMo\Source\repos\P1\File Management\Send\";

                Console.Write("Filename: ");
                string filename = Console.ReadLine();

                while (!File.Exists(FilePath + filename + ".csv"))
                {
                    Console.WriteLine("Another!");
                    filename = Console.ReadLine();
                }

                string FullFilePath = FilePath + filename + ".csv";

                byte[] Data = File.ReadAllBytes(FullFilePath);

                Client.GetStream().Write(Data, 0, Data.Length);
            }
        }

        private static void ReceiveFile(TcpClient Client)
        {
            if (Client.Client.Connected)
            {
                string Destination = @"C:\Users\PhyMo\Source\repos\P1\File Management\Server Coverage\";

                string Data = ReceiveMessage(Client);

                using (FileStream fs = new FileStream(Destination + "ServerCoverage.csv", FileMode.Create, FileAccess.Write))
                {
                    fs.Write(Encoding.ASCII.GetBytes(Data), 0, Data.Length);
                }

                Console.WriteLine("Coverage received from Server");
            }
        }

        private static void Disconnect(TcpClient Client)
        {
            //Client.Client.Shutdown(SocketShutdown.Both);

            Client.Client.Shutdown(SocketShutdown.Send);

            Client.Close();

            Console.WriteLine("Client Disconnected");
        }
    }
}
