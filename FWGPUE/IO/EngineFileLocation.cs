namespace FWGPUE.IO {
    class EngineFileLocation {
        public enum FileType {
            None = 0,

            Text,
            Image,
        }
        public static readonly Dictionary<FileType, string[]> FileExtensionsByType = new() {
            { FileType.Text,    new[]{ "txt" } },
            { FileType.Image,   new[]{ "png" } },
        };

        /// <summary> Name of the file. </summary>
        public string? Name { get; set; }
        /// <summary> Path to the directory containing the file. </summary>
        public virtual string[] Path { get; set; }
        public string FullPath =>
            AppDomain.CurrentDomain.BaseDirectory +
            (Path.Length > 0 ? string.Join(System.IO.Path.DirectorySeparatorChar, Path) : "") +
            (Path.Length > 0 ? System.IO.Path.DirectorySeparatorChar : "") + Name;

        public bool IsDirectory => File.GetAttributes(FullPath).HasFlag(FileAttributes.Directory);
        public bool IsFile => !IsDirectory;

        public FileType Type() {
            // split the file name by a period to find the extension
            string[] nameSplit =
                Name?.Split(".", StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string>();

            // if the name couldn't be split by a dot, it doesn't have an extension
            if (nameSplit.Length >= 2) {
                // otherwise, the extension is the last element of the split name
                string extension = nameSplit.Last();

                // iterate over the filetype registry to find the matching type for the extension
                foreach (FileType type in FileExtensionsByType.Keys) {
                    if (FileExtensionsByType[type].Contains(extension)) {
                        return type;
                    }
                }
            }

            return FileType.None;
        }

        /// <summary> Ensure the file pointed to by this <see cref="EngineFileLocation"/> exists. </summary>
        public void EnsureExists() {
            if(!Directory.Exists(FullPath)) {
                    try {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FullPath)!);
                    }
                    catch (Exception e) {
                        Log.Error($"error creating directory: {e}");
                    }
            }

            if (!File.Exists(FullPath)) {
                File.Create(FullPath).Close();
            }
        }
        /// <summary> Whether the file pointed to by this <see cref="EngineFileLocation"/> exists. </summary>
        public bool Exists() {
            return File.Exists(FullPath);
        }

        public EngineFileLocation(string[] directoryPath, string filename) {
            // convert all strings to lowercase
            filename = filename.ToLower();
            for (int i = 0; i < directoryPath.Length; i++) {
                directoryPath[i] = directoryPath[i].ToLower();
            }

            Path = directoryPath;
            Name = filename;
        }
        public EngineFileLocation(string relativePath, char seperator = '/')
            : this(relativePath.Split(seperator)[..^1], // directory path is all strings up to the last
                   relativePath.Split(seperator)[^1] // file name is the last string
                  ) { }

        public static implicit operator EngineFileLocation(string path) {
            return new EngineFileLocation(path);
        }

        public override string ToString() {
            return FullPath;
        }
    }
}