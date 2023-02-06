using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

public abstract class GLObject
{
    public GL Gl { get; }
    protected GLObject(GL gl)
    {
        Gl = gl;
    }
}
