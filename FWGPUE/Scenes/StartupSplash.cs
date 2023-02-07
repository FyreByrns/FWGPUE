using FWGPUE.IO;

namespace FWGPUE.Scenes;

using Colour = TextColour;
using static Engine.TextAlignment;
using static Silk.NET.Input.Key;

class StartupSplash : Scene {
    public float ShowTime { get; private set; }

    public override void Load(Engine context) {
        Load<StartupSplash>(context);

        ShowTime = GetGlobal<int>("ShowTime");
    }

    public override void Tick(Engine context) {
        base.Tick(context);

        context.DrawText("(space to skip)", new(0, 0), Colour.White, size: 15);
        context.DrawText("Made by: \n\tGavin White \n\tGaelan Edwards", new(Config.ScreenWidth / 2, Config.ScreenHeight / 2), Colour.White, size: 64, alignment: Center);

        // if total time in scene is greater than the amount of time the scene should be shown .. 
        if (Input.KeyPressed(Space) || TotalTimeInScene > ShowTime) {
            // .. swap to the next scene
            context.ChangeToScene(new MainMenu());
        }
    }

    public override void Unload(Engine context) {
    }
}
