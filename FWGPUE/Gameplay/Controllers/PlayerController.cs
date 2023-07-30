using FWGPUE.Nodes;
using System.Numerics;

namespace FWGPUE.Gameplay.Controllers;

class PlayerController : EntityController {
    public override void Tick() {
        // don't play with movement unless it's valid
        if (DesiredMovement != Vector2.Zero) {
            DesiredMovement = Vector2.Normalize(DesiredMovement);
        }
    }

    void Input(string input) {
        if (input == "up") {
            DesiredMovement.Y--;
        }
        if (input == "down") {
            DesiredMovement.Y++;
        }
        if (input == "left") {
            DesiredMovement.X--;
        }
        if (input == "right") {
            DesiredMovement.X++;
        }

        if (input == "attack") {
            Entity.Weapon?.Attack();
        }
    }

    public PlayerController(EntityNode owner) : base(owner) {
        InputEventFired += Input;
    }
}