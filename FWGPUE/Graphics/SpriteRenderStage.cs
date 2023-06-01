using Silk.NET.OpenGL;
using System.Numerics;

namespace FWGPUE.Graphics;

class SpriteRenderStage : RenderStage {
    static float[] quadVertices = {
        -0.5f,  0.5f, 0.0f, // B
        -0.5f, -0.5f, 0.0f, // A
         0.5f, -0.5f, 0.0f, // D
                           
        -0.5f,  0.5f, 0.0f, // B
         0.5f, -0.5f, 0.0f, // D
         0.5f,  0.5f, 0.0f, // C
    };
    static float[] quadUVs = {
        0f, 1f, // B
        0f, 0f, // A
        1f, 0f, // D
               
        0f, 1f, // B
        1f, 0f, // D
        1f, 1f, // C
    };

    VertexArrayObject Quad;
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

        Quad.SetBufferData(2, offsets);
        Quad.SetBufferData(3, uvs);

        // draw sprites instanced
        Quad.Bind();
        AtlasedSpriteShader.Use();
        AtlasedSpriteShader.SetUniform("uView", Camera.ViewMatrix);
        AtlasedSpriteShader.SetUniform("uProjection", Camera.ProjectionMatrix);
        AtlasedSpriteShader.SetUniform("uAtlasSize", new Vector2(CurrentScene.Atlas.Texture!.Width, CurrentScene.Atlas.Texture.Height));
        CurrentScene.Atlas.Texture.Bind();

        Gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, (uint)sprites.Length);
    }

    public SpriteRenderStage() : base() {
        AtlasedSpriteShader = new Shader(new("assets/sprite.shader"));

        Quad = new();
        Quad.AddBufferObject(1, 3, 3); // vertices
        Quad.AddBufferObject(1, 2, 2); // uvs
        Quad.AddBufferObject(4, 4, 4, true, 1); // transform
        Quad.AddBufferObject(1, 4, 4, true, 1); // atlas UV

        Quad.SetBufferData(0, quadVertices);
        Quad.SetBufferData(1, quadUVs);
    }
}
