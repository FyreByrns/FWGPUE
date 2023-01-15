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

        public EngineFileLocation(string relativePath, char seperator = '/') {
            relativePath = relativePath.ToLower(); // all locations should be lowercase
            string[] fullRelativePath = relativePath.Split(seperator);
            string[] directoryRelativePath = fullRelativePath[..^1];
            Name = fullRelativePath[^1];

            Path = new string[directoryRelativePath.Length];
            Array.Copy(directoryRelativePath, Path, directoryRelativePath.Length);
        }

        public static implicit operator EngineFileLocation(string path) {
            return new EngineFileLocation(path);
        }
    }
}