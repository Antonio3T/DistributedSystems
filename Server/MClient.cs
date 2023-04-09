using System.Net.Sockets;

namespace _1
{
    public class MClient
    {
        private static int count = 1;

        public int Id { get; private set; }

        public string? Name { get; set; }

        public TcpClient Client { get; private set; }

        public string Status { get; set; } = "Offline";

        public MClient(TcpClient client)
        {
            if (client == null) throw new ArgumentNullException("client");

            Client = client;
            Id = count++;
        }
    }
}