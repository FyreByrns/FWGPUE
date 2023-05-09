using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace FWGPUE.Graphics;

class Renderer {
    public delegate void FrameHandler(double elapsed);
    public event FrameHandler? OnFrame;
    public event FrameHandler? OnRender;

    public delegate void LoadHandler();
    public event LoadHandler? OnLoad;
    public delegate void CloseHandler();
    public event CloseHandler? OnClose;

    public static GL? Gl { get; private set; }
    public static IWindow? Window { get; private set; }

    private void OnFrameRender(double elapsed) {
        Gl!.ClearColor(1, 0.2f, 0.5f, 1);
        Gl!.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.ColorBufferBit);
    }

    public void SetupWindow() {
        WindowOptions options = WindowOptions.Default with {
            Size = new Vector2D<int>(Config.ScreenWidth, Config.ScreenHeight),
            Title = "FWGPUE",
            Samples = 8,
        };
        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Update += (e) => OnFrame?.Invoke(e);
        Window.Load += () => OnLoad?.Invoke();
        Window.Render += (e) => OnRender?.Invoke(e);

        Window.FramebufferResize += newSize => Gl!.Viewport(newSize);

        Window.Initialize();

        Gl = GL.GetApi(Window);
        Gl.Enable(GLEnum.Multisample);
        Gl.Enable(GLEnum.Blend);
        Gl.Viewport(0, 0, (uint)Config.ScreenWidth, (uint)Config.ScreenHeight);
        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
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
