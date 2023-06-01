namespace FWGPUE.Graphics;

class VertexArrayObject {
    public uint Handle;

    private int currentAddIndex;
    public FloatBufferObject[] BufferObjects = Array.Empty<FloatBufferObject>();
    public int SlotCount;

    bool Inbounds(int index) {
        return index >= 0 && index < BufferObjects.Length;
    }

    public void Bind() {
        Gl.BindVertexArray(Handle);
    }

    public void AddBufferObject(int slotSize, int sizeInFloats, int stride, bool instanced = false, int instanceDivisor = 1) {
        int newSlot = SlotCount;

        FloatBufferObject newObject = new();

        newObject.AttributePointer(Handle, newSlot, slotSize, sizeInFloats, stride);

        if (instanced) {
            newObject.Instanced(Handle, newSlot, slotSize, (uint)instanceDivisor);
        }

        SlotCount += slotSize;

        SetBufferObject(currentAddIndex++, newObject);
    }
    public void SetBufferObject(int index, FloatBufferObject bufferObject, bool resize = true) {
        if (index < 0) {
            Log.Error($"index negative ({index})");
            return;
        }

        while (!Inbounds(index)) {
            if (!resize) {
                return;
            }

            // resize the buffer object array if required.
            // .. doubling the size is a simple trick which doesn't use too much memory,
            // .. and doesn't reallocate too often.
            Array.Resize(ref BufferObjects, (BufferObjects.Length + 1) * 2);
        }

        // after previous code, this is guaranteed inbounds.
        BufferObjects[index] = bufferObject;
    }
    public void SetBufferData<T>(int index, T[] data)
        where T : unmanaged {

        if (!Inbounds(index)) {
            Log.Error($"can't set buffer data: index {index} out of range 0..{BufferObjects.Length}");
            return;
        }

        BufferObjects[index].SetData(Handle, data);
    }

    public VertexArrayObject() {
        Handle = Gl.GenVertexArray();
    }
}
