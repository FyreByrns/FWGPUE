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

        for (int x = 0; x < 10; x++) {
            for (int y = 0; y < 10; y++) {
                for (int i = 0; i < 10; i++) {
                    Nodes.AddChild(new ParallaxSpriteNode() {
                        Offset = new(x * 50, y * 50),
                        Scale = 2,
                        //TargetWidthPercentage = 0.06f,
                        Z = i,
                        Sprite = "square"
                    }, NodeFilters.ChildOf(Nodes.Root));
                }
            }
        }
    }

    public override void Tick() {
        base.Tick();

        MouseFollowNode.Rotation = TotalTimeInScene / 10f;
        MouseFollowNode.Children.First().Rotation = TotalTimeInScene / 5f;

    }

    public override void Render() {
        base.Render();

        DrawCircle(new Vector3(1, 1, 1), MousePosition(), 4);
        DrawCircle(new Vector3(1, 0, 0), new(100, 100), 10);

        //Nodes.DrawDebugNodes();
        Nodes.DrawDebugConnections();
    }

    public override void Unload() { }
}