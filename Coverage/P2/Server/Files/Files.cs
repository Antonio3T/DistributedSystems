namespace _1
{
    class CheckFiles
    {
        public static List<MFile> Files = new List<MFile>();

        static void Main(string[] args)
        {
            StartWatcher();
        }

        public static void StartWatcher()
        {
            string rPath = @"C:\Users\PhyMo\source\repos\P1\Data";

            LoadFiles(rPath);
            ThreadHandleFiles(rPath);
            ThreadShowFiles();
        }

        private static void LoadFiles(string rPath)
        {
            var newFiles = Directory.GetFiles(rPath, "*.csv");

            foreach (var File in newFiles)
            {
                NewFile(File);
            }
        }

        private static void NewFile(string File)
        {
            MFile mFile = new MFile(File);

            mFile.Name = Path.GetFileName(File);
            mFile.Size = new FileInfo(File).Length.ToString();
            mFile.DateCreated = DateTime.Now;
            mFile.DateModified = DateTime.Now;

            Files.Add(mFile);
        }

        private static void ThreadShowFiles()
        {
            var thread = new Thread(() => ShowFiles());

            thread.Start();
        }

        private static void ThreadHandleFiles(string rPath)
        {
            var thread = new Thread(() => HandleFiles(rPath));

            thread.Start();
        }

        private static void ShowFiles()
        {
            while (true)
            {
                string? Input = Console.ReadLine();

                if (!string.IsNullOrEmpty(Input))
                {
                    if (Input.Equals("Files"))
                    {
                        Files.ToList().ForEach(f =>
                        {
                            Console.WriteLine(f.Id + " " + f.Name);
                        });
                    }
                    if (Input.Equals("Quit"))
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }

        private static void HandleFiles(string rPath)
        {
            bool rPathExists = Directory.Exists(rPath);

            var Watcher = new FileSystemWatcher(rPath)
            {
                Filter = "*.csv",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            if (rPathExists)
            {
                Changes(Watcher);

                while (rPathExists) ;
            }
            else { Console.WriteLine("Check Path"); }
        }

        private static void Changes(FileSystemWatcher Watcher)
        {
            Watcher.Created += (s, e) =>
            {
                Console.WriteLine("Created: " + e.Name);
                NewFile(e.FullPath);
            };
            Watcher.Deleted += (s, e) =>
            {
                Console.WriteLine("Deleted: " + e.Name);

                Files.RemoveAll(n => n.Name.Equals(e.Name));
            };
            Watcher.Renamed += (s, e) =>
            {
                Console.WriteLine("Renamed: " + e.OldName + " to " + e.Name);

                foreach (var c in Files.Where(n => n.Name.Equals(e.OldName)))
                {
                    c.Name = e.Name;
                    c.DateModified = DateTime.Now;
                }

                bool FileExists = Files.Any(n => n.Name.Equals(e.Name));

                if (!FileExists) { NewFile(e.FullPath); }
            };
            Watcher.Changed += (s, e) =>
            {
                Console.WriteLine("Changed: " + e.Name);

                foreach (var c in Files.Where(n => n.Name.Equals(e.Name)))
                {
                    c.DateModified = DateTime.Now;
                    c.DateAccessed = DateTime.Now;
                }
            };
        }
    }
}
