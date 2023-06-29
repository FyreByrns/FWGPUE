using System.Numerics;
using FWGPUE.Graphics;

namespace FWGPUE.Nodes;

class PolygonNode : Node2D
{
    public PolygonSet Polygons;

    public override void Draw()
    {
        base.Draw();

        if (Polygons is null)
        {
            return;
        }

        foreach (List<Vector2> vertexArray in Polygons.Cast<List<Vector2>>())
        {
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
