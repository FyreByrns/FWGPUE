using FontStashSharp;
using FWGPUE.IO;
using static FWGPUE.IO.DataMarkupFile;

using FWGPUE.Graphics;
using RectpackSharp;
using FWGPUE.Nodes;

namespace FWGPUE.Scenes;

abstract class Scene {
    public const int AtlasPackingPadding = 2;

    public string? Directory { get; private set; }

    public float TotalTimeInScene { get; protected set; }

    public DataMarkupFile? Globals { get; protected set; }
    public AssetManifestFile? Assets { get; protected set; }

    public SpriteAtlasFile? Atlas { get; protected set; }

    public NodeCollection Nodes { get; } = new();

    public T? GetGlobal<T>(string globalName) {
        if (Globals!.TryGetToken(globalName, out var token)) {
            var t = typeof(T); // for smaller ifs

            try {
                object? result = null;

                if (t == typeof(int)) {
                    if (int.TryParse(token?.Contents.Value, out int i)) {
                        result = i;
                    }
                }
                if (t == typeof(float)) {
                    if (float.TryParse(token?.Contents.Value, out float f)) {
                        result = f;
                    }
                }
                if (t == typeof(string)) {
                    result = token?.Contents?.Value;
                }

                return (T)result!;
            }
            catch (Exception e) {
                Log.Error($"error reading global {globalName}: {e}");
            }
        }

        Log.Error($"global {globalName} not found");
        return default(T);
    }

    protected void Load<T>()
        where T : Scene {

        // get scene directory from scene type name
        string sceneNameLower = typeof(T).Name.ToLower();
        Directory = $"assets/scenes/{sceneNameLower}";
        string sceneGlobals = $"{Directory}/globals.fwgm";

        Globals = new DataMarkupFile(sceneGlobals);
        Globals.Location!.EnsureExists();
        Globals.Load();

        // load globals
        Assets = new AssetManifestFile($"{Directory}/asset-manifest.fwgm");
        Assets.Location!.EnsureExists();
        Assets.Load(("SCENE_ASSETS", $"{Directory}/assets/"));

        // load scene assets from assets list
        List<(string name, EngineFileLocation location)> imagesToAddToAtlas = new();

        foreach (Asset imageAsset in Assets.ImageAssets) {
            imagesToAddToAtlas.Add((imageAsset.Name, imageAsset.Location));
        }

        Log.Info($"number of images: {imagesToAddToAtlas.Count}");

        // add all images in the asset list to an atlas
        Atlas = new();

        // create packing rectangles
        PackingRectangle[] packingRectangles = new PackingRectangle[Assets.ImageAssets.Count];
        Texture[] loadedTextures = new Texture[Assets.ImageAssets.Count];
        for (int i = 0; i < Assets.ImageAssets.Count; i++) {
            loadedTextures[i] = Assets.LoadAsset<Texture>(Assets.ImageAssets[i].Name);

            packingRectangles[i].Id = i;
            packingRectangles[i].Width = (uint)(loadedTextures[i].Width + AtlasPackingPadding);
            packingRectangles[i].Height = (uint)(loadedTextures[i].Height + AtlasPackingPadding);
        }

        // pack rectangles
        RectanglePacker.Pack(packingRectangles, out var totalSize);

        // create a texture to hold all the images
        Texture atlasTexture = new((int)totalSize.Width, (int)totalSize.Height);
        Atlas.Texture = atlasTexture;
        Log.Info($"atlas is {totalSize.Width}x{totalSize.Height}");

        // set entire atlas to blank, to clear uninitialized memory
        atlasTexture.SetData(new(0, 0, atlasTexture.Width, atlasTexture.Height), new byte[atlasTexture.Width * atlasTexture.Height * 4]);

        // copy all the individual textures into the atlas texture
        for (int i = 0; i < packingRectangles.Length; i++) {
            PackingRectangle rect = packingRectangles[i];

            string name = imagesToAddToAtlas[rect.Id].name;
            Texture texture = loadedTextures[rect.Id];

            atlasTexture.SetData(new((int)rect.X, (int)rect.Y, (int)rect.Width - AtlasPackingPadding, (int)rect.Height - AtlasPackingPadding), texture.Data);
            Atlas.SpriteDefinitions[name] = new SpriteAtlasFile.SpriteRect(rect.X, rect.Y, rect.Width - AtlasPackingPadding, rect.Height - AtlasPackingPadding);

            // don't eat all vram
            texture.Dispose();
        }
    }

    public abstract void Load();
    public abstract void Unload();

    public virtual void Tick() {
        TotalTimeInScene += TickTime;
        Nodes.TickNodes();
    }

    public virtual void Render() {
        Nodes.DrawNodes();
        DrawText($"{GetType().Name} // {TotalTimeInScene:#.##}", Camera.ScreenToWorld(new(0, Window.Size.Y * 0.98f)), FSColor.Tan);
    }
}
