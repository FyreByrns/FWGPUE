namespace FWGPUE.Graphics;

class Sprite
{
    static Random RNG = new();
    public short ID { get; }

    public Transform Transform { get; set; } = new();
    public string? Texture { get; set; }

    public Sprite()
    {
        ID = unchecked((short)RNG.Next(0, short.MaxValue));
    }
}
