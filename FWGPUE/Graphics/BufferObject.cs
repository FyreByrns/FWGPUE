using Silk.NET.OpenGL;
using System.Runtime.InteropServices;

namespace FWGPUE.Graphics;

public class BufferObject<T> : GLObject, IDisposable where T : unmanaged
{
    private readonly uint _handle;
    private readonly BufferTargetARB _bufferType;
    private readonly int _size;

    public unsafe BufferObject(GL gl, int size, BufferTargetARB bufferType, bool isDynamic) : base(gl)
    {
        _bufferType = bufferType;
        _size = size;

        _handle = Gl.GenBuffer();

        Bind();

        var elementSizeInBytes = Marshal.SizeOf<T>();
        Gl.BufferData(bufferType, (nuint)(size * elementSizeInBytes), null, isDynamic ? BufferUsageARB.StreamDraw : BufferUsageARB.StaticDraw);
    }

    public void Bind()
    {
        Gl.BindBuffer(_bufferType, _handle);
    }

    public void Dispose()
    {
        Gl.DeleteBuffer(_handle);
    }

    public unsafe void SetData(T[] data, int startIndex, int elementCount)
    {
        Bind();

        fixed (T* dataPtr = &data[startIndex])
        {
            var elementSizeInBytes = sizeof(T);

            Gl.BufferSubData(_bufferType, 0, (nuint)(elementCount * elementSizeInBytes), dataPtr);
        }
    }
}
