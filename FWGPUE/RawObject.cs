using Silk.NET.OpenGL;
using FWGPUE.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace FWGPUE;

class RawObject : IDisposable {
    public Transform Transform { get; set; } = new();

    public BufferObject<float> VBO { get; protected set; }
    public BufferObject<uint> EBO { get; protected set; }
    public VertexArrayObject<float, uint> VAO { get; protected set; }
    public Texture Texture { get; set; }
    public Shader Shader { get; set; }

    readonly float[] Vertices = {
        //x     y      z     u   v
         0.5f,  0.5f, 0.0f, 1f, 0f,
         0.5f, -0.5f, 0.0f, 1f, 1f,
        -0.5f, -0.5f, 0.0f, 0f, 1f,
        -0.5f,  0.5f, 0.0f, 0f, 0f
    };
    readonly uint[] Indices = {
        0, 1, 3,
        1, 2, 3
    };

    public void Draw(GL Gl, Engine context) {
        VAO.Bind();
        Shader.Use();
        Texture.Bind();
        Shader.SetUniform("uTexture0", 0);
        //Shader.SetUniform("uScreenSize", new Vector2(context.Config.ScreenWidth, context.Config.ScreenHeight));

        var model =
            Matrix4x4.CreateRotationZ(Engine.TurnsToRadians(Transform.Rotation.Z)) *
            Matrix4x4.CreateRotationY(Engine.TurnsToRadians(Transform.Rotation.Y)) *
            Matrix4x4.CreateRotationX(Engine.TurnsToRadians(Transform.Rotation.X)) *
            Matrix4x4.CreateScale(Transform.Scale) *
            Matrix4x4.CreateTranslation(Transform.Position);
        var view = Matrix4x4.CreateLookAt(context.CameraPos, context.CameraTarget, context.CameraUp);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(Engine.DegreesToRadians(45.0f), (float)context.Config.ScreenWidth / context.Config.ScreenHeight, 0.1f, 100.0f);
        //var projection = Matrix4x4.CreateOrthographic(context.Config.ScreenWidth, context.Config.ScreenHeight, 0.1f, 100f);

        Shader.SetUniform("uModel", model);
        Shader.SetUniform("uView", view);
        Shader.SetUniform("uProjection", projection);

        unsafe {
            Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
        }
    }

    public RawObject(GL Gl) {
        EBO = new(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
        VBO = new(Gl, Vertices, BufferTargetARB.ArrayBuffer);
        VAO = new(Gl, VBO, EBO);

        VAO.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
        VAO.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

        Shader = new(Gl, new ShaderFile("assets/default.shader"));
        Texture = new(Gl, new ByteFile("assets/default.png"));
    }

    public void Dispose() {
        VBO.Dispose();
        EBO.Dispose();
        VAO.Dispose();
        Shader.Dispose();
        Texture.Dispose();
    }
}
