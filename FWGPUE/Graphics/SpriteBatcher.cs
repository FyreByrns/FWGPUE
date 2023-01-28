using Silk.NET.OpenGL;
using FWGPUE.IO;
using System.Numerics;
using Shader = FWGPUE.Graphics.Shader;

namespace FWGPUE.Graphics;

class SpriteBatcher {
    float[] quadVertices = {
        // positions  
        -0.5f,  0.5f, 1.0f,
        -0.5f, -0.5f, 1.0f,
         0.5f, -0.5f, 1.0f,

        -0.5f,  0.5f, 1.0f,
         0.5f, -0.5f, 1.0f,
         0.5f,  0.5f, 1.0f,
    };
    uint quadVAO;
    uint quadVBO;
    uint offsetVBO;

    public List<Sprite> SpritesThisFrame { get; } = new();
    public HashSet<short> RegisteredSpriteIDs { get; } = new(); // to track which sprites are already being drawn

    public Shader Shader { get; set; }

    /// <summary>
    /// Register sprite for drawing this frame.
    /// </summary>
    public void DrawSprite(Sprite sprite) {
        if (!RegisteredSpriteIDs.Contains(sprite.ID)) {
            SpritesThisFrame.Add(sprite);
            RegisteredSpriteIDs.Add(sprite.ID);
        }
    }

    public void DrawAll(GL gl, Engine context) {
        // get offsets
        Matrix4x4[] offsets = new Matrix4x4[SpritesThisFrame.Count];
        for (int i = 0; i < offsets.Length; i++) {
            Sprite sprite = SpritesThisFrame[i];
            var model =
                Matrix4x4.CreateRotationZ(Engine.TurnsToRadians(sprite.Transform.Rotation.Z)) *
                Matrix4x4.CreateRotationY(Engine.TurnsToRadians(sprite.Transform.Rotation.Y)) *
                Matrix4x4.CreateRotationX(Engine.TurnsToRadians(sprite.Transform.Rotation.X)) *
                Matrix4x4.CreateScale(sprite.Transform.Scale.X, sprite.Transform.Scale.Y, 1) *
                Matrix4x4.CreateTranslation(sprite.Transform.Position);

            offsets[i] = model;
        }
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, offsetVBO);
        unsafe {
            fixed (Matrix4x4* data = offsets) {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(Matrix4x4) * offsets.Length), data, BufferUsageARB.StaticDraw);
            }
        }

        var view = context.Camera.ViewMatrix;
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(Engine.DegreesToRadians(45.0f), (float)context.Config.ScreenWidth / context.Config.ScreenHeight, 0.1f, 200.0f);

        Shader.Use();
        Shader.SetUniform("uView", view);
        Shader.SetUniform("uProjection", projection);

        gl.BindVertexArray(quadVAO);
        gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, (uint)SpritesThisFrame.Count);

        SpritesThisFrame.Clear();
        RegisteredSpriteIDs.Clear();
    }

    public void Clear() {
        SpritesThisFrame.Clear();
    }

    public SpriteBatcher(GL gl) {
        Shader = new Shader(gl, new ShaderFile("assets/sprite.shader"));

        quadVAO = gl.GenVertexArray();
        quadVBO = gl.GenBuffer();
        offsetVBO = gl.GenBuffer();

        gl.BindBuffer(GLEnum.ArrayBuffer, quadVBO);
        unsafe {
            fixed (void* data = quadVertices) {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(quadVertices.Length * sizeof(float)), data, BufferUsageARB.StaticDraw);
            }
        }

        gl.BindVertexArray(quadVAO);
        unsafe {
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, (void*)0);
        }

        // setup offset buffer
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, offsetVBO);
        gl.EnableVertexAttribArray(1);
        gl.EnableVertexAttribArray(2);
        gl.EnableVertexAttribArray(3);
        gl.EnableVertexAttribArray(4);
        unsafe {
            gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 0));
            gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 1));
            gl.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 2));
            gl.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 3));
        }
        gl.VertexAttribDivisor(1, 1);
        gl.VertexAttribDivisor(2, 1);
        gl.VertexAttribDivisor(3, 1);
        gl.VertexAttribDivisor(4, 1);
    }
}
