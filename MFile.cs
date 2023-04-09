namespace _1
{
    public class MFile
    {
        private static int count = 1;
        public int Id { get; private set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public DateTime? DateAccessed { get; set; }

        public MFile(string name)
        {
            Id = count++;
            Name = name;
        }
    }
}