using System.Numerics;
using FWGPUE.Nodes;

namespace FWGPUE.Gameplay.Controllers;

abstract class EntityController {
    public EntityNode Entity;
    public Vector2 DesiredMovement;

    public void ResetDesiredMovement() {
        DesiredMovement = Vector2.Zero;
    }

    public abstract void Tick();

    protected EntityController(EntityNode owner) {
        Entity = owner;
    }
}
