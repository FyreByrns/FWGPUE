namespace FWGPUE.Scenes;

class StartupSplash : Scene {
    public float ShowTime { get; private set; }

    public override void Load(Engine context) {
        Load<StartupSplash>(context);

        ShowTime = GetGlobal<int>("ShowTime");
    }

    public override void Tick(Engine context) {
        base.Tick(context);

        // if total time in scene is greater than the amount of time the scene should be shown .. 
        if (TotalTimeInScene > ShowTime) {
            // .. swap to the next scene
            context.ChangeToScene(new MainMenu());
        }
    }

    public override void Unload(Engine context) {
    }
}
