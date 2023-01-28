using FWGPUE.Graphics;
using System.Numerics;
using GL = Silk.NET.OpenGL.GL;

namespace FWGPUE.IO;

class SpriteAtlasFile : DataMarkupFile {
    public string? Name { get; protected set; }
    public ByteFile? Spritesheet { get; protected set; }

    public record SpriteRect(float X, float Y, float Width, float Height);
    Dictionary<string, SpriteRect> SpriteDefinitions { get; } = new();

    public Texture? Texture { get; protected set; }

    public SpriteRect GetRect(string name) {
        if (SpriteDefinitions.ContainsKey(name)) {
            return SpriteDefinitions[name];
        }
        return new(0, 0, Texture!.Width, Texture!.Height);
    }

    public override void Load() {
        // load and parse data using base class
        base.Load();

        // parse tokenized values further into required data
        if (TryGetToken("Name", out var name) && name is not null) {
            Name = name.Contents.Value;
        }
        else {
            Name = "unnamed";
            Log.Warn("unnamed sprite atlas");
        }

        if (TryGetToken("Spritesheet", out var location) && location is not null) {
            // neighbour of this file
            EngineFileLocation spritesheetLocation = new(Location!.Path, location.Contents.Value);
            Log.Info($"spritesheet file: {spritesheetLocation}");

            if (spritesheetLocation.Exists()) {
                Log.Info($"spritesheet file exists for {Name}");
            }
            else {
                Log.Error($"spritesheet file exists for {Name}");
            }

            Spritesheet = new ByteFile(spritesheetLocation);
            Spritesheet.Load();
        }
        else {
            Log.Error($"atlas {Name} has no spritesheet");
        }

        if (TryGetToken("Sprites", out var sprites) && sprites is not null) {
            // 1    2 3 4     5 ------.
            // name x y width height  |
            const int stride = 5;// <-'
            const int nameLocation = 0;
            const int xLocation = 1;
            const int yLocation = 2;
            const int widthLocation = 3;
            const int heightLocation = 4;

            try {
                string[] definitions = sprites.Contents.Collection;
                for (int i = 0; i < definitions.Length; i += stride) {
                    string spriteName = definitions[i + nameLocation];
                    string x = definitions[i + xLocation];
                    string y = definitions[i + yLocation];
                    string width = definitions[i + widthLocation];
                    string height = definitions[i + heightLocation];

                    // try to parse values into a spriterect
                    if (float.TryParse(x, out float fx) &&
                        float.TryParse(y, out float fy) &&
                        float.TryParse(width, out float fw) &&
                        float.TryParse(height, out float fh)) {

                        SpriteRect rect = new(fx, fy, fw, fh);
                        SpriteDefinitions[spriteName] = rect;
                    }
                    else {
                        Log.Error($"error parsing sprite definition {spriteName} in {Name} (bad format)");
                    }
                }
            }
            catch (IndexOutOfRangeException e) {
                Log.Error($"sprites array of {Name} is the wrong length: {e}");
            }
        }
        else {
            Log.Error($"spritesheet definition not found in {Name}");
        }
    }
    public void LoadTexture(GL gl) {
        if (Spritesheet is null) {
            Log.Error($"cannot load texture of {Name} spritesheet (missing spritesheet)");
            return;
        }

        try {
            Texture = new Texture(gl, Spritesheet);
        }
        catch (Exception e) {
            Log.Error($"error loading texture of {Name}: {e}");
        }
    }

    public SpriteAtlasFile(EngineFileLocation header) : base(header) { }
}
