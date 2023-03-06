using FWGPUE.IO;
using FWGPUE.Nodes;

namespace FWGPUE.Scenes;

using Colour = TextColour;
using static Engine.TextAlignment;
using static Silk.NET.Input.Key;

class StartupSplash : Scene {
    public float ShowTime { get; private set; }
    
    Node2D splashArtNode;

    public override void Load() {
        Load<StartupSplash>();

        ShowTime = GetGlobal<int>("ShowTime");

        splashArtNode = new SpriteNode() {
            Sprite = "splashart",
            TargetWidthPercentage = 1
        };
        Nodes.AddChild(splashArtNode, NodeFilters.ChildOf(Nodes.Root));
    }

    public override void Tick() {
        base.Tick();

        splashArtNode.Offset = (System.Numerics.Vector2)(Window!.Size / 2);

        // if total time in scene is greater than the amount of time the scene should be shown .. 
        if (KeyPressed(Space) || TotalTimeInScene > ShowTime) {
            // .. swap to the next scene
            ChangeToScene(new MainMenu());
        }
    }

    public override void Render() {
        base.Render();

        DrawText("(space to skip)", new(0, 0), Colour.Black, size: 15);
        DrawText("Made by: \n\tGavin White \n\tGaelan Edwards \n\tAku Ichigoo", new(Config.ScreenWidth / 2, Config.ScreenHeight / 2), Colour.Black, size: 64, alignment: Center);
    }

    public override void Unload() { }
}
