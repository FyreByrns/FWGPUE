using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Numerics;

namespace FWGPUE.Graphics;

class DrawObject {
    public VertexBuffer<float> VertexBuffer;
    public Buffer<uint> IndexBuffer;
    public Shader Shader;
    public D3DPrimitiveTopology Topology;

    /// <summary>
    /// Set the data for a specific element within the vertex buffer.
    /// </summary>
    /// <param name="vertex">Which vertex.</param>
    /// <param name="offsetWithinVertex">Which element within the vertex.</param>
    public void SetVertexInfo(float value, int vertex, int element) {
        int overallIndex = (int)(vertex * VertexBuffer.Stride) + element;
        VertexBuffer[overallIndex] = value;
    }

    public void Draw(DeviceContext context) {
        unsafe {
            // use shader input layout
            context.IASetPrimitiveTopology(Topology);
            context.IASetInputLayout(Shader.inputLayout);
            context.IASetVertexBuffers(0, 1, VertexBuffer.D3DBuffer, in VertexBuffer.StrideInBytes, in VertexBuffer.Offset);
            context.IASetIndexBuffer(IndexBuffer.D3DBuffer, Format.FormatR32Uint, 0);
        }

        // bind shaders
        context.VSSetShader(Shader.vertexShader, ref nullref<ClassInstance>(), 0);
        context.PSSetShader(Shader.pixelShader, ref nullref<ClassInstance>(), 0);

        // draw
        context.DrawIndexed((uint)IndexBuffer.Length, 0, 0);
    }

    public DrawObject(VertexBuffer<float> vertexBuffer, Buffer<uint> indexBuffer, Shader shader, D3DPrimitiveTopology topology) {
        VertexBuffer = vertexBuffer;
        IndexBuffer = indexBuffer;
        Shader = shader;
        Topology = topology;
    }
}
