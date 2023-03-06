using FWGPUE.Graphics;

namespace FWGPUE.IO;

class Asset {
    public FileType Type { get; }
    public string Name { get; }
    public EngineFileLocation Location { get; }

    public int Width { get; set; }
    public int Height { get; set; }

    public Asset(FileType type, string name, EngineFileLocation location) {
        Type = type;
        Name = name;
        Location = location;
    }
}

class AssetManifestFile : DataMarkupFile {
    const int AssetInfoStride = 2;
    const int AssetInfoNameLocation = 0;
    const int AssetInfoLocationLocation = 1;

    /// <summary>
    /// All asset locations declared in the manifest.
    /// </summary>
    public List<Asset> Assets { get; } = new();
    /// <summary>
    /// The subset of <see cref="Assets"/> which are images.
    /// </summary>
    public List<Asset> ImageAssets { get; } = new();

    public bool GetAsset(string name, out Asset asset) {
        foreach(Asset a in Assets) {
            if (a.Name == name) {
                asset = a;
                return true;
            }
        }

        Log.Error($"no such asset {name}");
        asset = null;
        return false;
    }

    public T LoadAsset<T>(string name) {
        return (T)LoadAsset(name);
    }
    public object LoadAsset(string name) {
        if(GetAsset(name, out Asset result)) {
            if (result.Type == FileType.Image) {
                Texture resultTexture = new Texture(new ByteFile(result.Location));
                result.Width = resultTexture.Width;
                result.Height = resultTexture.Height;
                return resultTexture;
            }
        }

        Log.Error($"could not load {name}");
        return null;
    }

    /// <summary>
    /// Load all assets defined in the manifest, replacing items in the path with other items.
    /// </summary>
    /// <param name="pathReplacements"></param>
    public void Load(params (string original, string replacement)[] pathReplacements) {
        base.Load();

        // if there are any assets declared in this asset manifest file
        if (HasToken("Assets")) {
            string[] assetInfo = GetToken("Assets").Contents.Collection;

            for (int i = 0; i < assetInfo.Length; i += AssetInfoStride) {
                string assetName = assetInfo[i + AssetInfoNameLocation];
                string assetLocationStr = assetInfo[i + AssetInfoLocationLocation];

                // process path replacements
                foreach ((string original, string replacement) in pathReplacements) {
                    Log.Info($"{original}->{replacement}");
                    Log.Info($"\t{assetLocationStr} ->");
                    assetLocationStr = assetLocationStr.Replace(original, replacement);
                    Log.Info($"\t{assetLocationStr}");
                }

                EngineFileLocation assetLocation = assetLocationStr;

                Asset asset = new(assetLocation.Type(), assetName, assetLocation);
                Assets.Add(asset);
                if (asset.Type == FileType.Image) {
                    ImageAssets.Add(asset);
                }
            }
        }
        else {
            Log.Warn($"no assets declared in asset file {Location?.Name ?? "unlocated"}");
        }
    }

    public AssetManifestFile(EngineFileLocation header) : base(header) { }
}
