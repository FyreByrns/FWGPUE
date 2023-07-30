using Silk.NET.OpenGL;
using System.Numerics;

namespace FWGPUE.Graphics;

class ClearColourStage : RenderStage {
    public Vector4 ClearColour;

    public override void Render(RenderStage? previous) {
        Target.Bind();
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        Gl.Enable(GLEnum.DepthTest);
        Gl.ClearColor(ClearColour.X, ClearColour.Y, ClearColour.Z, ClearColour.W);
    }

    public ClearColourStage(Vector4 clearColour) : base() { 
        ClearColour = clearColour;
    }
}
