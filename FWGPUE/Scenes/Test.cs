using System.Reflection;
using System.Numerics;
using FWGPUE.UI;
using FWGPUE.Nodes;
using FWGPUE.Graphics;

namespace FWGPUE.Scenes;

class Test : Scene {
    EntityNode player = new();

    public override void Load() {
        Load<Test>();

        Renderer.OnRenderObjectsRequired += OnRender;
        MouseMove += OnMouseMove;

        Nodes.AddChild(player, NodeFilters.ChildOf(Nodes.Root))
            .AddChild(new EntityNode() {
                LocalOffset = new(10, 0)
            })
            .AddChild(new PolygonNode() { 
                LocalOffset = new(40, 0),
                Polygons = new PolygonSet(RenderManager.TextManager.GetTextPolygons("default", "A").ToArray()),
                LocalScale = new(15, 15)
            })
            .AddSibling(new ImageNode() { 
                Image = "square",
                LocalOffset = new(30, 10),
                LocalScale = new(3, 3)
            })
            .AddSibling(new ImageNode() { 
                Image = "square",
                LocalOffset = new(-10, 5),
                LocalScale = new(1.5f, 2),
                LocalRotation = 0.2f
            });

        Nodes.AddChild(new EntityNode() { LocalOffset = new(100, 100) }, NodeFilters.ChildOf(Nodes.Root))
            .AddChild(new EntityNode() {
                LocalOffset = new(10, 50)
            });
    }

    public override void Tick() {
        base.Tick();

        // input
        Vector2 direction = Vector2.Zero;
        if (KeyDown(Key.W)) { direction.Y += -1; }
        if (KeyDown(Key.S)) { direction.Y += +1; }
        if (KeyDown(Key.A)) { direction.X += -1; }
        if (KeyDown(Key.D)) { direction.X += +1; }
        player.Velocity = direction * 300f * TickTime;

        if (MouseButtonDown(MouseButton.Left)) {
            player.Heading = Camera.ScreenToWorld(MousePosition());
            player.LocalRotation = player.LocalOffset.AngleTo(player.Heading);
        }
    }

    void OnRender(double elapsed) {
        Camera.Position = new(player.Offset - new Vector2(Config.ScreenWidth / 2, Config.ScreenHeight / 2), Camera.Position.Z);

        Nodes.DrawNodes();
        Nodes.DrawDebugConnections();
    }

    void OnMouseMove(Vector2 oldMouse, Vector2 newMouse) {
    }

    public override void Unload() { }
}

class Weapon {
    // the current hitbox of the weapon is stored as a set of polygons
    public PolygonSet Hitbox;
    // different movements of the weapon are represented as different hitboxes
    public PolygonSet[] Hitboxes;
    // the current index into the hitbox array
    public int AttackStageCounter = 0;

    public virtual void TickHitbox() { }
}