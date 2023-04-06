using Silk.NET.Direct3D11;
using Silk.NET.Core.Native;

namespace FWGPUE.Graphics;

#region ease-of-use d3d aliases

using Device = ComPtr<ID3D11Device>;
#endregion
class VertexBuffer<T>
    : Buffer<T>
    where T : unmanaged
{
    public uint ByteStride;
    public uint Offset;

    public VertexBuffer(Device device, T[] data, uint stride, uint offset) : base(device, data, BindFlag.VertexBuffer)
    {
        unsafe { ByteStride = (uint)(stride * sizeof(T)); }
        Offset = offset;
    }
}
