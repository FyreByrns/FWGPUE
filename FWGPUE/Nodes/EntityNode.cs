using System.Numerics;
using FWGPUE.Gameplay;
using FWGPUE.Gameplay.Controllers;

namespace FWGPUE.Nodes;

class EntityNode : Node2D {
    public EntityController Controller;
    public Vector2 Heading = new(10, 0);
    public Vector2 Velocity;
    public float Speed = 1;

    public Weapon Weapon;

    public override void Tick() {
        base.Tick();

        Controller?.Tick();
        Weapon?.TickHitbox();

        Velocity /= 1.1f;

        if (Controller is not null && Controller.DesiredMovement.LengthSquared() != 0) {
            Velocity += Vector2.Normalize(Controller.DesiredMovement) * Speed;
            Controller.ResetDesiredMovement();
        }

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

class BodyPartNode : Node2D {
    public EntityNode Entity;
    public Hitbox Hitbox;

    public override void Draw() {
        base.Draw();

        if (Hitbox != null) {
            foreach (var circle in Hitbox) {
                Renderer.PushCircle(
                    Offset + circle.position,
                    circle.radius,
                    Z,
                    Vector3.UnitY,
                    false,
                    5);
            }
        }
    }

    public BodyPartNode(EntityNode owner, Circle basicHitbox) : base() {
        Entity = owner;
        Hitbox = new() {
            basicHitbox
        };
    }
}
