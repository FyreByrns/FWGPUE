using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

abstract class RenderStage {
    public Framebuffer Target;

    public abstract void Render(RenderStage? previous);

    protected RenderStage() {
        Target = new(Config.ScreenWidth, Config.ScreenHeight);
    }
}

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

class ClearColourStage : RenderStage {
    public override void Render(RenderStage? previous) {
        Target.Bind();
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        Gl.Enable(GLEnum.DepthTest);
        Gl.ClearColor(0, 0, 0, 0);
    }

    public ClearColourStage() : base() { }
}

class Renderer {
    public delegate void FrameHandler(double elapsed);
    /// <summary>
    /// Invoked each frame.
    /// </summary>
    public event FrameHandler? OnFrame;
    /// <summary>
    /// Invoked when the renderer wants to be given all objects that will render this frame.
    /// </summary>
    public event FrameHandler? OnRenderObjectsRequired;
    /// <summary>
    /// Invoked each render.
    /// </summary>
    public event FrameHandler? OnRender;

    public delegate void LoadHandler();
    /// <summary>
    /// Invoked after the window is created and the OpenGL context exists.
    /// </summary>
    public event LoadHandler? OnLoad;
    public delegate void CloseHandler();
    /// <summary>
    /// Invoked when the close button is pressed.
    /// </summary>
    public event CloseHandler? OnClose;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static GL Gl { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static IWindow? Window { get; private set; }

    Shader fullscreenQuadShader;
    uint screenQuadVAO;
    uint screenQuadVBO;
    public List<RenderStage> RenderStages = new();

    private void OnFrameRender(double elapsed) {
        // render all stages
        RenderStage? previous = null;
        foreach (RenderStage stage in RenderStages) {
            stage.Render(previous);
            previous = stage;
        }

        // rebind main framebuffer
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        // render to it
        fullscreenQuadShader.Use();
        Gl.BindVertexArray(screenQuadVAO);
        Gl.BindTexture(TextureTarget.Texture2D, previous!.Target.Colour);
        Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        Gl.BindVertexArray(0);
    }

    public void Setup() {
        WindowOptions options = WindowOptions.Default with {
            Size = new Vector2D<int>(Config.ScreenWidth, Config.ScreenHeight),
            Title = "FWGPUE",
            Samples = 8,
        };
        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Update += (e) => {
            OnFrame?.Invoke(e);
            OnRenderObjectsRequired?.Invoke(e);
        };
        Window.Load += () => OnLoad?.Invoke();
        Window.Render += (e) => OnRender?.Invoke(e);

        Window.FramebufferResize += newSize => Gl!.Viewport(newSize);

        Window.Initialize();

        Gl = GL.GetApi(Window);
        Gl.Enable(GLEnum.Multisample);
        Gl.Enable(GLEnum.Blend);
        Gl.Viewport(0, 0, (uint)Config.ScreenWidth, (uint)Config.ScreenHeight);
        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        fullscreenQuadShader = new(new("assets/fullscreenquad.shader"));

        screenQuadVAO = Gl.GenVertexArray();
        screenQuadVBO = Gl.GenBuffer();

        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, screenQuadVBO);
        Gl.BindVertexArray(screenQuadVAO);
        float[] vs = new float[] {
            -1f, -1f, 0f, 0f, /**/ 1f, -1f, 1f, 0f, /**/ -1f, 1f, 0f, 1f,
            1f, -1f, 1f, 0f, /**/ 1f, 1f, 1f, 1f, /**/ -1f, 1f, 0f, 1f,
        };
        unsafe {
            fixed (float* d = vs) {
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(sizeof(float) * vs.Length), d, BufferUsageARB.StaticDraw);
            }

            Gl.EnableVertexAttribArray(0);
            Gl.EnableVertexAttribArray(1);

            Gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, (void*)0);
            Gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, (void*)(sizeof(float) * 2));
        }

        Gl.BindVertexArray(0);

        RenderStages.Add(new ClearColourStage());
    }

    public void Begin() {
        Window!.Run();
    }

    public void Exit() {
        Window!.Close();
    }

    public Renderer() {
        OnRender += OnFrameRender;
    }
}
