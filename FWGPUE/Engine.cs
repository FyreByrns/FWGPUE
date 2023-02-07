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

class Engine {
    #region timing

    public static float TotalSeconds { get; protected set; } = 0;
    public static float LastFrameTime { get; protected set; }
    public static float TickTimer { get; protected set; }
    public static float TickTime => 1f / Config.TickRate;

    #endregion timing

    #region scene management

    public static Scene? CurrentScene { get; protected set; }
    public static Scene? NextScene { get; protected set; }
    public static bool WaitingToChangeScenes { get; protected set; }

    public static void ChangeToScene(Scene? scene) {
        WaitingToChangeScenes = true;
        NextScene = scene;
    }

    #endregion scene management

    #region rendering

    public static GL? Gl { get; protected set; }
    public static IWindow? Window { get; protected set; }

    public static ImGuiController? ImGuiController { get; protected set; }

    public static Camera? Camera { get; set; }

    #region sprites

    public static SpriteBatcher? SpriteBatcher { get; protected set; }
    public static SpriteAtlasFile? SpriteAtlas { get; protected set; }

    #endregion sprites

    #region text

    public static FontManager? FontManager { get; protected set; }
    public static FontRenderer? FontRenderer { get; protected set; }
    public static FontSystem? FontSystem { get; protected set; }

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

    #endregion text 

    #endregion rendering

    #region initialization

    protected void Start() {
        Init();
        MainLoop();
        End();
    }

    protected void Init() {
        InitWindow();
        Input.Init();
        InitGraphics();

        ChangeToScene(new StartupSplash());
    }

    protected void InitWindow() {
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

    protected void InitGraphics() {
        Gl = GL.GetApi(Window);
        Gl.Enable(GLEnum.Multisample);
        Gl.Enable(GLEnum.Blend);
        Gl.Viewport(0, 0, (uint)Config.ScreenWidth, (uint)Config.ScreenHeight);
        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Camera = new Camera(new Vector2(0), 100);

        SpriteBatcher = new();
        SpriteAtlas = new("assets/atlases/main.fwgm");
        SpriteAtlas.Load();
        SpriteAtlas.LoadTexture();

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
    }

    #endregion initialization

    #region engine meta-state

    public bool ShutdownComplete { get; private set; } = false;

    protected void MainLoop() {
        Window!.Run();
    }

    protected void Closing() { }

    protected void End() {
        ShutdownComplete = true;
    }

    private void Load() { }

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
        ImGuiController!.Update((float)elapsed);

        Gl!.Clear((uint)ClearBufferMask.ColorBufferBit);

        SpriteBatcher!.DrawAll();
        SpriteBatcher.Clear();

        float lastSize = 0;
        DynamicSpriteFont? font = null;
        foreach (TextDrawData textToDraw in TextThisFrame) {
            if (textToDraw.size != lastSize) {
                FontRenderer?.End();
                lastSize = textToDraw.size;
                font = FontSystem?.GetFont(textToDraw.size);
                FontRenderer?.Begin(Camera!.ProjectionMatrix(Config.ScreenWidth, Config.ScreenHeight));
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

        CurrentScene?.Render();

        ImGuiController.Render();
    }

    #endregion per-frame

    public Engine() {
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
