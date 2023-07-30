using System.Numerics;
using FWGPUE.Nodes;

namespace FWGPUE.Gameplay.Controllers;

class RandomMoveController : EntityController {
    static Random Random = new();

    public float FindNewMoveTargetTime = 1;
    public float FindNewMoveTargetTimeAccumulator = 0;
    public float RandomMoveTargetDistance = 1000;

    public Vector2 RandomMoveTarget;

    public override void Tick() {
        FindNewMoveTargetTimeAccumulator += TickTime;
        while (FindNewMoveTargetTimeAccumulator >= FindNewMoveTargetTime) {
            FindNewMoveTargetTimeAccumulator -= FindNewMoveTargetTime;

            RandomMoveTarget =
                Entity.Offset.Along(
                    Random.NextSingle() * RandomMoveTargetDistance,
                    Random.NextSingle());
        }

        Vector2 localMoveTarget = RandomMoveTarget - Entity.Offset;

        DesiredMovement = localMoveTarget;
        Entity.LocalRotation = Entity.Offset.AngleTo(Entity.Offset + localMoveTarget);

        Renderer.PushCircle(RandomMoveTarget, 5, Entity.Z, new(0.2f));
        Renderer.PushLine(Entity.Offset, RandomMoveTarget, Entity.Z, new(0.3f));
    }

    public RandomMoveController(EntityNode owner) : base(owner) { }
}