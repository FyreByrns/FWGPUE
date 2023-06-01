using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

class FloatBufferObject {
    public uint Handle;

    /// <summary>
    /// Setup an attribute pointer to the buffer.
    /// </summary>
    /// <param name="vertexArray">Which VAO the buffer belongs to.</param>
    /// <param name="slot">Which attribute slot this buffer starts at.</param>
    /// <param name="slots">
    /// How many attribute slots this buffer fills.
    /// <para>Anything smaller than a Vector4 takes one slot.</para>
    /// <para>A Matrix4x4 takes 4 slots.</para>
    /// </param>
    /// <param name="size">How many floats this attribute takes up per slot.</param>
    /// <param name="stride">How large the overall vertex data this attribute is part of is.</param>
    public void AttributePointer(uint vertexArray, int slot, int slots, int size, int stride) {
        Gl.BindVertexArray(vertexArray);
        Bind();
        unsafe {
            for (int i = 0; i < slots; i++) {
                Log.Info($"{i + 1} of {slots} from {slot} ({slot + i} overall)");
                Gl.EnableVertexArrayAttrib(vertexArray, (uint)(slot + i));
                Gl.VertexAttribPointer((uint)(slot + i), size, VertexAttribPointerType.Float, false, (uint)(stride * sizeof(float)), (void*)(stride * sizeof(float) * i));
                Log.Info(".. setup");
            }
        }
    }
    /// <summary>
    /// Set how the data is treated for instancing.
    /// </summary>
    /// <param name="slots">How many input slots this input takes.</param>
    /// <param name="divisor">
    /// How many instances until a new element is read from.
    /// <para> 1 would mean every instance reads from the next element in the buffer.</para>
    /// <para> 3 would mean every third instance reads from the next element in the buffer.</para>
    /// </param>
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
