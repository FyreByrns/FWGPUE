using Silk.NET.OpenGL;
using FWGPUE.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FWGPUE;

class Texture : IDisposable {
    public uint Handle { get; }
    public GL Gl { get; }

    public TextureTarget Target { get; }
    public InternalFormat Format { get; }

    public void Bind(TextureUnit slot = TextureUnit.Texture0) {
        Gl.ActiveTexture(slot);
        Gl.BindTexture(Target, Handle);
    }

    void SetParameters() {
        Gl.TexParameter(Target, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        Gl.TexParameter(Target, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        Gl.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        Gl.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
        Gl.GenerateMipmap(Target);
    }

    public Texture(GL gl, ByteFile file, TextureTarget target = TextureTarget.Texture2D, InternalFormat internalFormat = InternalFormat.Rgb8) {
        Gl = gl;
        Handle = Gl.GenTexture();
        Target = target;
        Format = internalFormat;

        Bind();

        file.Load();
        using (var img = Image.Load<Rgba32>(file.Data)) {
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
