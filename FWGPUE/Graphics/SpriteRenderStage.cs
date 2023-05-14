using Silk.NET.OpenGL;
using System.Collections.Immutable;
using System.Numerics;

namespace FWGPUE.Graphics;

class SpriteRenderStage : RenderStage {
    static float[] quadVertices = {
        -0.5f,  0.5f, 0.0f, 0f, 1f, // B
        -0.5f, -0.5f, 0.0f, 0f, 0f, // A
         0.5f, -0.5f, 0.0f, 1f, 0f, // D
                                   
        -0.5f,  0.5f, 0.0f, 0f, 1f, // B
         0.5f, -0.5f, 0.0f, 1f, 0f, // D
         0.5f,  0.5f, 0.0f, 1f, 1f, // C
    };
    uint quadVAO;
    uint quadVBO;
    uint offsetVBO;
    uint uvVBO;

    Shader AtlasedSpriteShader;

    public override void Render(RenderStage? previous) {
        // don't try to render sprites if there are no loaded sprites
        if (CurrentScene?.Atlas == null) {
            return;
        }

        // get all sprites, sorted by Z
        ToRenderSprite[] sprites = Renderer.SpritesToRender.OrderBy(x => x.z).ToArray();

        // get sprite positions
        Matrix4x4[] offsets = new Matrix4x4[sprites.Length];
        Parallel.For(0, sprites.Length, (i) => {
            ToRenderSprite current = sprites[i];
            var rect = CurrentScene.Atlas.GetRect(current.name);

            var model =
                   Matrix4x4.CreateScale(current.scaleX * rect.Width, current.scaleY * rect.Height, 1) *
                   Matrix4x4.CreateRotationZ(TurnsToRadians(current.rotZ)) *
                   Matrix4x4.CreateRotationY(TurnsToRadians(current.rotY)) *
                   Matrix4x4.CreateRotationX(TurnsToRadians(current.rotX)) *
                   Matrix4x4.CreateTranslation(new(current.x, current.y, current.z));

            offsets[i] = model;
        });

        // get uvs
        Vector4[] uvs = new Vector4[sprites.Length];
        Parallel.For(0, uvs.Length, (i) => {
            ToRenderSprite current = sprites[i];
            var rect = CurrentScene.Atlas.GetRect(current.name);

            uvs[i] = new Vector4(rect.X, rect.Y, rect.Width, rect.Height);
        });


        Gl.BindVertexArray(quadVAO);
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, offsetVBO);
        // push sprite positions to gpu
        unsafe {
            fixed (Matrix4x4* data = offsets) {
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(Matrix4x4) * offsets.Length), data, BufferUsageARB.StaticDraw);
            }
        }

        // push UVs to gpu
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, uvVBO);
        unsafe {
            fixed (Vector4* data = uvs) {
                Gl!.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(Vector4) * uvs.Length), data, BufferUsageARB.StaticDraw);
            }
        }

        // draw sprites instanced
        AtlasedSpriteShader.Use();
        AtlasedSpriteShader.SetUniform("uView", Camera.ViewMatrix);
        AtlasedSpriteShader.SetUniform("uProjection", Camera.ProjectionMatrix);
        AtlasedSpriteShader.SetUniform("uAtlasSize", new Vector2(CurrentScene.Atlas.Texture!.Width, CurrentScene.Atlas.Texture.Height));
        CurrentScene.Atlas.Texture.Bind();

        Gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, (uint)sprites.Length);
    }

    public SpriteRenderStage() : base() {
        AtlasedSpriteShader = new Shader(new("assets/sprite.shader"));

        quadVAO = Gl.GenVertexArray();
        quadVBO = Gl.GenBuffer();
        offsetVBO = Gl.GenBuffer();
        uvVBO = Gl.GenBuffer();

        Gl.BindBuffer(GLEnum.ArrayBuffer, quadVBO);
        unsafe {
            fixed (float* data = quadVertices) {
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(quadVertices.Length * sizeof(float)), data, BufferUsageARB.StaticDraw);
            }
        }

        // setup shader input data
        uint slot = 0;

        Gl.BindVertexArray(quadVAO);
        Gl.EnableVertexAttribArray(slot);
        Gl.EnableVertexAttribArray(slot + 1);
        unsafe {
            Gl!.VertexAttribPointer(slot + 0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5, (void*)0);
            Gl!.VertexAttribPointer(slot + 1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, (void*)(sizeof(float) * 3));
        }
        slot += 2;

        // setup offset buffer
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, offsetVBO);
        Gl.EnableVertexAttribArray(slot + 0); // mat4 takes up 4 slots as float4s
        Gl.EnableVertexAttribArray(slot + 1); // ..
        Gl.EnableVertexAttribArray(slot + 2); // ..
        Gl.EnableVertexAttribArray(slot + 3); // ..
        unsafe {
            Gl.VertexAttribPointer(slot + 0, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 0));
            Gl.VertexAttribPointer(slot + 1, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 1));
            Gl.VertexAttribPointer(slot + 2, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 2));
            Gl.VertexAttribPointer(slot + 3, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Matrix4x4), (void*)(sizeof(float) * 4 * 3));
        }
        Gl.VertexAttribDivisor(slot + 0, 1); // instanced row 1
        Gl.VertexAttribDivisor(slot + 1, 1); // ..         .. 2
        Gl.VertexAttribDivisor(slot + 2, 1); // ..         .. 3
        Gl.VertexAttribDivisor(slot + 3, 1); // ..         .. 4
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
