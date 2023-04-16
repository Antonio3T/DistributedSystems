using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _1
{
    public class HandleServer
    {
        //
        private static Mutex mutex = new Mutex();

        private static List<CMFile> Files = new List<CMFile>();

        public static void Welcome(MClient Client, NetworkStream Stream)
        {
            if (Client.Client.Connected)
            {
                //Client.Status = "Connected";

                Connected(Client);
                SendPredefinedMessage(Client, "100 OK" + Environment.NewLine);
                SendPredefinedMessage(Client, "Send File - to send file, Receive Coverage - to receive server coverage");
                SendPredefinedMessage(Client, ", Quit - to quit.");
                //SendPredefinedMessage(Client, Environment.NewLine + "Enter your IP");
                //ReceiveIP(Client, Stream);
            }
        }

        private static void Connected(MClient Client)
        {
            if (Client.Client.Connected)
            {
                Client.Status = "Connected";

                IPEndPoint ClientIP = (IPEndPoint)Client.Client.Client.LocalEndPoint;
                Console.WriteLine("Client connected: " + ClientIP.Address + " on port number: " + ClientIP.Port);

                Client.Name = ClientIP.Address.ToString();

                if (Client.Name.Equals("127.0.0.7")) { Client.Ownership = "Owner"; } else { Client.Ownership = Client.Name; }
                Console.WriteLine("Client Ownership: " + Client.Ownership);
            }
        }

        private static void SendPredefinedMessage(MClient Client, string Message)
        {
            if (Client.Client.Connected)
            {
                foreach (var C in Server.Clients.Where(n => n.Id.Equals(Client.Id)))
                {
                    byte[] BufferMessage = Encoding.ASCII.GetBytes(Message);

                    Client.Client.GetStream().Write(BufferMessage, 0, BufferMessage.Length);
                }
            }
        }

        private static void ReceiveIP(MClient Client, NetworkStream Stream)
        {
            if (Client.Client.Connected)
            {
                Client.Status = "Connected";

                string ClientsIP = ReceiveMessage(Client, Stream);

                while (!IPAddress.TryParse(ClientsIP, out _) || string.IsNullOrEmpty(ClientsIP))
                {
                    SendPredefinedMessage(Client, "IPv4 or IPv6 Adress");

                    ClientsIP = ReceiveMessage(Client, Stream);
                }

                Client.Name = ClientsIP;
            }
        }

        public static void ClientsMessage(MClient Client, NetworkStream Stream)
        {
            UseResourceClientsMessage(Client, Stream);
        }

        private static string ReceiveMessage(MClient Client, NetworkStream Stream)
        {
            if (Client.Client.Connected)
            {
                byte[] Buffer = new byte[1024];

                int BytesCount = Stream.Read(Buffer, 0, Buffer.Length);

                string DataMessage = Encoding.ASCII.GetString(Buffer, 0, BytesCount);

                if (DataMessage.Equals("Quit")) { Disconnect(Client); }

                if (DataMessage.Equals("Receive Coverage")) { SendPredefinedMessage(Client, "Sending Coverage"); SendFile(Client); }

                if (DataMessage.Equals("Clients")) { SendPredefinedMessage(Client, "Sending List Of Clients"); SendListOfClients(Client); }

                return DataMessage;
            }
            else
            {
                return "";
            }
        }

        private static void UseResourceClientsMessage(MClient Client, NetworkStream Stream)
        {
            int nFiles = 0;

            while (Client.Client.Connected)
            {
                string Message = ReceiveMessage(Client, Stream);

                Client.Status = "Sending Data";

                mutex.WaitOne();

                if (Message.Equals("Send File"))
                {
                    Client.Status = "Sending File";

                    nFiles++;

                    ReceiveFile(Client, Stream);
                }
                else
                {
                    //Console.WriteLine("Client " + Client.Id + ": " + Message);
                }

                mutex.ReleaseMutex();

                Client.Status = Client.Name + " Sent " + nFiles + " Files";
            }
        }

        private static void SendFile(MClient Client)
        {
            if (Client.Client.Connected)
            {
                string FilePath = @"C:\Users\PhyMo\Source\repos\P1\P1\File Management\Data\Data Base\Coverage\Coverage.csv";

                if (File.Exists(FilePath))
                {
                    string[] Data = File.ReadAllLines(FilePath);

                    string newData = string.Empty;

                    foreach (string line in Data)
                    {
                        string[] values = line.Split(',');

                        values = values.Take(values.Length - 1).ToArray();

                        newData += string.Join(",", values) + Environment.NewLine;
                    }

                    newData = newData.TrimEnd(Environment.NewLine.ToCharArray());

                    byte[] newFile = Encoding.ASCII.GetBytes(newData);

                    Client.Client.GetStream().Write(newFile, 0, newFile.Length);
                }
                else { Console.WriteLine("File Not Found"); }
            }
        }

        private static void ReceiveFile(MClient Client, NetworkStream Stream)
        {
            if (Client.Client.Connected)
            {
                string format = "Mddyyyyhhmmsstt";
                string DateAndTime = DateTime.Now.ToString(format);

                string Destination = @"C:\Users\PhyMo\Source\repos\P1\P1\File Management\Data\";

                string Data = ReceiveMessage(Client, Stream);

                //string Municipality = Data.Split(',')[9];

                if (Files.Any(f => f.Data.Equals(Data)))
                {
                    SendPredefinedMessage(Client, "File already sent");
                }
                else
                {
                    using (FileStream fs = new FileStream(Destination + Client.Ownership + "_" + Client.Id + "_" + DateAndTime + ".csv", FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(Encoding.ASCII.GetBytes(Data), 0, Data.Length);
                    }

                    Files.Add(new CMFile(Client.Name + "_" + Client.Id + "_" + DateAndTime, Data, Client));
                    // in alternative, one could read every file in the directory and compare the data. The method used is faster. Assumes that the server stays on.

                    Console.WriteLine("{0}, Client {1} submitted a file", Path.GetFileName(Client.Name), Client.Id);
                }
            }
        }

        private static void SendListOfClients(MClient Client)
        {
            if (Client.Client.Connected)
            {
                string ListOfClients = "";

                foreach (MClient client in Server.Clients)
                {
                    ListOfClients += client.Name + " ";
                }

                byte[] BufferListOfClients = Encoding.ASCII.GetBytes(ListOfClients);

                Client.Client.GetStream().Write(BufferListOfClients, 0, BufferListOfClients.Length);
            }
        }

        private static void Disconnect(MClient Client)
        {
            SendPredefinedMessage(Client, "400 BYE");

            Client.Client.Client.Shutdown(SocketShutdown.Both);

            Client.Status = "Disconnected";

            Server.Clients.Remove(Client);

            Client.Client.Close();

            Console.WriteLine("Client {0} Disconnected", Client.Id);
        }
        //
    }
}