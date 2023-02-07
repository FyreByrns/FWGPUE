using System.Numerics;

namespace FWGPUE.Scenes;

class Test : Scene {
    Node2D baseNode = new();

    public override void Load() {
        baseNode.AddChild(new Node2D() {
            Offset = new(100, 10)
        }).AddChild(new Node2D() {
            Offset = new(10, 10)
        });

    }

    public override void Tick() {
        base.Tick();

        baseNode.Offset = MousePosition();
        baseNode.Rotation = TotalTimeInScene / 10f;

        foreach (Node2D n in baseNode.AllNodes()) {
            DrawTextRotated($"#", n.RelativeOffset(), n.RelativeRotation(), TextColour.AliceBlue, size: 30, alignment: TextAlignment.Center);
        }
    }

    public override void Render() {
        base.Render();
    }

    public override void Unload() { }
}