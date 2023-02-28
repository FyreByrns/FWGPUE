using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using FWGPUE.IO;
using System.Numerics;
using FontStashSharp;

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

    public static GL? Gl { get; private set; }
    public static IWindow? Window { get; private set; }

    public static ImGuiController? ImGuiController { get; private set; }

    public static OrthoCamera2D? Camera { get; set; }

    public static SpriteBatcher? SpriteBatcher { get; private set; }

    public static FontManager? FontManager { get; private set; }
    public static FontRenderer? FontRenderer { get; private set; }
    public static FontSystem? FontSystem { get; private set; }

    public static GeometryRenderer? GeometryRenderer { get; private set; }

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
        SpriteBatcher!.DrawSprite(new Sprite(CurrentScene!.Atlas!) {
            Texture = name,
            Transform = {
                Position = new Vector3(location, z),
                Scale = new Vector3(size, size, 1),
                Rotation = new Vector3(0, 0, rotation)
            }
        });
    }

    public record VertexDrawData(Vector3 colour, Vector3 position);
    public static List<VertexDrawData> VerticesThisFrame = new();

    public static void DrawTriangle(Vector3 colour, Vector2 a, Vector2 b, Vector2 c) {
        VerticesThisFrame.Add(new VertexDrawData(colour, new(a, 1)));
        VerticesThisFrame.Add(new VertexDrawData(colour, new(b, 1)));
        VerticesThisFrame.Add(new VertexDrawData(colour, new(c, 1)));
    }
    public static void DrawLine(Vector3 colour, Vector2 start, Vector2 end, float thickness = 0.5f) {
        float angleBetween = (float)Math.Atan2(start.Y - end.Y, start.X - end.X);
        float anglePlusHalf = angleBetween + TurnsToRadians(0.5f);

        float cos = (float)Math.Cos(anglePlusHalf);
        float sin = (float)Math.Sin(anglePlusHalf);

        thickness /= 2;
        Vector2 a = new(
            start.X + cos * thickness - sin * thickness,
            start.Y + sin * thickness + cos * thickness);
        Vector2 b = new(
            start.X + cos * thickness - sin * -thickness,
            start.Y + sin * thickness + cos * -thickness);
        Vector2 c = new(
            end.X + cos * thickness - sin * thickness,
            end.Y + sin * thickness + cos * thickness);
        Vector2 d = new(
            end.X + cos * thickness - sin * -thickness,
            end.Y + sin * thickness + cos * -thickness);

        DrawTriangle(colour, a, c, b);
        DrawTriangle(colour, b, c, d);
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

        Camera = new OrthoCamera2D(new Vector2(0), 100);

        SpriteBatcher = new();

        FontManager = new FontManager();
        FontFile fonts = new FontFile("assets/fonts/fonts.fwgm");
        FontManager.LoadFont(Gl, fonts, 20);

        FontSystem = new FontSystem(new() {
            FontResolutionFactor = 2,
            KernelWidth = 2,
            KernelHeight = 2
        });
        FontSystem.AddFont(FontManager.GetFontData(FontManager.DefaultFont!));

        FontRenderer = new FontRenderer();

        ImGuiController = new ImGuiController(Gl, Window, Input.InputContext);

        GeometryRenderer = new GeometryRenderer();
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
        ImGuiController!.Update((float)elapsed);

        // clear backbuffer
        Gl!.Clear((uint)ClearBufferMask.ColorBufferBit);

        // render the current frame
        CurrentScene?.Render();

        // draw all batched sprites
        SpriteBatcher!.DrawAll();
        SpriteBatcher.Clear();

        // draw all batched fonts
        float lastSize = 0;
        DynamicSpriteFont? font = null;
        foreach (TextDrawData textToDraw in TextThisFrame) {
            if (textToDraw.size != lastSize) {
                FontRenderer?.End();
                lastSize = textToDraw.size;
                font = FontSystem?.GetFont(textToDraw.size);
                FontRenderer?.Begin(Camera!.ViewMatrix * Camera.ProjectionMatrix);
            }

            Vector2 origin = new();
            Vector2 size = font!.MeasureString(textToDraw.text, textToDraw.scale);

            switch (textToDraw.alignment) {
                case TextAlignment.TopLeft: break;
                case TextAlignment.MiddleLeft: origin.Y += size.Y; break;
                case TextAlignment.BottomLeft: origin.Y += size.Y * 2; break;

                case TextAlignment.TopMiddle: origin.X += size.X; break;
                case TextAlignment.MiddleMiddle: origin += size; break;
                case TextAlignment.BottomMiddle: origin.X += size.X; origin.Y += size.Y * 2; break;

                case TextAlignment.TopRight: origin.X += size.X * 2; break;
                case TextAlignment.MiddleRight: origin.X += size.X * 2; origin.Y += size.Y; break;
                case TextAlignment.BottomRight: origin += size * 2; break;
            }

            font.DrawText(FontRenderer, textToDraw.text, textToDraw.location, textToDraw.colour, textToDraw.scale, TurnsToRadians(textToDraw.rotation), origin);
        }
        FontRenderer?.End();
        TextThisFrame.Clear();

        // draw all batched raw geometry
        foreach (VertexDrawData vertex in VerticesThisFrame) {
            GeometryRenderer?.PushVertex(vertex.colour, vertex.position);
        }
        GeometryRenderer?.FlushVertexBuffer();
        VerticesThisFrame.Clear();

        // render ImGui
        ImGuiController.Render();
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
