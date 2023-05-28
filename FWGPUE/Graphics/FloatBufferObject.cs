using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

class FloatBufferObject {
    public uint Handle;

    public void AttributePointer(uint vertexArray, int slot, int slots, int size, int vertexSize) {
        Gl.BindVertexArray(vertexArray);
        Bind();
        unsafe {
            for (int i = 0; i < slots; i++) {
                Log.Info($"{i + 1} of {slots} from {slot} ({slot + i} overall)");
                Gl.EnableVertexArrayAttrib(vertexArray, (uint)(slot + i));
                Gl.VertexAttribPointer((uint)(slot + i), size, VertexAttribPointerType.Float, false, (uint)(vertexSize * sizeof(float)), (void*)(vertexSize * sizeof(float) * i));
            }
        }
    }
    public void Instanced(uint vertexArray, int slot, int slots, uint divisor) {
        Gl.BindVertexArray(vertexArray);
        Bind();
        unsafe {
            for (int i = 0; i < slots; i++) {
                Gl.VertexAttribDivisor((uint)(slot + i), divisor);
            }
        }
    }

    /// <summary>
    /// Set data in the buffer object by some starting index.
    /// <para>Index is by <typeparamref name="T"/>, not index * sizeof <typeparamref name="T"/>.</para>
    /// <para>So the second element of a float array would be 1, not 4.</para>
    /// </summary>
    public unsafe void SetData<T>(uint vertexArray, params T[] data)
        where T : unmanaged {

        Bind();
        // actually set the data
        fixed (void* d = data) {
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(data.Length * sizeof(T)), d, BufferUsageARB.StaticDraw);
        }
    }

    public void Bind() {
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, Handle);
    }

    public FloatBufferObject() {
        Handle = Gl.GenBuffer();
    }
}
