using FWGPUE.IO;

namespace FWGPUE.Graphics;

class Sprite {
    static readonly Random RNG = new();
    public short ID { get; }

    public SpriteAtlasFile Atlas { get; }
    public Transform Transform { get; set; } = new();
    public string? Texture { get; set; }

    Sprite() {
        ID = unchecked((short)RNG.Next(0, short.MaxValue));
    }
    public Sprite(SpriteAtlasFile atlas) : this() {
        Atlas = atlas;
    }
}
