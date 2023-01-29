namespace FWGPUE.IO;

abstract class EngineFile {
    public EngineFileLocation? Location { get; }
    public bool Loaded { get; protected set; } = false;

    public virtual void Save(bool overwrite = true) {
        if (Location == null) {
            Log.Error("attempting to save file without location");
            return;
        }

        if (!File.Exists(Location.FullPath)) {
            File.WriteAllBytes(Location.FullPath, SaveData());
            return;
        }

        if (File.Exists(Location.FullPath)) {
            if (overwrite) {
                File.Delete(Location.FullPath);
                File.WriteAllBytes(Location.FullPath, SaveData());
            }
            else {
                Log.Warn($"overwriting not enabled, file {Location.Name} not saved");
            }
        }
    }
    public virtual void Load() {
        if (Location == null) {
            Log.Error("attempting to load file without location");
            return;
        }

        if (File.Exists(Location.FullPath)) {
            try {
                ReadData(File.ReadAllBytes(Location.FullPath));
                Loaded = true;
            }
            catch (Exception e) {
                Log.Error($"exception thrown while attempting to load file {Location.Name}: {e.Message}");
            }
        }
    }

    /// <summary> Read the <paramref name="data"/> into properties etc. </summary>
    protected abstract void ReadData(byte[] data);
    /// <summary> Get data to save. </summary>
    protected abstract byte[] SaveData();

    public EngineFile(EngineFileLocation header) {
        if (header == null) {
            Log.Error("no header for file");
        }

        Location = header;
    }
}
