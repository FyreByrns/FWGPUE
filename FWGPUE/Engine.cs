using System.Runtime.InteropServices;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using FWGPUE.IO;
using System.Runtime.CompilerServices;
using System.Numerics;

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

    public GL? Gl { get; protected set; }

    public bool ShutdownComplete { get; private set; } = false;

    RawObject? TestObject;

    public Config Config { get; }

    public Vector3 CameraPos = new Vector3(0, 0, 100);
    public Vector3 CameraTarget = Vector3.Zero;
    public Vector3 CameraDirection => Vector3.Normalize(CameraPos - CameraTarget);
    public Vector3 CameraRight => Vector3.Normalize(Vector3.Cross(Vector3.UnitY, CameraDirection));
    public Vector3 CameraUp => Vector3.Cross(CameraDirection, CameraRight);

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

        TestObject = new RawObject(Gl) {
            Transform = new() {
                Position = new Vector3(0.5f, 0.5f, 0),
                Rotation = new Vector3(0, 0, TurnsToRadians(0.5f)),
                Scale = 10f,
            }
        };

        Gl.Viewport(0, 0, (uint)Config.ScreenWidth, (uint)Config.ScreenHeight);
    }
    #endregion initialization

    protected void MainLoop() {
        Window!.Run();
    }

    protected void Closing() {
        TestObject!.Dispose();
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
            Log.Inane("ticked");
            Tick();
            TickTimer -= TickTime;
        }
    }
    public virtual void Tick() {
        TestObject!.Transform.Rotation.ChangeBy(new Vector3(0, 0, 0.1f * (float)TickTime));

        if (KeyStates![(int)Key.Left]) { TestObject!.Transform!.Position.ChangeBy(new Vector3(-10 * TickTime, 0, 0)); }
        if (KeyStates![(int)Key.Right]) { TestObject!.Transform!.Position.ChangeBy(new Vector3(10 * TickTime, 0, 0)); }
        if (KeyStates![(int)Key.Up]) { TestObject!.Transform!.Position.ChangeBy(new Vector3(0, 10 * TickTime, 0)); }
        if (KeyStates![(int)Key.Down]) { TestObject!.Transform!.Position.ChangeBy(new Vector3(0, -10 * TickTime, 0)); }
        if (KeyStates![(int)Key.Q]) { TestObject!.Transform!.Position.ChangeBy(new Vector3(0, 0, 10 * TickTime)); }
        if (KeyStates![(int)Key.E]) { TestObject!.Transform!.Position.ChangeBy(new Vector3(0, 0, -10 * TickTime)); }
    }
    private void Render(double obj) {
        Gl!.Clear((uint)ClearBufferMask.ColorBufferBit);
        TestObject!.Draw(Gl!, this);
    }

    public Engine() {
        Log.Info("loading config");
        Config = new Config();
        if (Config.Location!.Exists()) {
            Config.Load();
        }
        else {
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
