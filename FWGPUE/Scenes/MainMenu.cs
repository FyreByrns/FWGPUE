using IM = ImGuiNET.ImGui;

namespace FWGPUE.Scenes;

using FWGPUE.IO;
using static ImGuiNET.ImGuiWindowFlags;

class MainMenu : Scene {
    public override void Load(Engine context) { }
    public override void Unload(Engine context) { }

    public override void Tick(Engine context) {
        base.Tick(context);
    }

    public override void Render(Engine context) {
        IM.SetNextWindowSize(new(Config.Instance.ScreenWidth, Config.Instance.ScreenHeight));
        IM.Begin("mainMenuContainer", NoTitleBar | NoBackground | NoResize);

        IM.SetCursorPos(new(Config.Instance.ScreenWidth / 2 - 100, Config.Instance.ScreenHeight / 4));
        IM.Button("play", new(100, 20));

        IM.SetCursorPos(new(Config.Instance.ScreenWidth / 2 - 100, Config.Instance.ScreenHeight / 3));
        if (IM.Button("quit", new(100, 20))) {
            // close
            context.ChangeToScene(null);
        }

        IM.End();
    }
}