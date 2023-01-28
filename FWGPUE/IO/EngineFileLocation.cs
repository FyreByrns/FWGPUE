namespace FWGPUE.IO {
    class EngineFileLocation {
        /// <summary> Name of the file. </summary>
        public string? Name { get; set; }
        /// <summary> Path to the directory containing the file. </summary>
        public virtual string[] Path { get; set; }
        public string FullPath =>
            AppDomain.CurrentDomain.BaseDirectory +
            (Path.Length > 0 ? string.Join(System.IO.Path.DirectorySeparatorChar, Path) : "") +
            (Path.Length > 0 ? System.IO.Path.DirectorySeparatorChar : "") + Name;

        /// <summary> Ensure the file pointed to by this <see cref="EngineFileLocation"/> exists. </summary>
        public void EnsureExists() {
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