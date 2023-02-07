﻿using IM = ImGuiNET.ImGui;

namespace FWGPUE.Scenes;

using FWGPUE.IO;
using static ImGuiNET.ImGuiWindowFlags;

class MainMenu : Scene {
    public override void Load(Engine context) { }

    public override void Tick(Engine context) {
        base.Tick(context);
    }

    public override void Render(Engine context) {
        IM.SetNextWindowSize(new(Config.ScreenWidth, Config.ScreenHeight));
        IM.Begin("mainMenuContainer", NoTitleBar | NoBackground | NoResize);

        IM.SetCursorPos(new(Config.ScreenWidth / 2 - 100, Config.ScreenHeight / 4));
        if (IM.Button("play", new(100, 20))) {
            context.ChangeToScene(new Test());
        }

        IM.SetCursorPos(new(Config.ScreenWidth / 2 - 100, Config.ScreenHeight / 3));
        if (IM.Button("quit", new(100, 20))) {
            // close
            context.ChangeToScene(null);
        }

        IM.End();
    }

    public override void Unload(Engine context) { }
}
