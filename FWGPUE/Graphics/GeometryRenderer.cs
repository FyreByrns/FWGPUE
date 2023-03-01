using FWGPUE.IO;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FWGPUE.Graphics {
    /// <summary>
    /// Used to draw solid-coloured geometry.
    /// </summary>
    class GeometryRenderer {
        public static readonly int MAX_VERTICES = 1024;
        public static readonly int MAX_VERTEX_ARRAY_SIZE = MAX_VERTICES * 6;

        public VertexArrayObject VAO;
        public BufferObject<float> Buffer;

        private int _vertexIndex = 0;
        private readonly float[] _vertexData = new float[MAX_VERTEX_ARRAY_SIZE];

        public uint VBO { get; protected set; }
        public Shader Shader { get; protected set; }

        public void PushVertex(Vector3 colour, Vector3 position) {
            if (_vertexIndex > _vertexData.Length) {
                Log.Error("too many vertices");
            }

            try {
                _vertexData[_vertexIndex + 0] = position.X;
                _vertexData[_vertexIndex + 1] = position.Y;
                _vertexData[_vertexIndex + 2] = position.Z;
                _vertexData[_vertexIndex + 3] = colour.X;
                _vertexData[_vertexIndex + 4] = colour.Y;
                _vertexData[_vertexIndex + 5] = colour.Z;
                _vertexIndex += 6;
            }
            catch (Exception e) {
                Log.Error(e.StackTrace);
            }
        }

        public void FlushVertexBuffer() {
            var view = Camera!.ViewMatrix;
            var projection = Camera!.ProjectionMatrix;

            Shader.Use();
            Shader.SetUniform("uView", view);
            Shader.SetUniform("uProjection", projection);

            Buffer.Bind();
            Buffer.SetData(_vertexData, 0, _vertexIndex);
            VAO.Bind();

            Gl!.Disable(EnableCap.DepthTest);
            Gl.Disable(EnableCap.CullFace);
            Gl.DrawArrays(GLEnum.Triangles, 0, (uint)(_vertexIndex / 6));

            _vertexIndex = 0;
        }

        public GeometryRenderer() {
            Shader = new(new ShaderFile("assets/geometry.shader"));

            VAO = new VertexArrayObject(sizeof(float) * 6);
            Buffer = new BufferObject<float>(MAX_VERTEX_ARRAY_SIZE, BufferTargetARB.ArrayBuffer, true);

            VAO.Bind();
            VAO.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0);
            VAO.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3);
        }
    }
}
