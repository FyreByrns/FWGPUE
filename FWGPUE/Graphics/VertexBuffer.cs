using Silk.NET.Direct3D11;
using Silk.NET.Core.Native;

namespace FWGPUE.Graphics;

class VertexBuffer<T>
    : Buffer<T>
    where T : unmanaged
{
    public uint Stride;
    public uint StrideInBytes;
    public uint Offset;

    public VertexBuffer(Device device, T[] data, uint stride, uint offset) : base(device, data, BindFlag.VertexBuffer)
    {
        Stride = stride;
        unsafe { StrideInBytes = (uint)(stride * sizeof(T)); }
        Offset = offset;
    }
}
