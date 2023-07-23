using System.Numerics;
using FWGPUE.Gameplay;

namespace FWGPUE.Nodes;

class EntityNode : Node2D {
    public Vector2 Heading = new(10, 0);
    public Vector2 Velocity;

    public Weapon Weapon;

    public override void Tick() {
        base.Tick();

        Weapon?.TickHitbox();

        Velocity /= 1.1f;
        LocalOffset += Velocity;
    }

    public override void Draw() {
        base.Draw();

        Renderer.PushCircle(Offset, 10, Z, Colour.One);

        // show weapon hitbox
        if (Weapon != null && Weapon.Attacking) {
            foreach (var circle in Weapon.Hitbox.Circles) {
                Renderer.PushCircle(circle.position.Rotate(Vector2.Zero, Rotation) + Offset, circle.radius * Scale.X, Z, Colour.One);
            }
        }
    }
}
