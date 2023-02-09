using System.Numerics;

namespace FWGPUE.Scenes;

class Test : Scene {
    Node2D baseNode = new();

    public override void Load() {
        Load<Test>();

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
        baseNode.Children.First().Rotation = TotalTimeInScene / 5f;

        SpriteBatcher!.DrawSprite(new Graphics.Sprite(Atlas!) {
            Texture = "square",
            Transform = {
                Scale = new(10, 10, 1),
                Position = new(100, 100, 1)
            }
        });

        SpriteBatcher!.DrawSprite(new Graphics.Sprite(Atlas!) {
            Texture = "otherSquare",
            Transform = {
                Scale = new(10, 10, 1),
                Position = new(100, 100, 0)
            }
        });

        foreach (Node2D n in baseNode.AllNodes()) {
            DrawTextRotated($"#", n.RelativeOffset(), n.RelativeRotation(), TextColour.AliceBlue, size: 30, alignment: TextAlignment.Center);
        }
    }

    public override void Render() {
        base.Render();

        SpriteBatcher.DrawAll();
        SpriteBatcher.Clear();
    }

    public override void Unload() { }
}