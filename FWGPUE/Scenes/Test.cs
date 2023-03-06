using FontStashSharp;
using FWGPUE.Nodes;
using System.Numerics;

namespace FWGPUE.Scenes;

class Test : Scene {
    Node2D MouseFollowNode;

    public override void Load() {
        Load<Test>();

        MouseFollowNode = Nodes.Root.AddChild(new SpriteNode() {
            Sprite = "otherSquare",
            Scale = 2
        });

        MouseFollowNode.AddChild(new SpriteNode() {
            Offset = new(100, 10),
            Sprite = "square",
            Scale = 3,
        }).AddChild(new SpriteNode() {
            Offset = new(50, 10),
            Sprite = "otherSquare"
        }).AddChild(new SpriteNode() { 
            Offset = new(60, 0),
            Sprite = "square"
        });
    }

    public override void Tick() {
        base.Tick();

        MouseFollowNode.Offset = Camera.ScreenToWorld(MousePosition());
        MouseFollowNode.Rotation = TotalTimeInScene / 10f;
        MouseFollowNode.Children.First().Rotation = TotalTimeInScene / 5f;

        if (KeyDown(Key.Up)) { Camera!.Position.ChangeBy(new(0, -50 * TickTime, 0)); }
        if (KeyDown(Key.Down)) { Camera!.Position.ChangeBy(new(0, 50 * TickTime, 0)); }
        if (KeyDown(Key.Left)) { Camera!.Position.ChangeBy(new(-50 * TickTime, 0, 0)); }
        if (KeyDown(Key.Right)) { Camera!.Position.ChangeBy(new(50 * TickTime, 0, 0)); }
    }

    public override void Render() {
        base.Render();

        DrawCircle(new Vector3(1, 1, 1), MousePosition(), 4);
        DrawCircle(new Vector3(1, 0, 0), new(100, 100), 10);

        Nodes.DrawDebugNodes();
        Nodes.DrawDebugConnections();
    }

    public override void Unload() { }
}