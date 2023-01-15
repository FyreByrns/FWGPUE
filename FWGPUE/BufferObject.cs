using Silk.NET.OpenGL;

namespace FWGPUE;

class BufferObject<T> : IDisposable
    where T : unmanaged {

    public uint Handle { get; }
    public BufferTargetARB BufferType { get; }
    public GL Gl { get; }

    public void Bind() {
        Gl.BindBuffer(BufferType, Handle);
    }

    public BufferObject(GL gl, Span<T> data, BufferTargetARB bufferType, BufferUsageARB usage = BufferUsageARB.StaticDraw) {
        Gl = gl;
        BufferType = bufferType;

        Handle = Gl.GenBuffer();
        Bind();

        unsafe {
            fixed (void* d = data) {
                Gl.BufferData(BufferType, (nuint)(data.Length * sizeof(T)), d, usage);
            }
        }
    }

    public void Dispose() { 
        Gl.DeleteBuffer(Handle);
    }
}
