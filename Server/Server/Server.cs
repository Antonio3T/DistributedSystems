using System.Net;
using System.Net.Sockets;




namespace _1
{
    class Server
    {
        public static List<MClient> Clients = new List<MClient>();

        static void Main(string[] args)
        {
            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 8888);

            ServerSocket.Start();

            Console.WriteLine("Waiting for a client...");

            //
            CheckFiles.StartWatcher();
            //

            while (true)
            {
                TcpClient Client = ServerSocket.AcceptTcpClient();

                //
                MClient mClient = new MClient(Client);

                var ChildSocketThread = new Thread(() => handle_client(mClient));

                ChildSocketThread.Start();

                Clients.Add(mClient);
                //

                Console.WriteLine("Client Connected {0}", mClient.Id);
            }
        }

        private static void handle_client(MClient Client)
        {
            if (Client.Client.Connected)
            {
                NetworkStream Stream = Client.Client.GetStream();


                HandleServer.Welcome(Client, Stream);
                HandleServer.ClientsMessage(Client, Stream);
            }
        }
    }
}
