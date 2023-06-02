#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Numerics;

namespace FWGPUE.Graphics;

class RenderManager {
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

    public static GL Gl { get; private set; }
    public static IWindow? Window { get; private set; }

    public List<RenderStage> RenderStages = new();

    public record ToRenderSprite(float x, float y, float z, string name, float scaleX, float scaleY, float rotX, float rotY, float rotZ);
    public HashSet<ToRenderSprite> SpritesToRender = new();
    /// <summary>
    /// Request rendering of a named sprite from the current scene's atlas.
    /// </summary>
    /// <param name="spriteName">Name in atlas.</param>
    public void PushSprite(float x, float y, float z, string spriteName, float scaleX = 1, float scaleY = 1, float rotX = 0, float rotY = 0, float rotZ = 0) {
        SpritesToRender.Add(new(x, y, z, spriteName, scaleX, scaleY, rotX, rotY, rotZ));
    }

    public record ToRenderGeometry(float x, float y, float z, Vector3 colour);
    public List<ToRenderGeometry> GeometryToRender = new();
    public void PushVertex(Vector3 position, Vector3 colour) {
        GeometryToRender.Add(new(position.X, position.Y, position.Z, colour));
    }
    /// <summary>
    /// Request rendering of a triangle at a certain Z level.
    /// </summary>
    public void PushTriangle(Vector2 a, Vector2 b, Vector2 c, float z, Vector3 colour) {
        PushVertex(new(a, z), colour);
        PushVertex(new(a, z), colour);
        PushVertex(new(a, z), colour);
    }
    public void PushLine(float ax, float ay, float bx, float by, float z, Vector3 colour, float thickness = 1) {
        // construct line out of two triangles
        Vector2 start = new(ax, ay);
        Vector2 end = new(bx, by);

        float angleBetween = RadiansToTurns((float)Math.Atan2(start.Y - end.Y, start.X - end.X));
        float anglePlusHalf = angleBetween + 0.5f;

        thickness /= 2;

        Vector2 a = start.Along(+thickness, anglePlusHalf);
        Vector2 b = start.Along(-thickness, anglePlusHalf);
        Vector2 c = end.Along(+thickness, anglePlusHalf);
        Vector2 d = end.Along(-thickness, anglePlusHalf);

        PushTriangle(a, c, b, z, colour);
        PushTriangle(b, c, d, z, colour);
    }

    void OnFrameRender(double elapsed) {
        // render all stages
        RenderStage? previous = null;
        foreach (RenderStage stage in RenderStages) {
            // each stage is rendered, then the render target is set to the result of that render
            stage.Render(previous);
            stage.Target.Bind();
            previous = stage;
        }

        // rebind main framebuffer
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        // render to it
        previous!.DrawToBackbuffer();

        // clear buffers
        SpritesToRender.Clear();
        GeometryToRender.Clear();
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

        RenderStages.Add(new ClearColourStage());
        RenderStages.Add(new SpriteRenderStage());
        RenderStages.Add(new GeometryRenderStage());
    }

    public void Begin() {
        Window!.Run();
    }

    public void Exit() {
        Window!.Close();
    }

    public RenderManager() {
        OnRender += OnFrameRender;
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
