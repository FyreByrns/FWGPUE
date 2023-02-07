using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

public class VertexArrayObject : IDisposable {
    private readonly uint _handle;
    private readonly int _stride;

    public VertexArrayObject(int stride) {
        _stride = stride;

        Gl!.GenVertexArrays(1, out _handle);
    }

    public void Dispose() {
        Gl!.DeleteVertexArray(_handle);
    }

    public void Bind() {
        Gl!.BindVertexArray(_handle);
    }

    public unsafe void VertexAttribPointer(int location, int size, VertexAttribPointerType type, bool normalized, int offset) {
        Gl!.EnableVertexAttribArray((uint)location);
        Gl.VertexAttribPointer((uint)location, size, type, normalized, (uint)_stride, (void*)offset);
    }
}
