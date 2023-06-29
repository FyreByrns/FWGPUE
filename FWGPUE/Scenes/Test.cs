using System.Reflection;
using System.Numerics;
using FWGPUE.UI;
using FWGPUE.Nodes;

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
                Polygons = new PolygonSet(TextManager.GetTextPolygons("default", "A").ToArray()),
                LocalScale = new(15, 15)
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

class EntityNode : Node2D {
    public Vector2 Heading = new(10, 0);
    public Vector2 Velocity;

    public Weapon Weapon;

    public override void Tick() {
        base.Tick();

        Velocity /= 1.1f;
        LocalOffset += Velocity;
    }

    public override void Draw() {
        base.Draw();

        Renderer.PushCircle(Offset, 10, Z, Vector3.One);
    }
}

class PolygonNode : Node2D {
    public PolygonSet Polygons;

    public override void Draw() {
        base.Draw();

        if (Polygons is null) {
            return;
        }

        foreach (List<Vector2> vertexArray in Polygons.Cast<List<Vector2>>()) {
            Renderer.PushConvexPolygon(
                Z,
                new Colour(1f, 0.3f, 1f),
                false,
                true,
                1,
                vertexArray
                    .RotateAll(new(0, 0), Rotation)
                    .ScaleAll(Scale)
                    .TransformAll(Offset)
                    .ToArray());
        }
    }
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