namespace FWGPUE.Scenes;

class Test : Scene {
    Node2D baseNode = new();

    public override void Load(Engine context) {
        baseNode.AddChild(new Node2D() {
            Offset = new(100, 10)
        }).AddChild(new Node2D() {
            Offset = new(10, 10)
        });

    }

    public override void Tick(Engine context) {
        base.Tick(context);

        baseNode.Offset = Input.MousePosition();
        baseNode.Rotation = TotalTimeInScene / 10f;

        foreach (Node2D n in baseNode.AllNodes()) {
            context.DrawTextRotated($"#", n.RelativeOffset(), n.RelativeRotation(), TextColour.AliceBlue, size: 30, alignment: TextAlignment.Center);
        }
    }

    public override void Render(Engine context) {
        base.Render(context);
    }

    public override void Unload(Engine context) {
    }
}