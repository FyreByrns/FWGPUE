using FWGPUE.IO;
using System.Numerics;

using FWGPUE.Graphics;
using FWGPUE.Scenes;
using System.Runtime.InteropServices;

namespace FWGPUE;

static class Engine {
    public static bool ShutdownComplete { get; private set; } = false;

    public static float TotalSeconds { get; private set; } = 0;
    public static float LastFrameTime { get; private set; }
    public static float TickTimer { get; private set; }
    public static float TickTime => 1f / Config.TickRate;

    public static Scene? CurrentScene { get; private set; }
    public static Scene? NextScene { get; private set; }
    public static bool WaitingToChangeScenes { get; private set; }

    public static OrthoCamera2D Camera { get; set; } = new(new(0, 0), 10);
    public static RenderManager Renderer { get; private set; }

    public static void ChangeToScene(Scene? scene) {
        WaitingToChangeScenes = true;
        NextScene = scene;
    }

    static void Start() {
        Init();
        MainLoop();
        End();
    }

    static void Init() {
        Renderer = new();
        Renderer.OnFrame += Update;

        Renderer.OnLoad += Load;
        Renderer.OnClose += Closing;

        Renderer.Setup();
        Renderer.Begin();
    }

    static void Load() {
        Input.Init();

        ChangeToScene(new Test());
    }

    static void MainLoop() {
    }

    static void Closing() { }

    static void End() {
        ShutdownComplete = true;
    }

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
            Renderer.Exit();
        }

        CurrentScene?.Tick();

        if (WaitingToChangeScenes) {
            CurrentScene?.Unload();
            CurrentScene = NextScene;
            CurrentScene?.Load();
            WaitingToChangeScenes = false;
        }
    }

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
