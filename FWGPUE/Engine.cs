using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using FWGPUE.IO;
using System.Numerics;

using FWGPUE.Graphics;
using Silk.NET.OpenAL;

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

    public bool KeyPressed(Key key, int framesSincePress = 1) {
        return KeyFrames![(int)key] == 1;
    }
    public bool KeyDown(Key key) {
        return KeyStates![(int)key];
    }
    public bool KeyUp(Key key) => !KeyDown(key);

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

    #endregion input

    public float TotalSeconds { get; protected set; } = 0;
    public float LastFrameTime { get; protected set; }
    public float TickTimer { get; protected set; }
    public float TickTime => 1f / Config.TickRate;

    public Camera? Camera { get; set; }
    public SpriteBatcher? SpriteBatcher { get; protected set; }

    public bool ShutdownComplete { get; private set; } = false;

    public Config Config { get; }

    #region initialization
    protected void InitWindow() {
        WindowOptions options = WindowOptions.Default with {
            Size = new Vector2D<int>(Config.ScreenWidth, Config.ScreenHeight),
            Title = "FWGPUE",
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

        int keyCount = (int)Enum.GetValues<Key>().Max();
        KeyStates = new bool[keyCount];
        KeyFrames = new int[keyCount];
        KeyTimers = new float[keyCount];
    }

    protected void InitGraphics() {
        Gl = GL.GetApi(Window);

        Gl.Viewport(0, 0, (uint)Config.ScreenWidth, (uint)Config.ScreenHeight);

        Camera = new Camera(new Vector2(0), 100);
        SpriteBatcher = new SpriteBatcher(Gl);
    }
    #endregion initialization

    protected void MainLoop() {
        Window!.Run();
    }

    protected void Closing() {
    }

    protected void End() {
        ShutdownComplete = true;
    }

    protected void Start() {
        InitWindow();
        InitInput();
        InitGraphics();
        MainLoop();
        End();
    }

    private unsafe void Load() {
    }

    private void Update(double elapsed) {
        LastFrameTime = (float)elapsed;
        TotalSeconds += LastFrameTime;
        TickTimer += LastFrameTime;

        UpdateKeyFrames();
        UpdateKeyTimers((float)elapsed);

        Log.Inane(TickTimer);
        while (TickTimer > TickTime) {
            Tick();
            TickTimer -= TickTime;
        }
    }

    public virtual void Tick() { }

    private void Render(double obj) {
        Gl!.Clear((uint)ClearBufferMask.ColorBufferBit);

        SpriteBatcher!.DrawAll(Gl, this);
        SpriteBatcher!.Clear();
    }

    public Engine() {
        Log.Info("loading config");
        Config = new Config();
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
