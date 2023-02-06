using IM = ImGuiNET.ImGui;

namespace FWGPUE.Scenes;

class MainMenu : Scene {
    public override void Load(Engine context) { }
    public override void Unload(Engine context) { }

    public override void Tick(Engine context) {
        base.Tick(context);
    }

    public override void Render(Engine context) {
        IM.Button("testing");
    }
}