using System.Text;

namespace _1
{
    class CheckFiles
    {
        private static Mutex mutex = new Mutex();

        private static List<MFile> Files = new List<MFile>();

        private static List<Address> Addresses = new List<Address>();

        public static void StartWatcher()
        {
            string rPath = @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data";

            string cPath = @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Cities";

            LoadFiles(rPath);
            ThreadHandleFiles(rPath);
            ThreadHandleCities(cPath);
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

        private static void ThreadHandleCities(string cPath)
        {
            var thread = new Thread(() => HandleCities(cPath));

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

        private static void HandleCities(string cPath)
        {
            bool cPathExists = Directory.Exists(cPath);

            var Overlaid = new FileSystemWatcher(cPath)
            {
                Filter = "*.csv",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            if (cPathExists)
            {
                ChangesCities(Overlaid);

                while (cPathExists) ;
            }
            else { Console.WriteLine("Check Path"); }
        }

        private static void Changes(FileSystemWatcher Watcher)
        {
            Watcher.Created += (s, e) =>
            {
                Console.WriteLine("Created: " + e.Name);
                NewFile(e.FullPath);

                WriteLogs("Open", @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Logs\Logs.csv", e.Name);
                StoreFile(e.FullPath);
            };
            Watcher.Deleted += (s, e) =>
            {
                Console.WriteLine("Deleted: " + e.Name);

                Files.RemoveAll(n => n.Name.Equals(e.Name));

                WriteLogs("Completed", @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Logs\Logs.csv", e.Name);
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

                WriteLogs("Renamed " + e.Name, @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Logs\Logs.csv", e.OldName);
            };
            Watcher.Changed += (s, e) =>
            {
                Console.WriteLine("Changed: " + e.Name);

                foreach (var c in Files.Where(n => n.Name.Equals(e.Name)))
                {
                    c.DateModified = DateTime.Now;
                    c.DateAccessed = DateTime.Now;
                }

                //WriteLogs("Changed", @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Logs\Logs.csv", e.Name);
            };
        }

        private static void ChangesCities(FileSystemWatcher Watcher)
        {
            Watcher.Changed += (s, e) =>
            {
                mutex.WaitOne();

                Thread.Sleep(1000);

                CheckCities(e.FullPath);

                mutex.ReleaseMutex();
            };
        }

        private static void StoreFile(string path)
        {
            string Id = path.Split('\\').Last().Split('_').First();

            string destinationFolder = @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\" + Id + @"\";

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            WriteLogs("In Progress", @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Logs\Logs.csv", Path.GetFileName(path));
            UpdateFile(path, Id);

            File.Move(path, destinationFolder + Path.GetFileName(path));
        }

        private static void CheckCities(string path)
        {
            string[] lines = File.ReadAllLines(path);
            lines = lines.Skip(1).ToArray();

            List<string> matches = new List<string>();
            string previous = string.Empty;
            int match = 0;

            foreach (string line in lines)
            {
                string CLine = line.TrimEnd(',').Remove(line.LastIndexOf(',') + 1);

                if (CLine.Equals(previous))
                {
                    match++;

                    if (!matches.Any(l => l.Equals(line)))
                    {
                        matches.Add(line);
                    }
                }

                previous = CLine;
            }

            int streets = lines.Length;
            string filename = path.Split('\\').Last().Split('.').First();

            Console.WriteLine();
            Console.WriteLine(filename + ": " + streets + " addresses");

            if (match > 0)
            {
                matches.ToArray();

                Console.WriteLine("{0} repeated address(es): ", match);

                foreach (string line in matches)
                {
                    Console.WriteLine(line);
                }
            }
        }

        private static void UpdateFile(string Path, string Id)
        {
            string CoveragePath = @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Coverage\Coverage.csv";

            string[] newlines = File.ReadAllLines(Path);
            string Header = newlines.First() + ",Ownership";
            newlines = newlines.Skip(1).ToArray();

            if (File.Exists(CoveragePath))
            {
                List<string> matches = new List<string>();

                string[] lines = File.ReadAllLines(CoveragePath);
                lines = lines.Skip(1).ToArray();

                foreach (string line in lines)
                {
                    string[] values = line.Split(',');

                    string Ownership = values[5];

                    if (Ownership.Equals(Id))
                    {
                        matches.Add(line);
                    }
                }

                List<string> CLines = lines.ToList();

                matches.ForEach(m => CLines.Remove(m));

                string Message = string.Join(Environment.NewLine, CLines) + Environment.NewLine + string.Join("," + Id + Environment.NewLine, newlines) + "," + Id;

                WriteInFile(Message, CoveragePath, Header);
            }
            else
            {
                string Message = string.Join("," + Id + Environment.NewLine, newlines) + "," + Id;

                WriteInFile(Message, CoveragePath, Header);
            }

            ProcessFile(CoveragePath);
        }

        private static void ProcessFile(string path)
        {
            string[] lines = File.ReadAllLines(path);
            string Header = lines.First() + Environment.NewLine;
            lines = lines.Skip(1).ToArray();

            Addresses.Clear();

            string[] files = Directory.GetFiles(@"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Cities\");
            foreach (string file in files)
            {
                File.Delete(file);
            }

            foreach (string line in lines)
            {
                string[] values = line.Split(',');

                string Municipality = values[4];

                string destinationMunicipality = @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Cities\" + Municipality + ".csv";

                //WriteInFile(line + "," + Id, destinationMunicipality);

                using (FileStream fs = new FileStream(destinationMunicipality, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(Encoding.ASCII.GetBytes(Header), 0, Header.Length);
                };
            }

            foreach (string line in lines)
            {
                string[] values = line.Split(',');

                string Street = values[0];
                string Zip = values[1];
                string DoorNumber = values[2];
                string City = values[3];
                string Municipality = values[4];
                string Ownership = values[5];

                Addresses.Add(new Address(Street, Zip, DoorNumber, City, Municipality, Ownership));

                string destinationMunicipality = @"C:\Users\PhyMo\source\repos\P1\P1\File Management\Data\Data Base\Cities\" + Municipality + ".csv";

                using (StreamWriter sw = File.AppendText(destinationMunicipality))
                {
                    sw.WriteLine(line);
                }
            }
        }

        private static void WriteLogs(string Message, string Path, string Filename)
        {
            if (!File.Exists(Path))
            {
                using (StreamWriter sw = File.AppendText(Path))
                {
                    sw.WriteLine("Filename,Status,Date");
                    sw.WriteLine(Filename + "," + Message + "," + DateTime.Now.ToString());
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(Path))
                {
                    sw.WriteLine(Filename + "," + Message + "," + DateTime.Now.ToString());
                }
            }
        }

        private static void WriteInFile(string Message, string Path, string Header)
        {
            using (StreamWriter sw = File.CreateText(Path))
            {
                sw.Write(Header);
                sw.WriteLine(Message);
            }
        }
    }
}