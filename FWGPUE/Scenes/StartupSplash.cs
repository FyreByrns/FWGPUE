using FWGPUE.IO;

namespace FWGPUE.Scenes;

using Colour = TextColour;
using static Engine.TextAlignment;
using static Silk.NET.Input.Key;

class StartupSplash : Scene {
    public float ShowTime { get; private set; }

    public override void Load() {
        Load<StartupSplash>();

        ShowTime = GetGlobal<int>("ShowTime");
    }

    public override void Tick() {
        base.Tick();

        DrawImage("splashart", new(Config.ScreenWidth / 2, Config.ScreenHeight / 2), size:0.2f);

        DrawText("(space to skip)", new(0, 0), Colour.Black, size: 15);
        DrawText("Made by: \n\tGavin White \n\tGaelan Edwards \n\tAku Ichigoo", new(Config.ScreenWidth / 2, Config.ScreenHeight / 2), Colour.Black, size: 64, alignment: Center);

        // if total time in scene is greater than the amount of time the scene should be shown .. 
        if (KeyPressed(Space) || TotalTimeInScene > ShowTime) {
            // .. swap to the next scene
            ChangeToScene(new MainMenu());
        }
    }

    public override void Unload() { }
}
