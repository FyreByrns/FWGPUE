using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

class ClearColourStage : RenderStage {
    public override void Render(RenderStage? previous) {
        Target.Bind();
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        Gl.Enable(GLEnum.DepthTest);
        Gl.ClearColor(0, 0, 0, 0);
    }

    public ClearColourStage() : base() { }
}
