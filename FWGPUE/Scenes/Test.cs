using System.Reflection;
using System.Numerics;

namespace FWGPUE.Scenes;

class UIElement {
    public Vector2 Position;
    public Vector2 Dimensions;
    public Vector2 BottomRight => Position + Dimensions;

    public List<UIElement> Children = new();
}

class Test : Scene {
    List<UIElement> UI = new();
    UIElement? Hovered = null;

    public override void Load() {
        Load<Test>();

        Renderer.OnRenderObjectsRequired += OnRender;
        MouseMove += OnMouseMove;

        for (int i = 0; i < 10; i++) {
            UI.Add(new() {
                Position = new(5 + 30 * i + 5 * i, 5),
                Dimensions = new(30, 30)
            });
        }
    }

    public override void Tick() {
        base.Tick();
    }
    private void OnMouseMove(Vector2 oldMouse, Vector2 newMouse) {
        // update UI
        Hovered = null;
        foreach (var element in UI) {
            if (newMouse.X >= element.Position.X && newMouse.Y >= element.Position.Y && newMouse.X < element.BottomRight.X && newMouse.Y < element.BottomRight.Y) {
                Hovered = element;
            }
        }
    }

    private void OnRender(double elapsed) {
        Vector3 col = new(0.8f, 0.9f, 0.99f);

        Renderer.PushString(new(100, 100, 0), "AaBbCcDdEeFfGgHhIiJjKkLl\nMmNnOoPpQqRrSsTtUuVvWwXx\nYyZz");

        // draw UI
        foreach (var element in UI) {
            if (element == Hovered) {
                Renderer.PushRect(element.Position, element.Position + element.Dimensions, 2, col, 3, new(0.4f, 0.2f, 0.9f), true);
            }
            else {
                Renderer.PushRect(element.Position, element.Position + element.Dimensions, 2, col, filled: true);
            }
        }
    }

    public override void Unload() { }
}