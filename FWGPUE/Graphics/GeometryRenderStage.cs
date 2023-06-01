using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

class GeometryRenderStage : RenderStage {
    VertexArrayObject Geometry;
    Shader GeometryShader;

    public override void Render(RenderStage? previous) {
        // get geometry data as floats
        ToRenderGeometry[] geo = Renderer.GeometryToRender.ToArray();
        float[] geometryData = new float[geo.Length * 6];

        Parallel.For(0, geo.Length, (i) => {
            ToRenderGeometry current = geo[i];

            int floatIndex = i * 6;
            geometryData[floatIndex + 0] = current.x;
            geometryData[floatIndex + 1] = current.y;
            geometryData[floatIndex + 2] = current.z;
            geometryData[floatIndex + 3] = current.colour.X;
            geometryData[floatIndex + 4] = current.colour.Y;
            geometryData[floatIndex + 5] = current.colour.Z;
        });

        Geometry.SetBufferData(0, geometryData);

        Geometry.Bind();
        GeometryShader.Use();
        GeometryShader.SetUniform("uView", Camera.ViewMatrix);
        GeometryShader.SetUniform("uProjection", Camera.ProjectionMatrix);

        Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)geo.Length);
    }

    public GeometryRenderStage() : base() {
        GeometryShader = new(new("assets/geometry.shader"));

        Geometry = new();
        Geometry.SetBufferObject(0, new());
        // position and colour are packed in the same array
        Geometry.BufferObjects[0].AttributePointer(Geometry.Handle, 0, 1, 3, 6); // position (3x float)
        Geometry.BufferObjects[0].AttributePointer(Geometry.Handle, 1, 1, 3, 6, 3); // colour (3x float)
    }
}