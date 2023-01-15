using Silk.NET.OpenGL;

namespace FWGPUE;

class VertexArrayObject<T, TIndex> : IDisposable
    where T : unmanaged
    where TIndex : unmanaged {
    public uint Handle { get; }
    public GL Gl { get; }

    public void Bind() {
        Gl.BindVertexArray(Handle);
    }

    public void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize, int offset) {
        unsafe {
            Gl.VertexAttribPointer(index, count, type, false, vertexSize * (uint)sizeof(T), (void*)(offset * sizeof(T)));
            Gl.EnableVertexAttribArray(index);
        }
    }

    public VertexArrayObject(GL gl, BufferObject<T> vbo, BufferObject<TIndex> ebo) {
        Gl = gl;

        Handle = Gl.GenVertexArray();
        Bind();
        vbo.Bind();
        ebo.Bind();
    }

    public void Dispose() {
        Gl.DeleteVertexArray(Handle);
    }
}
