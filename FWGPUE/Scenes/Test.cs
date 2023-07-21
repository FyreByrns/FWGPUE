using System.Reflection;
using System.Numerics;
using FWGPUE.UI;
using FWGPUE.Nodes;
using FWGPUE.Graphics;
using System.Collections;

namespace FWGPUE.Scenes;

class Test : Scene {
    EntityNode player = new();
    Vector2 desiredMovement = Vector2.Zero;

    public override void Load() {
        Load<Test>();

        Renderer.OnRenderObjectsRequired += OnRender;
        MouseMove += OnMouseMove;
        InputEventFired += InputEvent;

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

        Nodes.AddChild(player, NodeFilters.ChildOf(Nodes.Root))
            .AddChild(new EntityNode() {
                LocalOffset = new(10, 0),
            });

        Nodes.AddChild(new EntityNode() { LocalOffset = new(100, 100) }, NodeFilters.ChildOf(Nodes.Root))
            .AddChild(new EntityNode() {
                LocalOffset = new(10, 50)
            });
    }

    private void InputEvent(string input) {
        switch (input) {
            case "up": {
                    desiredMovement.Y += -1;
                    break;
                }
            case "down": {
                    desiredMovement.Y += +1;
                    break;
                }
            case "left": {
                    desiredMovement.X += -1;
                    break;
                }
            case "right": {
                    desiredMovement.X += +1;
                    break;
                }

            case "attack": {
                    player.Weapon.Attack();

                    break;
                }
        }
    }

    public override void Tick() {
        base.Tick();

        // input
        if (desiredMovement.LengthSquared() > 0) {
            player.Velocity = Vector2.Normalize(desiredMovement) * 300f * TickTime;
            desiredMovement = Vector2.Zero;
        }
    }

    void OnRender(double elapsed) {
        Camera.Position = new(player.Offset - new Vector2(Config.ScreenWidth / 2, Config.ScreenHeight / 2), Camera.Position.Z);

        foreach (NodeGridCoordinates ngc in Nodes.Grid.Grid.Keys) {
            Vector2 topLeft = new Vector2(ngc.X, ngc.Y) * NodeGrid.GridSize;
            Vector2 bottomRight = new Vector2(ngc.X + 1, ngc.Y + 1) * NodeGrid.GridSize;

            Renderer.PushRect(topLeft, bottomRight, 10, Colour.Zero, 1, Colour.One, false);
        }

        int searchRadius = 80;
        Renderer.PushCircle(player.Offset, searchRadius, 10, new(0.2f, 0.1f, 1f), false);
        foreach(var node in Nodes.Grid.GetNodesInCircle(new(player.Offset, searchRadius))) {
            Renderer.PushCircle(node.Offset, 25, 10, new(1, 0, 0.2f), false);
        }

        Nodes.DrawNodes();
        //Nodes.DrawDebugConnections();
    }

    void OnMouseMove(Vector2 oldMouse, Vector2 newMouse) {
        player.Heading = Camera.ScreenToWorld(MousePosition());
        player.LocalRotation = player.LocalOffset.AngleTo(player.Heading);
    }

    public override void Unload() { }
}

/// <summary>
/// Used to test intersection.
/// <para> Currently just a set of circles. </para>
/// </summary>
class Hitbox : IEnumerable<Circle> {
    public List<Circle> Circles = new();

    public void Add(Circle circle) {
        Circles.Add(circle);
    }

    public bool Intersects(Hitbox other) {
        // test circle intersection between all circles in this and other
        foreach (var circle in Circles) {
            foreach (var otherCircle in other.Circles) {
                if ((circle.position - otherCircle.position).Length() <= circle.radius + otherCircle.radius) {
                    return true;
                }
            }
        }

        return false;
    }

    #region ienumerable
    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)Circles).GetEnumerator();
    }
    public IEnumerator<Circle> GetEnumerator() {
        return ((IEnumerable<Circle>)Circles).GetEnumerator();
    }
    #endregion ienumerable
}

class Weapon {
    /// <summary>
    /// Whether the hitbox of the weapon is currently active.
    /// </summary>
    public bool Attacking;
    /// <summary>
    /// Whether the weapon can restart an attack during an attack.
    /// </summary>
    public bool CanCancelIntoNewAttack;

    /// <summary>
    /// The current hitbox of the weapon is stored as a set of polygons.
    /// </summary>
    public Hitbox Hitbox => Hitboxes[AttackStageCounter];
    /// <summary>
    /// Different movements of the weapon are represented as different hitboxes.
    /// </summary>
    public Hitbox[] Hitboxes;
    /// <summary>
    /// The current index into the hitbox array.
    /// </summary>
    public int AttackStageCounter = 0;
    /// <summary> 
    /// Speed at which the attack stage advances. 
    /// </summary>
    public float AttackSpeed;
    /// <summary> 
    /// Attack stage accumulator. 
    /// </summary>
    public float TimeInCurrentStage;

    public virtual void StopAttack() {
        Attacking = false;
        AttackStageCounter = 0;
        TimeInCurrentStage = 0;
    }
    public void ForceAttack() {
        StopAttack();
        Attacking = true;
    }

    public virtual void Attack() {
        // if the weapon can start attacking regardless of attack state, just do that
        if (CanCancelIntoNewAttack) {
            ForceAttack();
            return;
        }

        // otherwise, don't attack if the weapon is already attacking
        if (Attacking) {
            return;
        }

        ForceAttack();
    }

    public virtual void TickHitbox() {
        if (Attacking) {
            TimeInCurrentStage += TickTime;

            if (TimeInCurrentStage >= AttackSpeed) {
                AttackStageCounter++;
                TimeInCurrentStage = 0;

                if (AttackStageCounter >= Hitboxes.Length) {
                    // if there are no more hitboxes, the weapon is done attacking
                    StopAttack();
                }
            }

            // TODO: find overlaps and resolve damage etc.
        }
    }
}