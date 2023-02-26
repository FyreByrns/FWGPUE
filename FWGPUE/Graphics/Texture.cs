using Silk.NET.OpenGL;
using FWGPUE.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Silk.NET.Core.Native;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.Advanced;

namespace FWGPUE.Graphics;

class Texture : IDisposable {
    public uint Handle { get; }

    public TextureTarget Target { get; }
    public InternalFormat Format { get; }

    public int Width { get; }
    public int Height { get; }

    public byte[] Data { get; protected set; }

    public void Bind(TextureUnit slot = TextureUnit.Texture0) {
        Gl!.ActiveTexture(slot);
        Gl.BindTexture(Target, Handle);
    }

    void SetParameters() {
        Gl!.TexParameter(Target, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        Gl.TexParameter(Target, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        Gl.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        Gl.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
    }

    public unsafe void SetData(System.Drawing.Rectangle bounds, byte[] data) {
        Bind();
        fixed (byte* d = data) {
            Gl!.TexSubImage2D(Target, 0, bounds.Left, bounds.Top, (uint)bounds.Width, (uint)bounds.Height, PixelFormat.Rgba, PixelType.UnsignedByte, d);
        }
    }

    // todo: cleanup and remove duplicated code
    public Texture(int width, int height) : this(Array.Empty<byte>(), width, height) { }
    public Texture(byte[] data, int width, int height, TextureTarget target = TextureTarget.Texture2D, InternalFormat internalFormat = InternalFormat.Rgba) {
        Handle = Gl!.GenTexture();
        Target = target;
        Format = internalFormat;

        Bind();

        Width = width;
        Height = height;
        Data = data;

        unsafe {
            fixed (byte* d = data) {
                Gl.TexImage2D(Target, 0, Format, (uint)Width, (uint)Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            }
        }

        SetParameters();
    }
    public Texture(ByteFile file, TextureTarget target = TextureTarget.Texture2D, InternalFormat internalFormat = InternalFormat.Rgba) {
        Handle = Gl!.GenTexture();
        Target = target;
        Format = internalFormat;

        Bind();

        file.Load();
        using (var img = Image.Load<Rgba32>(file.Data)) {
            Width = img.Width;
            Height = img.Height;
            Data = new byte[Width * Height * 4];
            img.CopyPixelDataTo(Data);

            unsafe {
                Gl.TexImage2D(Target, 0, Format, (uint)Width, (uint)Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

                fixed (void* data = Data) {
                    Gl.TexImage2D(Target, 0, InternalFormat.Rgba, (uint)Width, (uint)Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                }
            }
        }

        SetParameters();
    }

    public void Dispose() {
        Gl!.DeleteTexture(Handle);
    }
}
