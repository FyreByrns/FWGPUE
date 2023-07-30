using System.Reflection;
using System.Numerics;
using FWGPUE.UI;
using FWGPUE.Nodes;
using FWGPUE.Graphics;
using FWGPUE.Gameplay;
using FWGPUE.Gameplay.Controllers;

namespace FWGPUE.Scenes;

class Test : Scene {
    EntityNode player = new();

    public override void Load() {
        Load<Test>();

        Renderer.OnRenderObjectsRequired += OnRender;
        MouseMove += OnMouseMove;
        //InputEventFired += InputEvent;

        player.Controller = new PlayerController(player);
        player.Speed = 0.5f;
        player.LocalScale = new(10, 10);
        player.Weapon = new() {
            Hitboxes = new Hitbox[] {
                        new(){
                            new(new(00, -10), 1),
                            new(new(20, -10), 2),
                            new(new(40, -10), 3),
                            new(new(60, -10), 2),
                            new(new(80, -10), 1),
                        },
                        new(){
                            new(new(00, 0), 1),
                            new(new(20, 0), 2),
                            new(new(40, 0), 3),
                            new(new(60, 0), 2),
                            new(new(80, 0), 1),
                        },
                        new(){
                            new(new(00, +10), 1),
                            new(new(20, +10), 2),
                            new(new(40, +10), 3),
                            new(new(60, +10), 2),
                            new(new(80, +10), 1),
                        },
                    },
            AttackSpeed = 0.02f
        };

        player.AddChild(new BodyPartNode(player, new(new(0, 0), 10)) {
            LocalOffset = new(0, 0)
        });
        player.AddChild(new BodyPartNode(player, new(new(0, 0), 10)) {
            LocalOffset = new(0, -20)
        });
        player.AddChild(new BodyPartNode(player, new(new(0, 0), 10)) {
            LocalOffset = new(0, 20)
        });

        Nodes.AddChild(player, NodeFilters.ChildOf(Nodes.Root))
        .AddChild(new EntityNode() {
            LocalOffset = new(10, 0),

        });

        EntityNode tmp =
        Nodes.AddChild(new EntityNode() {
            LocalOffset = new(100, 100),
            Speed = 0.1f,
        }, NodeFilters.ChildOf(Nodes.Root)) as EntityNode;
        tmp.Controller = new RandomMoveController(tmp);

        tmp.AddChild(new EntityNode() {
            LocalOffset = new(10, 50)
        });
    }

    public override void Tick() {
        base.Tick();
    }

    void OnRender(double elapsed) {
        Camera.Position = new(player.Offset - new Vector2(Config.ScreenWidth / 2, Config.ScreenHeight / 2), Camera.Position.Z);

        Nodes.DrawNodes();
    }

    void OnMouseMove(Vector2 oldMouse, Vector2 newMouse) {
        player.Heading = Camera.ScreenToWorld(MousePosition());
        player.LocalRotation = player.LocalOffset.AngleTo(player.Heading);
    }

    public override void Unload() { }
}
