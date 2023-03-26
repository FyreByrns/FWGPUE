using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using FWGPUE.IO;
using System.Numerics;

using FWGPUE.Graphics;
using FWGPUE.Scenes;

namespace FWGPUE;

static class Engine {
    #region timing

    public static float TotalSeconds { get; private set; } = 0;
    public static float LastFrameTime { get; private set; }
    public static float TickTimer { get; private set; }
    public static float TickTime => 1f / Config.TickRate;

    #endregion timing

    #region scene management

    public static Scene? CurrentScene { get; private set; }
    public static Scene? NextScene { get; private set; }
    public static bool WaitingToChangeScenes { get; private set; }

    public static void ChangeToScene(Scene? scene) {
        WaitingToChangeScenes = true;
        NextScene = scene;
    }

    #endregion scene management

    #region rendering

    public static BaseCamera Camera;

    public static GL? Gl { get; private set; }
    public static IWindow? Window { get; private set; }

    public enum TextAlignment {
        None = 0,

        TopLeft,
        MiddleLeft,
        BottomLeft,

        TopMiddle,
        MiddleMiddle,
        BottomMiddle,

        TopRight,
        MiddleRight,
        BottomRight,

        Normal = TopLeft,
        Center = MiddleMiddle,
    }
    public record TextDrawData(string text, Vector2 location, TextColour colour, float size, Vector2 scale, float rotation, TextAlignment alignment);
    public static HashSet<TextDrawData> TextThisFrame = new();

    public static void DrawText(string text, Vector2 location, TextColour colour, float size = 10, TextAlignment alignment = TextAlignment.Normal) {
        TextThisFrame.Add(new(text, location, colour, size, new(1, 1), 0, alignment));
    }
    public static void DrawTextRotated(string text, Vector2 location, float rotation, TextColour colour, float size = 10, TextAlignment alignment = TextAlignment.Normal) {
        TextThisFrame.Add(new(text, location, colour, size, new(1, 1), rotation, alignment));
    }

    /// <summary>
    /// Draw an image from the current scene's atlas.
    /// </summary>
    public static void DrawImage(string name, Vector2 location, float z = 0, float size = 1, float rotation = 0) {
    }

    public record VertexDrawData(Vector3 colour, Vector3 position);
    public static List<VertexDrawData> VerticesThisFrame = new();

    public static void DrawTriangle(Vector3 colour, Vector2 a, Vector2 b, Vector2 c) {
        VerticesThisFrame.Add(new VertexDrawData(colour, new(a, 1)));
        VerticesThisFrame.Add(new VertexDrawData(colour, new(b, 1)));
        VerticesThisFrame.Add(new VertexDrawData(colour, new(c, 1)));
    }
    public static void DrawLine(Vector3 colour, Vector2 start, Vector2 end, float thickness = 0.5f) {
        float angleBetween = RadiansToTurns((float)Math.Atan2(start.Y - end.Y, start.X - end.X));
        float anglePlusHalf = angleBetween + 0.5f;

        thickness /= 2;

        Vector2 a = start.Along(+thickness, anglePlusHalf);
        Vector2 b = start.Along(-thickness, anglePlusHalf);
        Vector2 c = end.Along(+thickness, anglePlusHalf);
        Vector2 d = end.Along(-thickness, anglePlusHalf);

        DrawTriangle(colour, a, c, b);
        DrawTriangle(colour, b, c, d);
    }
    public static void DrawCircle(Vector3 colour, Vector2 location, float radius) {
        const int vertices = 10;
        const float turnsPer = 1f / vertices;
        Vector2[] points = new Vector2[vertices];

        for (int i = 0; i < vertices; i++) {
            points[i] = location.Along(radius, turnsPer);
        }

        for (int i = 0; i < vertices; i++) {
            int next = i + 1;
            if (next >= vertices) {
                next = 0;
            }

            DrawLine(colour, points[i], points[next], 4);
        }
    }

    #endregion rendering

    #region initialization

    static void Start() {
        Init();
        MainLoop();
        End();
    }

    static void Init() {
        InitWindow();
        Input.Init();
        InitGraphics();

        ChangeToScene(new StartupSplash());
    }

    static void InitWindow() {
        WindowOptions options = WindowOptions.Default with {
            Size = new Vector2D<int>(Config.ScreenWidth, Config.ScreenHeight),
            Title = "FWGPUE",
            Samples = 8,
        };

        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Load += Load;
        Window.Update += Update;
        Window.Render += Render;
        Window.Closing += Closing;

        Window.FramebufferResize += newSize => Gl!.Viewport(newSize);

        Window.Initialize();
    }

    static void InitGraphics() {
        Gl = GL.GetApi(Window);
        Gl.Enable(GLEnum.Multisample);
        Gl.Enable(GLEnum.Blend);
        Gl.Viewport(0, 0, (uint)Config.ScreenWidth, (uint)Config.ScreenHeight);
        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

    }

    #endregion initialization

    #region engine meta-state

    public static bool ShutdownComplete { get; private set; } = false;

    static void MainLoop() {
        Window!.Run();
    }

    static void Closing() { }

    static void End() {
        ShutdownComplete = true;
    }

    static void Load() { }

    #endregion engine meta-state

    #region per-frame

    private static void Update(double elapsed) {
        LastFrameTime = (float)elapsed;
        TotalSeconds += LastFrameTime;
        TickTimer += LastFrameTime;

        Input.Update((float)elapsed);

        Log.Inane(TickTimer);
        while (TickTimer > TickTime) {
            Tick();
            TickTimer -= TickTime;
        }
    }

    public static void Tick() {
        if (CurrentScene == null && !WaitingToChangeScenes) {
            Window!.Close();
        }

        CurrentScene?.Tick();

        if (WaitingToChangeScenes) {
            CurrentScene?.Unload();
            CurrentScene = NextScene;
            CurrentScene?.Load();
            WaitingToChangeScenes = false;
        }
    }

    private static void Render(double elapsed) {
        // update ImGui

        // clear backbuffer
        Gl!.Clear((uint)ClearBufferMask.ColorBufferBit);

        // render the current frame
        CurrentScene?.Render();

        // draw all batched sprites

        // draw all batched fonts

        // draw all batched raw geometry

        // render ImGui
    }

    #endregion per-frame    

    public static void Begin() {
        Log.Info("loading config");
        if (Config.Location!.Exists()) {
            Config.Load();
        }
        else {
            Log.Info("writing default config");
            Config.Save();
        }

        Log.Info("starting game");
        try {
            Start();
        }
        finally {
            if (!ShutdownComplete) {
                End();
            }
        }
    }
}
