using FontStashSharp;
using FWGPUE.IO;

namespace FWGPUE.Scenes;

abstract class Scene {
    public float TotalTimeInScene { get; protected set; }

    public DataMarkupFile? Globals { get; protected set; }
    public T? GetGlobal<T>(string globalName) {
        if (Globals!.TryGetToken(globalName, out var token)) {
            var t = typeof(T); // for smaller ifs

            try {
                object? result = null;

                if (t == typeof(int)) {
                    if (int.TryParse(token?.Contents.Value, out int i)) {
                        result = i;
                    }
                }
                if (t == typeof(float)) {
                    if (float.TryParse(token?.Contents.Value, out float f)) {
                        result = f;
                    }
                }
                if (t == typeof(string)) {
                    result = token?.Contents?.Value;
                }

                return (T)result!;
            }
            catch (Exception e) {
                Log.Error($"error reading global {globalName}: {e}");
            }
        }

        Log.Error($"global {globalName} not found");
        return default(T);
    }

    protected void Load<T>(Engine context)
        where T : Scene {

        // get scene directory from scene type name
        string sceneNameLower = typeof(T).Name.ToLower();
        string sceneDirectory = $"assets/scenes/{sceneNameLower}";
        string sceneGlobals = $"{sceneDirectory}/globals.fwgm";

        Globals = new DataMarkupFile(sceneGlobals);
        Globals.Location!.EnsureExists();
        Globals.Load();
    }
    public abstract void Load(Engine context);
    public abstract void Unload(Engine context);

    public virtual void Tick(Engine context) {
        TotalTimeInScene += TickTime;

        context.DrawText($"{GetType().Name} // {TotalTimeInScene:#.##}", new(0, Config.ScreenHeight * 0.98f), FSColor.Tan);
    }

    public virtual void Render(Engine context) { }
}
