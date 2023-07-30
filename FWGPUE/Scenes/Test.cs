﻿using System.Reflection;
using System.Numerics;
using FWGPUE.UI;
using FWGPUE.Nodes;
using FWGPUE.Graphics;
using FWGPUE.Gameplay;

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

        int searchRadius = 80;
        Renderer.PushCircle(player.Offset, searchRadius, 10, new(0, 1, 0), false, 10);
        foreach(var node in Nodes.Grid.GetNodesInCircle(new(player.Offset, searchRadius))) {
            Renderer.PushCircle(node.Offset, 30, 10, Vector3.UnitZ, false, 10);
        }


        Nodes.DrawNodes();
    }

    void OnMouseMove(Vector2 oldMouse, Vector2 newMouse) {
        player.Heading = Camera.ScreenToWorld(MousePosition());
        player.LocalRotation = player.LocalOffset.AngleTo(player.Heading);
    }

    public override void Unload() { }
}
