using Silk.NET.OpenGL;
using FWGPUE.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Silk.NET.Core.Native;

namespace FWGPUE.Graphics;

class Texture : GLObject, IDisposable {
    public uint Handle { get; }

    public TextureTarget Target { get; }
    public InternalFormat Format { get; }

    public int Width { get; }
    public int Height { get; }

    public void Bind(TextureUnit slot = TextureUnit.Texture0) {
        Gl.ActiveTexture(slot);
        Gl.BindTexture(Target, Handle);
    }

    void SetParameters() {
        Gl.TexParameter(Target, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        Gl.TexParameter(Target, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        Gl.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        Gl.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        //Gl.GenerateMipmap(Target);
    }

    public unsafe void SetData(System.Drawing.Rectangle bounds, byte[] data) {
        Bind();
        fixed (byte* ptr = data) {
            Gl.TexSubImage2D(
            target: TextureTarget.Texture2D,
            level: 0,
            xoffset: bounds.Left,
            yoffset: bounds.Top,
            width: (uint)bounds.Width,
            height: (uint)bounds.Height,
            format: PixelFormat.Rgba,
            type: PixelType.UnsignedByte,
            pixels: ptr
        );
        }
    }

    // todo: cleanup and remove duplicated code
    public Texture(GL gl, int width, int height) : this(gl, Array.Empty<byte>(), width, height) { }
    public Texture(GL gl, byte[] data, int width, int height, TextureTarget target = TextureTarget.Texture2D, InternalFormat internalFormat = InternalFormat.Rgba)
        : base(gl) {
        Handle = Gl.GenTexture();
        Target = target;
        Format = internalFormat;

        Bind();

        Width = width;
        Height = height;

        unsafe {
            fixed (byte* d = data) {
                Gl.TexImage2D(Target, 0, Format, (uint)Width, (uint)Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            }
        }

        SetParameters();
    }
    public Texture(GL gl, ByteFile file, TextureTarget target = TextureTarget.Texture2D, InternalFormat internalFormat = InternalFormat.Rgba)
        : base(gl) {
        Handle = Gl.GenTexture();
        Target = target;
        Format = internalFormat;

        Bind();

        file.Load();
        using (var img = Image.Load<Rgba32>(file.Data)) {
            Width = img.Width;
            Height = img.Height;

            unsafe {
                Gl.TexImage2D(Target, 0, Format, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

                // load image row-by-row because imagesharp is bad
                img.ProcessPixelRows(accessor => {
                    for (int y = 0; y < accessor.Height; y++) {
                        fixed (void* data = accessor.GetRowSpan(y)) {
                            gl.TexSubImage2D(Target, 0, 0, y, (uint)accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                        }
                    }
                });
            }
        }

        SetParameters();
    }

    public void Dispose() {
        Gl.DeleteTexture(Handle);
    }
}
