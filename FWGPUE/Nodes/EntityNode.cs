using System.Numerics;
using FWGPUE.Scenes;

namespace FWGPUE.Nodes;

class EntityNode : Node2D
{
    public Vector2 Heading = new(10, 0);
    public Vector2 Velocity;

    public Weapon Weapon;

    public override void Tick()
    {
        base.Tick();

        Velocity /= 1.1f;
        LocalOffset += Velocity;
    }

    public override void Draw()
    {
        base.Draw();

        Renderer.PushCircle(Offset, 10, Z, Colour.One);
    }
}
