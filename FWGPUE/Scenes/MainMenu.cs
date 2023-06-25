namespace FWGPUE.Scenes;

using FWGPUE.IO;
using FWGPUE.UI;

class MainMenu : Scene {
    public override void Load() {
        Load<MainMenu>();

        UI.Add(new() {
            Middle = new(Config.ScreenWidth / 2, 40),
            Dimensions = new(Config.ScreenWidth / 3, 60),
            Text = "Play",
        });
    }

    public override void Tick() {
        base.Tick();
    }

    public override void Unload() { }
}
