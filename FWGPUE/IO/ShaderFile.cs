using System.Text;

namespace FWGPUE.IO;

class ShaderFile : EngineFile {
    public const string Split = "^ vertex ^ / v fragment v";

    public string Vertex { get; protected set; }
    public string Fragment { get; protected set; }

    protected override void ReadData(byte[] data) {
        string contents = Encoding.ASCII.GetString(data);

        if (!contents.Contains(Split)) {
            Log.Error($".shader invalid: \'{Split}\' not found in shader file");
        }

        string[] both = contents.Split(Split);
        Vertex = both[0];
        Fragment = both[1];
    }

    protected override byte[] SaveData() {
        List<byte> data = new List<byte>();

        data.AddRange(Encoding.ASCII.GetBytes(Vertex));
        data.AddRange(Encoding.ASCII.GetBytes(Split));
        data.AddRange(Encoding.ASCII.GetBytes(Fragment));

        return data.ToArray();
    }

    public ShaderFile(EngineFileLocation location) : base(location) { }
}
