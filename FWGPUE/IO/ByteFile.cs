namespace FWGPUE.IO;

/// <summary> A file which should be loaded as just an array of bytes </summary>
class ByteFile : EngineFile {
    public byte[]? Data { get; protected set; }

    protected override void ReadData(byte[] data) {
        Data = data;
    }

    protected override byte[] SaveData() {
        return Data!;
    }

    public ByteFile(EngineFileLocation header) : base(header) { }
}
