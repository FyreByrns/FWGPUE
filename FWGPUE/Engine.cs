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
    #region static helper methods
    public static float DegreesToRadians(float degrees) {
        return MathF.PI / 180f * degrees;
    }
    public static float TurnsToRadians(float turns) {
        return turns * MathF.PI * 2f;
    }
    #endregion static helper methods

    #region timing

    public float TotalSeconds { get; protected set; } = 0;
    public float LastFrameTime { get; protected set; }
    public float TickTimer { get; protected set; }
    public static float TickTime => 1f / Config.Instance.TickRate;

    #endregion timing

    #region scene management

    public Scene? CurrentScene { get; protected set; }
    public Scene? NextScene { get; protected set; }
    public bool WaitingToChangeScenes { get; protected set; }

    public void ChangeToScene(Scene scene) {
        WaitingToChangeScenes = true;
        NextScene = scene;
    }

    #endregion scene management

    #region rendering

    public GL? Gl { get; protected set; }
    public IWindow? Window { get; protected set; }

    public ImGuiController ImGuiController { get; protected set; }

    public Camera? Camera { get; set; }

    #region sprites

    public SpriteBatcher? SpriteBatcher { get; protected set; }
    public SpriteAtlasFile? SpriteAtlas { get; protected set; }

    #endregion sprites

    #region text

    public FontManager? FontManager { get; protected set; }
    public FontRenderer? FontRenderer { get; protected set; }
    public FontSystem? FontSystem { get; protected set; }

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
    public record TextDrawData(string text, Vector2 location, FSColor colour, float size, Vector2 scale, float rotation, TextAlignment alignment);
    public HashSet<TextDrawData> TextThisFrame = new();

    public void DrawText(string text, Vector2 location, FSColor colour, float size = 10, TextAlignment alignment = TextAlignment.Normal) {
        TextThisFrame.Add(new(text, location, colour, size, new(1, 1), 0, alignment));
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
        Input.Init(this);
        InitGraphics();

        ChangeToScene(new StartupSplash());
    }

    protected void InitWindow() {
        WindowOptions options = WindowOptions.Default with {
            Size = new Vector2D<int>(Config.Instance.ScreenWidth, Config.Instance.ScreenHeight),
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

        Gl.Viewport(0, 0, (uint)Config.Instance.ScreenWidth, (uint)Config.Instance.ScreenHeight);

        Gl.Enable(GLEnum.Multisample);

        Gl.Enable(GLEnum.Blend);
        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Camera = new Camera(new Vector2(0), 100);

        SpriteBatcher = new SpriteBatcher(Gl);
        SpriteAtlas = new("assets/atlases/main.fwgm");
        SpriteAtlas.Load();
        SpriteAtlas.LoadTexture(Gl);

        FontManager = new FontManager();
        FontFile fonts = new FontFile("assets/fonts/fonts.fwgm");
        FontManager.LoadFont(Gl, fonts, 20);

        FontSystem = new FontSystem(new() {
            FontResolutionFactor = 2,
            KernelWidth = 2,
            KernelHeight = 2
        });
        FontSystem.AddFont(FontManager.GetFontData(FontManager.DefaultFont!));

        FontRenderer = new FontRenderer(Gl);

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

    private void Update(double elapsed) {
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

    public virtual void Tick() {
        if (CurrentScene == null && !WaitingToChangeScenes) {
            Window!.Close();
        }

        CurrentScene?.Tick(this);

        if (WaitingToChangeScenes) {
            CurrentScene?.Unload(this);
            CurrentScene = NextScene;
            CurrentScene?.Load(this);
            WaitingToChangeScenes = false;
        }
    }

    private void Render(double elapsed) {
        ImGuiController.Update((float)elapsed);

        Gl!.Clear((uint)ClearBufferMask.ColorBufferBit);

        SpriteBatcher!.DrawAll(Gl, this);
        SpriteBatcher.Clear();

        float lastSize = 0;
        DynamicSpriteFont? font = null;
        foreach (TextDrawData textToDraw in TextThisFrame) {
            if (textToDraw.size != lastSize) {
                FontRenderer?.End();
                lastSize = textToDraw.size;
                font = FontSystem?.GetFont(textToDraw.size);
                FontRenderer?.Begin(Camera!.ProjectionMatrix(Config.Instance.ScreenWidth, Config.Instance.ScreenHeight));
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

        ImGuiNET.ImGui.ShowDemoWindow();
        ImGuiController.Render();
    }

    #endregion per-frame

    public Engine() {
        Log.Info("loading config");
        if (Config.Instance.Location!.Exists()) {
            Config.Instance.Load();
        }
        else {
            Log.Info("writing default config");
            Config.Instance.Save();
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
