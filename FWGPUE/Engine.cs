using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using FWGPUE.IO;
using System.Numerics;

using FWGPUE.Graphics;
using Shader = FWGPUE.Graphics.Shader;

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
    public SpriteBatcher SpriteBatcher { get; protected set; }

    public bool ShutdownComplete { get; private set; } = false;

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

        Gl.Viewport(0, 0, (uint)Config.ScreenWidth, (uint)Config.ScreenHeight);

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
            Log.Inane("ticked");
            Tick();
            TickTimer -= TickTime;
        }
    }

    Sprite testSprite = new Sprite() {
        Transform = new Transform() {
            Scale = new Vector3(10f),
            Position = new Vector3(0, 0, 1),
        }
    };
    public virtual void Tick() {
        if (KeyDown(Key.Left)) {
            CameraPos.ChangeBy(new Vector3(-10 * TickTime, 0, 0));
            CameraTarget.ChangeBy(new Vector3(-10 * TickTime, 0, 0));
        }

        testSprite.Transform.Rotation = new Vector3(0, 0, TotalSeconds);
        SpriteBatcher.DrawSprite(testSprite);
    }
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

class SpriteBatcher {
    float[] quadVertices = {
        // positions  
        -0.5f,  0.5f, 1.0f,
        -0.5f, -0.5f, 1.0f,
         0.5f, -0.5f, 1.0f,

        -0.5f,  0.5f, 1.0f,
         0.5f, -0.5f, 1.0f,
         0.5f,  0.5f, 1.0f,
    };
    uint quadVAO;
    uint quadVBO;
    uint offsetVBO;

    public List<Sprite> SpritesThisFrame { get; } = new();
    public HashSet<short> RegisteredSpriteIDs { get; } = new(); // to track which sprites are already being drawn

    public Shader Shader { get; set; }

    /// <summary>
    /// Register sprite for drawing this frame.
    /// </summary>
    public void DrawSprite(Sprite sprite) {
        if (!RegisteredSpriteIDs.Contains(sprite.ID)) {
            SpritesThisFrame.Add(sprite);
            RegisteredSpriteIDs.Add(sprite.ID);
        }
    }

    public void DrawAll(GL gl, Engine context) {
        // get offsets
        Matrix4x4[] offsets = new Matrix4x4[SpritesThisFrame.Count];
        for (int i = 0; i < offsets.Length; i++) {
            Sprite sprite = SpritesThisFrame[i];
            var model =
                Matrix4x4.CreateRotationZ(Engine.TurnsToRadians(sprite.Transform.Rotation.Z)) *
                Matrix4x4.CreateRotationY(Engine.TurnsToRadians(sprite.Transform.Rotation.Y)) *
                Matrix4x4.CreateRotationX(Engine.TurnsToRadians(sprite.Transform.Rotation.X)) *
                Matrix4x4.CreateScale(sprite.Transform.Scale.X, sprite.Transform.Scale.Y, 1) *
                Matrix4x4.CreateTranslation(sprite.Transform.Position);

            offsets[i] = model;
        }
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, offsetVBO);
        unsafe {
            fixed (Matrix4x4* data = offsets) {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(sizeof(Matrix4x4) * offsets.Length), data, BufferUsageARB.StaticDraw);
            }
        }

        var view = Matrix4x4.CreateLookAt(context.CameraPos, context.CameraTarget, context.CameraUp);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(Engine.DegreesToRadians(45.0f), (float)context.Config.ScreenWidth / context.Config.ScreenHeight, 0.1f, 200.0f);

        Shader.Use();
        Shader.SetUniform("uView", view);
        Shader.SetUniform("uProjection", projection);

        gl.BindVertexArray(quadVAO);
        gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, (uint)SpritesThisFrame.Count);

        SpritesThisFrame.Clear();
        RegisteredSpriteIDs.Clear();
    }

    public void Clear() {
        SpritesThisFrame.Clear();
    }

    public SpriteBatcher(GL gl) {
        Shader = new Shader(gl, new ShaderFile("assets/sprite.shader"));

        quadVAO = gl.GenVertexArray();
        quadVBO = gl.GenBuffer();
        offsetVBO = gl.GenBuffer();

        gl.BindBuffer(GLEnum.ArrayBuffer, quadVBO);
        unsafe {
            fixed (void* data = quadVertices) {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(quadVertices.Length * sizeof(float)), data, BufferUsageARB.StaticDraw);
            }
        }

        gl.BindVertexArray(quadVAO);
        unsafe {
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, (void*)0);
        }

        // setup offset buffer
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, offsetVBO);
        gl.EnableVertexAttribArray(1);
        gl.EnableVertexAttribArray(2);
        gl.EnableVertexAttribArray(3);
        gl.EnableVertexAttribArray(4);
        unsafe {
            gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)(sizeof(Matrix4x4)), (void*)(sizeof(float) * 4 * 0));
            gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, (uint)(sizeof(Matrix4x4)), (void*)(sizeof(float) * 4 * 1));
            gl.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, (uint)(sizeof(Matrix4x4)), (void*)(sizeof(float) * 4 * 2));
            gl.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, (uint)(sizeof(Matrix4x4)), (void*)(sizeof(float) * 4 * 3));
        }
        gl.VertexAttribDivisor(1, 1);
        gl.VertexAttribDivisor(2, 1);
        gl.VertexAttribDivisor(3, 1);
        gl.VertexAttribDivisor(4, 1);
    }
}
