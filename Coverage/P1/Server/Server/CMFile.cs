namespace _1
{
    public class CMFile
    {
        public string? Name { get; set; }
        public string? Data { get; set; }
        public MClient? Client { get; private set; }

        public CMFile(string Name, string Data, MClient Client)
        {
            this.Name = Name;
            this.Data = Data;
            this.Client = Client;
        }
    }
}
