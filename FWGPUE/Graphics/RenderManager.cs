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

    public static TextManager TextManager { get; private set; }

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
        PushVertex(new(b, z), colour);
        PushVertex(new(c, z), colour);
    }
    public void PushLine(Vector2 a, Vector2 b, float z, Vector3 colour, float thickness = 1) {
        float angleBetween = RadiansToTurns((float)Math.Atan2(a.Y - b.Y, a.X - b.X));
        float anglePlusHalf = angleBetween + 0.5f;

        thickness /= 2;

        Vector2 _a = a.Along(+thickness, anglePlusHalf);
        Vector2 _b = a.Along(-thickness, anglePlusHalf);
        Vector2 _c = b.Along(+thickness, anglePlusHalf);
        Vector2 _d = b.Along(-thickness, anglePlusHalf);

        PushTriangle(_a, _c, _b, z, colour);
        PushTriangle(_b, _c, _d, z, colour);
    }
    public void PushLine(float ax, float ay, float bx, float by, float z, Vector3 colour, float thickness = 1) {
        PushLine(new(ax, ay), new(bx, by), z, colour, thickness);
    }
    public void PushRect(Vector2 topLeft, Vector2 bottomRight, float z, Vector3 colour, float outlineThickness = 0, Vector3? outlineColour = null, bool filled = true) {
        Vector2 offsetUp = new(0, -outlineThickness / 2f);
        Vector2 offsetDown = new(0, outlineThickness / 2f);
        Vector2 offsetLeft = new(-outlineThickness / 2f, 0);
        Vector2 offsetRight = new(outlineThickness / 2f, 0);

        Vector2 a = topLeft + offsetRight + offsetDown;
        Vector2 b = new Vector2(bottomRight.X, topLeft.Y) + offsetLeft + offsetDown;
        Vector2 c = new Vector2(topLeft.X, bottomRight.Y) + offsetRight + offsetUp;
        Vector2 d = bottomRight + offsetLeft + offsetUp;

        if (filled) {
            PushTriangle(a, b, c, z, colour);
            PushTriangle(c, b, d, z, colour);
        }

        if (outlineThickness > 0 && outlineColour is not null) {
            PushLine(a + offsetLeft, b + offsetRight, z + 1, outlineColour ?? Vector3.One, outlineThickness);
            PushLine(b + offsetUp, d + offsetDown, z + 1, outlineColour ?? Vector3.One, outlineThickness);
            PushLine(d + offsetRight, c + offsetLeft, z + 1, outlineColour ?? Vector3.One, outlineThickness);
            PushLine(c + offsetDown, a + offsetUp, z + 1, outlineColour ?? Vector3.One, outlineThickness);
        }
    }
    public void PushCircle(Vector2 center, float radius, float z, Vector3 colour, bool filled = true, int points = 30) {
        // generate vertices
        float change = 1f / (float)points;
        for (int i = 0; i < points; i++) {
            int last = i - 1;
            Vector2 a = center.Along(radius, last * change);
            Vector2 b = center.Along(radius, i * change);

            if (filled) {
                PushTriangle(a, b, center, z, colour);
            }
            else {
                PushLine(a, b, z, colour);
            }
        }
    }
    public void PushConvexPolygon(float z, Vector3 colour, bool filled = true, bool outline = false, float thickness = 1, params Vector2[] points) {
        if (points.Length == 0) {
            return;
        }

        // if there are fewer points than 2, it's just a point
        if (points.Length < 2) {
            PushCircle(points[0], thickness, z, colour, filled);
            return;
        }

        // if there are only two, then it's a line
        if (points.Length == 2) {
            PushLine(points[0], points[1], z, colour, thickness);
            return;
        }

        // otherwise, handle as normal
        Vector2 lastPoint = points.Last();
        foreach (Vector2 point in points) {
            if (filled) {
                PushTriangle(points[0], point, lastPoint, z, colour);
            }

            if (outline) {
                PushLine(lastPoint, point, z + 1, colour, thickness);
                PushCircle(point, thickness / 2, z + 1, colour);
            }

            lastPoint = point;
        }
    }

    public record ToRenderText(Vector3 location, Vector2 scale, string font, string text, Vector3 colour);
    public List<ToRenderText> TextToRender = new();
    public void PushString(Vector3 location, string font, Vector2 scale, string text, Vector3 colour) {
        TextToRender.Add(new(location, scale, font, text, colour));
    }

    void OnFrameRender(double elapsed) {
        // request all render objects
        OnRenderObjectsRequired?.Invoke(elapsed);

        // push text
        foreach(var text in TextToRender) {
            foreach (var poly in TextManager.GetTextPolygons(text.font, text.text)) {
                PushConvexPolygon(text.location.Z, text.colour, true, false, 0.1f, poly.ScaleAll(text.scale).TransformAll(text.location.XY()).ToArray());
            }
        }
        TextToRender.Clear();

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
            FramesPerSecond = Config.FPS,
            Title = "FWGPUE",
            Samples = 8,
        };
        Log.Info(options.FramesPerSecond);
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

        TextManager = new();
        TextManager.LoadFont("default");
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
