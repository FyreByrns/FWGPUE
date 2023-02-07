using Silk.NET.OpenGL;
using FWGPUE.IO;
using System.Numerics;

namespace FWGPUE.Graphics;

class SpriteBatcher {
    // -0.5, -0.5 v       v 0.5, -0.5
    //            A------>D
    //            ^     / |
    //            |   o   | <--- o is origin
    //            | /     v
    //            B<------C
    // -0.5,  0.5 ^       ^ 0.5,  0.5
    float[] quadVertices = {
     // positions           uvs
        -0.5f,  0.5f, 1.0f, 0f, 1f, // B
        -0.5f, -0.5f, 1.0f, 0f, 0f, // A
         0.5f, -0.5f, 1.0f, 1f, 0f, // D
                                    
        -0.5f,  0.5f, 1.0f, 0f, 1f, // B
         0.5f, -0.5f, 1.0f, 1f, 0f, // D
         0.5f,  0.5f, 1.0f, 1f, 1f, // C
    };
    uint quadVAO;
    uint quadVBO;
    uint offsetVBO;
    uint uvVBO;

    public Dictionary<SpriteAtlasFile, List<Sprite>> SpritesByAtlas { get; } = new();
    public HashSet<short> RegisteredSpriteIDs { get; } = new(); // to track which sprites are already being drawn

    public Shader Shader { get; set; }

    /// <summary>
    /// Register sprite for drawing this frame.
    /// </summary>
    public void DrawSprite(Sprite sprite) {
        if (!SpritesByAtlas.ContainsKey(sprite.Atlas!)) {
            SpritesByAtlas[sprite.Atlas!] = new List<Sprite>();
        }

        if (!RegisteredSpriteIDs.Contains(sprite.ID)) {
            SpritesByAtlas[sprite.Atlas!].Add(sprite);
            RegisteredSpriteIDs.Add(sprite.ID);
        }
    }

    public void DrawAll() {
        foreach (SpriteAtlasFile atlas in SpritesByAtlas.Keys) {
            List<Sprite> SpritesThisFrame = SpritesByAtlas[atlas];
            SpritesThisFrame = SpritesThisFrame.OrderBy(x => x.Transform.Position.Z).ToList();

            // get offsets
            Matrix4x4[] offsets = new Matrix4x4[SpritesThisFrame.Count];
            for (int i = 0; i < offsets.Length; i++) {
                Sprite sprite = SpritesThisFrame[i];
                var model =
                    Matrix4x4.CreateScale(sprite.Transform.Scale.X * atlas.GetRect(sprite.Texture!).Width, sprite.Transform.Scale.Y * atlas.GetRect(sprite.Texture!).Height, 1) *
                    Matrix4x4.CreateRotationZ(TurnsToRadians(sprite.Transform.Rotation.Z)) *
                    Matrix4x4.CreateRotationY(TurnsToRadians(sprite.Transform.Rotation.Y)) *
                    Matrix4x4.CreateRotationX(TurnsToRadians(sprite.Transform.Rotation.X)) *
                    Matrix4x4.CreateTranslation(sprite.Transform.Position);

                offsets[i] = model;
            }
            Gl!.BindBuffer(BufferTargetARB.ArrayBuffer, offsetVBO);
            unsafe {
                fixed (Matrix4x4* data = offsets) {
                    Gl!.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(Matrix4x4) * offsets.Length), data, BufferUsageARB.StaticDraw);
                }
            }

            // get uvs
            Vector4[] uvs = new Vector4[SpritesThisFrame.Count];
            for (int i = 0; i < uvs.Length; i++) {
                Sprite sprite = SpritesThisFrame[i];
                var rect = atlas.GetRect(sprite.Texture!);
                uvs[i] = new Vector4(rect.X, rect.Y, rect.Width, rect.Height);
            }
            Gl!.BindBuffer(BufferTargetARB.ArrayBuffer, uvVBO);
            unsafe {
                fixed (Vector4* data = uvs) {
                    Gl!.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(Vector4) * uvs.Length), data, BufferUsageARB.StaticDraw);
                }
            }

            var view = Engine.Camera!.ViewMatrix;
            var projection = Engine.Camera!.ProjectionMatrix(Config.ScreenWidth, Config.ScreenHeight);

            Shader.Use();
            Shader.SetUniform("uView", view);
            Shader.SetUniform("uProjection", projection);
            Shader.SetUniform("uAtlasSize", new Vector2(atlas.Texture!.Width, atlas.Texture!.Height));

            if (atlas.Texture is not null) {
                atlas.Texture.Bind();
            }

            Gl!.BindVertexArray(quadVAO);
            Gl!.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, (uint)SpritesThisFrame.Count);
        }
    }

    public void Clear() {
        SpritesByAtlas.Clear();
        RegisteredSpriteIDs.Clear();
    }

    public SpriteBatcher() {
        Shader = new Shader(new ShaderFile("assets/sprite.shader"));

        quadVAO = Gl!.GenVertexArray();
        quadVBO = Gl!.GenBuffer();
        offsetVBO = Gl!.GenBuffer();
        uvVBO = Gl!.GenBuffer();

        Gl!.BindBuffer(GLEnum.ArrayBuffer, quadVBO);
        unsafe {
            fixed (void* data = quadVertices) {
                Gl!.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(quadVertices.Length * sizeof(float)), data, BufferUsageARB.StaticDraw);
            }
        }

        // setup vertex data
        uint slot = 0;

        Gl!.BindVertexArray(quadVAO);
        Gl!.EnableVertexAttribArray(slot);
        Gl!.EnableVertexAttribArray(slot + 1);
        unsafe {
            Gl!.VertexAttribPointer(slot + 0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5, (void*)0);
            Gl!.VertexAttribPointer(slot + 1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, (void*)(sizeof(float) * 3));
        }
        slot += 2;

        // setup offset buffer
        Gl!.BindBuffer(BufferTargetARB.ArrayBuffer, offsetVBO);
        Gl!.EnableVertexAttribArray(slot + 0); // mat4 takes up 4 slots as float4s
        Gl!.EnableVertexAttribArray(slot + 1); // ..
        Gl!.EnableVertexAttribArray(slot + 2); // ..
        Gl!.EnableVertexAttribArray(slot + 3); // ..
        unsafe {
            Gl!.VertexAttribPointer(slot + 0, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 0));
            Gl!.VertexAttribPointer(slot + 1, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 1));
            Gl!.VertexAttribPointer(slot + 2, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 2));
            Gl!.VertexAttribPointer(slot + 3, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 3));
        }
        Gl!.VertexAttribDivisor(slot + 0, 1); // instanced row 1
        Gl!.VertexAttribDivisor(slot + 1, 1); // ..         .. 2
        Gl!.VertexAttribDivisor(slot + 2, 1); // ..         .. 3
        Gl!.VertexAttribDivisor(slot + 3, 1); // ..         .. 4
        slot += 4;

        // setup uv buffer
        Gl!.BindBuffer(BufferTargetARB.ArrayBuffer, uvVBO);
        Gl!.EnableVertexAttribArray(slot);
        unsafe {
            Gl!.VertexAttribPointer(slot, 4, VertexAttribPointerType.Float, false, sizeof(float) * 4, (void*)0);
        }
        Gl!.VertexAttribDivisor(slot, 1); // instanced 
    }
}
