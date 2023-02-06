using Silk.NET.OpenGL;
using FontStashSharp.Interfaces;

namespace FWGPUE.Graphics;

using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;

class Texture2DManager : GLObject, ITexture2DManager
{
    public Texture2DManager(GL gl) : base(gl) { }

    public object CreateTexture(int width, int height) => new Texture(Gl, width, height);

    public Point GetTextureSize(object texture)
    {
        var t = (Texture)texture;
        return new Point(t.Width, t.Height);
    }

    public void SetTextureData(object texture, Rectangle bounds, byte[] data)
    {
        var t = (Texture)texture;
        t.SetData(bounds, data);
    }
}
