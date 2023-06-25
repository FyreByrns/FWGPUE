using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

class Framebuffer {
    public uint Handle;
    public uint Colour;
    public uint DepthStencil;

    public void Bind() {
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

        if (Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete) {
            Log.Warn("binding incomplete framebuffer");
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return;
        }

        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public Framebuffer(int width, int height) {
        Handle = Gl.GenFramebuffer();
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

        Colour = Gl.GenTexture();
        Gl.BindTexture(GLEnum.Texture2D, Colour);
        unsafe {
            Gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)width, (uint)height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, null);
        }
        Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        Gl.BindTexture(GLEnum.Texture2D, 0);
        Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Colour, 0);

        DepthStencil = Gl.GenRenderbuffer();
        Gl.BindRenderbuffer(GLEnum.Renderbuffer, DepthStencil);
        Gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth24Stencil8, (uint)width, (uint)height);
        Gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);

        Gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthStencilAttachment, GLEnum.Renderbuffer, DepthStencil);

        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
}
