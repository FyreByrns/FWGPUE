using FontStashSharp;
using FWGPUE.IO;
using static FWGPUE.IO.DataMarkupFile;

using FWGPUE.Graphics;
using RectpackSharp;

namespace FWGPUE.Scenes;

abstract class Scene {
    public float TotalTimeInScene { get; protected set; }

    public DataMarkupFile? Globals { get; protected set; }
    public DataMarkupFile? Assets { get; protected set; }

    public SpriteAtlasFile? Atlas { get; protected set; }

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
        string sceneDirectory = $"assets/scenes/{sceneNameLower}";
        string sceneGlobals = $"{sceneDirectory}/globals.fwgm";

        Globals = new DataMarkupFile(sceneGlobals);
        Globals.Location!.EnsureExists();
        Globals.Load();

        // load globals
        Assets = new DataMarkupFile($"{sceneDirectory}/asset-manifest.fwgm");
        Assets.Location!.EnsureExists();
        Assets.Load();

        // load scene assets from assets list
        List<(string name, EngineFileLocation location)> imagesToAddToAtlas = new();

        if (Assets.HasToken("Assets")) {
            string[] assetCollection = Assets.GetToken("Assets").Contents.Collection;
            for (int i = 0; i < assetCollection.Length; i += 2) {
                string name = assetCollection[i];
                string strLocation = assetCollection[i + 1];

                // substitute SCENE_ASSETS in path for the path to the scene's asset directory
                strLocation = strLocation.Replace("SCENE_ASSETS", $"{sceneDirectory}/assets");

                EngineFileLocation location = strLocation;
                if (location.Exists()) {
                    if (location.IsFile) {
                        // if the asset is an image, add it to the list of images to be put in a per-scene atlas
                        if(location.Type() == FileType.Image) {
                            imagesToAddToAtlas.Add((name, location));
                        }
                    }
                }
                else {
                    Log.Error($"listed asset {name} does not exist at {strLocation}");
                }
            }
        }

        Log.Info($"number of images: {imagesToAddToAtlas.Count}");

        // add all images in the asset list to an atlas
        Atlas = new();
        
        // create packing rectangles
        PackingRectangle[] packingRectangles = new PackingRectangle[imagesToAddToAtlas.Count];
        Texture[] loadedTextures = new Texture[imagesToAddToAtlas.Count];
        for (int i = 0; i < imagesToAddToAtlas.Count; i++) {
            EngineFileLocation imageLocation = imagesToAddToAtlas[i].location;
            loadedTextures[i] = new(new ByteFile(imageLocation));

            packingRectangles[i].Id = i;
            packingRectangles[i].Width = (uint)loadedTextures[i].Width;
            packingRectangles[i].Height = (uint)loadedTextures[i].Height;
        }
        
        // pack rectangles
        RectanglePacker.Pack(packingRectangles, out var totalSize);

        // create a texture to hold all the images
        Texture atlasTexture = new((int)totalSize.Width, (int)totalSize.Height);
        Atlas.Texture = atlasTexture;
        Log.Info($"atlas is {totalSize.Width}x{totalSize.Height}");

        // copy all the individual textures into the atlas texture
        for (int i = 0; i < packingRectangles.Length; i++) {
            PackingRectangle rect = packingRectangles[i];

            string name = imagesToAddToAtlas[rect.Id].name;
            Texture texture = loadedTextures[rect.Id];

            atlasTexture.SetData(rect, texture.Data);
            Atlas.SpriteDefinitions[name] = new SpriteAtlasFile.SpriteRect(rect.X, rect.Y, rect.Width, rect.Height);

            // don't eat all vram
            texture.Dispose();
        }
    }

    public abstract void Load();
    public abstract void Unload();

    public virtual void Tick() {
        TotalTimeInScene += TickTime;

        DrawText($"{GetType().Name} // {TotalTimeInScene:#.##}", new(0, Config.ScreenHeight * 0.98f), FSColor.Tan);
    }

    public virtual void Render() { }
}
