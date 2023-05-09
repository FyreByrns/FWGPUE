using FWGPUE.IO;
using FWGPUE.Nodes;

namespace FWGPUE.Scenes;

using static Silk.NET.Input.Key;

class StartupSplash : Scene {
    public float ShowTime { get; private set; }
    
    Node2D splashArtNode;

    public override void Load() {
        Load<StartupSplash>();

        ShowTime = GetGlobal<int>("ShowTime");
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
    }

    public override void Unload() { }
}
