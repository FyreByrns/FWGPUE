using System.Reflection;
using System.Numerics;
using FWGPUE.UI;

namespace FWGPUE.Scenes;

class Test : Scene {
    public override void Load() {
        Load<Test>();

        Renderer.OnRenderObjectsRequired += OnRender;
        MouseMove += OnMouseMove;
    }

    public override void Tick() {
        base.Tick();
    }

    void OnRender(double elapsed) {
    }

    void OnMouseMove(Vector2 oldMouse, Vector2 newMouse) {
    }

    public override void Unload() { }
}