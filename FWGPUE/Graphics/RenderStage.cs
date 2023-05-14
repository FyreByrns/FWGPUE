using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

abstract class RenderStage {
    public Framebuffer Target;

    static Shader fullscreenQuadShader;
    static uint screenQuadVAO;
    static uint screenQuadVBO;

    public abstract void Render(RenderStage? previous);

    public void DrawToBackbuffer() {
        fullscreenQuadShader.Use();
        Gl.BindVertexArray(screenQuadVAO);
        Gl.BindTexture(TextureTarget.Texture2D, Target.Colour);
        Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        Gl.BindVertexArray(0);
    }

    protected RenderStage() {
        Target = new(Config.ScreenWidth, Config.ScreenHeight);
    }

    static RenderStage() {
        fullscreenQuadShader = new(new("assets/fullscreenquad.shader"));

        screenQuadVAO = Gl.GenVertexArray();
        screenQuadVBO = Gl.GenBuffer();

        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, screenQuadVBO);
        Gl.BindVertexArray(screenQuadVAO);
        float[] vs = new float[] {
            -1f, -1f, 0f, 0f, /**/ 1f, -1f, 1f, 0f, /**/ -1f, 1f, 0f, 1f,
            1f, -1f, 1f, 0f, /**/ 1f, 1f, 1f, 1f, /**/ -1f, 1f, 0f, 1f,
        };
        unsafe {
            fixed (float* d = vs) {
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(sizeof(float) * vs.Length), d, BufferUsageARB.StaticDraw);
            }

            Gl.EnableVertexAttribArray(0);
            Gl.EnableVertexAttribArray(1);

            Gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, (void*)0);
            Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, (void*)(sizeof(float) * 2));
        }

        Gl.BindVertexArray(0);
    }
}
