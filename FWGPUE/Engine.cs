using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using Silk.NET.OpenGL;
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

    public GL? Gl { get; protected set; }
    public IWindow? Window { get; protected set; }

    #region input
    public IInputContext? Input { get; protected set; }

    /// <summary> Whether keys are down (true) or up (false). </summary>
    public bool[]? KeyStates { get; protected set; }
    /// <summary> Number of frames keys have been down, or 0 if they are not currently down. </summary>
    public int[]? KeyFrames { get; protected set; }
    /// <summary> Time in seconds keys have been down, or 0 if they are not currently down. </summary>
    public float[]? KeyTimers { get; protected set; }

    /// <summary> Whether mouse buttons are down (true) or up (false). </summary>
    public bool[]? MouseStates { get; protected set; }
    /// <summary> Number of frames mouse buttons have been down, or 0 if they are not currently down. </summary>
    public int[]? MouseFrames { get; protected set; }
    /// <summary> Time in seconds mouse buttons have been down, or 0 if they are not currently down. </summary>
    public float[]? MouseTimers { get; protected set; }

    void UpdateKeyFrames() {
        for (int key = 0; key < (int)Enum.GetValues<Key>().Max(); key++) {
            if (KeyStates![key]) {
                KeyFrames![key]++;
            }
            else {
                KeyFrames![key] = 0;
            }
        }
    }
    void UpdateKeyTimers(float elapsed) {
        for (int key = 0; key < (int)Enum.GetValues<Key>().Max(); key++) {
            if (KeyStates![key]) {
                KeyTimers![key] += elapsed;
            }
            else {
                KeyTimers![key] = 0;
            }
        }
    }

    void UpdateMouseFrames() {
        for (int mouse = 0; mouse < (int)Enum.GetValues<MouseButton>().Max(); mouse++) {
            if (MouseStates![mouse]) {
                MouseFrames![mouse]++;
            }
            else {
                MouseFrames![mouse] = 0;
            }
        }
    }
    void UpdateMouseTimers(float elapsed) {
        for (int mouse = 0; mouse < (int)Enum.GetValues<MouseButton>().Max(); mouse++) {
            if (MouseStates![mouse]) {
                MouseTimers![mouse] += elapsed;
            }
            else {
                MouseTimers![mouse] = 0;
            }
        }
    }

    public bool KeyPressed(Key key, int framesSincePress = 1) {
        return KeyFrames![(int)key] == framesSincePress;
    }
    public bool KeyDown(Key key) {
        return KeyStates![(int)key];
    }
    public bool KeyUp(Key key) => !KeyDown(key);

    public bool MouseButtonPressed(MouseButton button, int framesSincePress = 1) {
        return MouseFrames![(int)button] == framesSincePress;
    }
    public bool MouseButtonDown(MouseButton button) {
        return MouseStates![(int)button];
    }
    public bool MouseButtonUp(MouseButton button) => !MouseButtonDown(button);

    public Vector2 MousePosition() {
        return Input!.Mice.First().Position;
    }
    public Vector2 WorldSpaceMousePosition() {
        // get inverse transformation from world to screen
        Matrix4x4 projection = Camera!.ProjectionMatrix(Config.Instance.ScreenWidth, Config.Instance.ScreenHeight);
        projection *= Camera!.ViewMatrix;
        Matrix4x4.Invert(projection, out projection);

        // create screen space mouse position
        Vector4 screenSpaceMouse = new(
            2.0f * ((MousePosition().X - 0) / (Config.Instance.ScreenWidth)) - 1.0f,
            1.0f - (2.0f * (MousePosition().Y / Config.Instance.ScreenHeight)),
            1, 1);
        // transform it back to world space
        Vector4 worldSpaceMouse = Vector4.Transform(screenSpaceMouse, projection);
        return new(worldSpaceMouse.X, worldSpaceMouse.Y);
    }

    public bool KeyPressedWithinTime(Key key, float secondsSincePress) {
        return KeyTimers![(int)key] <= secondsSincePress;
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int args) {
        Log.Inane($"{key} | {args} up");
        KeyStates![(int)key] = false;
    }
    private void OnKeyDown(IKeyboard keyboard, Key key, int args) {
        Log.Inane($"{key} | {args} down");
        KeyStates![(int)key] = true;
    }

    private void OnMouseUp(IMouse mouse, MouseButton button) {
        Log.Inane($"{button} up");
        MouseStates![(int)button] = false;
    }
    private void OnMouseDown(IMouse mouse, MouseButton button) {
        Log.Inane($"{button} down");
        MouseStates![(int)button] = true;
    }

    #endregion input

    public float TotalSeconds { get; protected set; } = 0;
    public float LastFrameTime { get; protected set; }
    public float TickTimer { get; protected set; }
    public float TickTime => 1f / Config.Instance.TickRate;

    public Scene CurrentScene { get; protected set; }
    public Scene NextScene { get; protected set; }
    public bool WaitingToChangeScenes { get; protected set; }

    public Camera? Camera { get; set; }
    public SpriteBatcher? SpriteBatcher { get; protected set; }
    public SpriteAtlasFile? SpriteAtlas { get; protected set; }

    #region text drawing

    public FontManager? FontManager { get; protected set; }
    public FontRenderer FontRenderer { get; protected set; }
    public FontSystem FontSystem { get; protected set; }

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

        Normal = MiddleLeft,
        Center = MiddleMiddle,
    }
    public record TextDrawData(string text, Vector2 location, FSColor colour, float size, Vector2 scale, float rotation, TextAlignment alignment);
    public HashSet<TextDrawData> TextThisFrame = new();

    public void DrawText(string text, Vector2 location, FSColor colour, float size = 10, TextAlignment alignment = TextAlignment.Normal) {
        TextThisFrame.Add(new(text, location, colour, size, new(1, 1), 0, alignment));
    }

    #endregion text drawing

    public bool ShutdownComplete { get; private set; } = false;

    #region initialization
    protected void Init() {
        InitWindow();
        InitInput();
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
        Window.Initialize();
    }

    protected void InitInput() {
        Input = Window!.CreateInput();
        for (int i = 0; i < Input.Keyboards.Count; i++) {
            Input.Keyboards[i].KeyDown += OnKeyDown;
            Input.Keyboards[i].KeyUp += OnKeyUp;
        }
        for (int i = 0; i < Input.Mice.Count; i++) {
            Input.Mice[i].MouseDown += OnMouseDown; ;
            Input.Mice[i].MouseUp += OnMouseUp;
        }

        int keyCount = (int)Enum.GetValues<Key>().Max();
        KeyStates = new bool[keyCount];
        KeyFrames = new int[keyCount];
        KeyTimers = new float[keyCount];

        int mouseCount = (int)Enum.GetValues<MouseButton>().Max();
        MouseStates = new bool[mouseCount];
        MouseFrames = new int[mouseCount];
        MouseTimers = new float[mouseCount];
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

        testSprite = new Sprite(SpriteAtlas!) {
            Texture = "hello",
            Transform = {
                Position = new(0, 0, 0),
                Scale = new(20)
            }
        };

        FontManager = new FontManager();
        FontFile fonts = new FontFile("assets/fonts/fonts.fwgm");
        FontManager.LoadFont(Gl, fonts, 20);

        FontSystem = new FontSystem(new() {
            FontResolutionFactor = 2,
            KernelWidth = 2,
            KernelHeight = 2
        });
        FontSystem.AddFont(FontManager.GetFontData(FontManager.DefaultFont));

        FontRenderer = new FontRenderer(Gl);
    }
    #endregion initialization

    public void ChangeToScene(Scene scene) {
        WaitingToChangeScenes = true;
        NextScene = scene;
    }

    protected void MainLoop() {
        Window!.Run();
    }

    protected void Closing() {
    }

    protected void End() {
        ShutdownComplete = true;
    }

    protected void Start() {
        Init();
        MainLoop();
        End();
    }

    private void Load() { }

    private void Update(double elapsed) {
        LastFrameTime = (float)elapsed;
        TotalSeconds += LastFrameTime;
        TickTimer += LastFrameTime;

        UpdateMouseFrames();
        UpdateMouseTimers((float)elapsed);
        UpdateKeyFrames();
        UpdateKeyTimers((float)elapsed);

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

    private void Render(double obj) {
        Gl!.Clear((uint)ClearBufferMask.ColorBufferBit);

        SpriteBatcher!.DrawAll(Gl, this);
        SpriteBatcher.Clear();

        var font = FontSystem.GetFont(64);

        FontRenderer.Begin(Camera!.ProjectionMatrix(Config.Instance.ScreenWidth, Config.Instance.ScreenHeight));

        foreach (TextDrawData textToDraw in TextThisFrame) {
            Vector2 origin = new();
            Vector2 size = font.MeasureString(textToDraw.text, textToDraw.scale);

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

        FontRenderer.End();
        TextThisFrame.Clear();
    }

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
