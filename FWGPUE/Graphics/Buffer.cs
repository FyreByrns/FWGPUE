using Silk.NET.Direct3D11;
using Silk.NET.Core.Native;

namespace FWGPUE.Graphics;

#region ease-of-use d3d aliases

using Device = ComPtr<ID3D11Device>;
using Buffer = ComPtr<ID3D11Buffer>;
#endregion

class Buffer<T>
    where T : unmanaged
{

    public Device Device { get; private set; }
    public Buffer D3DBuffer = default;
    public BufferDesc Description;
    public Usage Usage { get; set; } = Usage.Default;

    BindFlag _bindFlag;
    public BindFlag BindFlag
    {
        get => _bindFlag;
        set
        {
            _bindFlag = value; OnDataChanged();
        }
    }

    T[] _data;
    public T[] Data
    {
        get => _data;
        set
        {
            _data = value;
            OnDataChanged();
        }
    }

    public int Length => Data.Length;
    public unsafe int ByteSize => sizeof(T) * Data.Length;

    void OnDataChanged()
    {
        // update description
        Description = new()
        {
            ByteWidth = (uint)ByteSize,
            Usage = Usage,
            BindFlags = (uint)BindFlag
        };

        // update buffer in device
        unsafe
        {
            fixed (T* data = _data)
            {
                SubresourceData subData = new()
                {
                    PSysMem = data
                };
                SilkMarshal.ThrowHResult(Device.CreateBuffer(in Description, in subData, ref D3DBuffer));
            }
        }
    }

    private Buffer(Device device) { Device = device; }
    public Buffer(Device device, int initialSize)
        : this(device)
    {
        Data = new T[initialSize];
    }
    public Buffer(Device device, T[] data)
        : this(device)
    {
        Data = data;
    }
    public Buffer(Device device, T[] data, BindFlag bindFlags) : this(device, data)
    {
        BindFlag = bindFlags;
    }
}
