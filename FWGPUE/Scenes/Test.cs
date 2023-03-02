﻿using FontStashSharp;
using FWGPUE.Nodes;
using System.Numerics;

namespace FWGPUE.Scenes;

class Test : Scene {
    Node2D MouseFollowNode;

    public override void Load() {
        Load<Test>();

        MouseFollowNode = Nodes.Root.AddChild(new SpriteNode() {
            Sprite = "otherSquare",
            Scale = 2
        });

        MouseFollowNode.AddChild(new SpriteNode() {
            Offset = new(100, 10),
            Sprite = "square",
            Scale = 3,
        }).AddChild(new SpriteNode() {
            Offset = new(50, 10),
            Sprite = "otherSquare"
        }).AddChild(new SpriteNode() { 
            Offset = new(60, 0),
            Sprite = "square"
        });
    }

    public override void Tick() {
        base.Tick();

        MouseFollowNode.Offset = Camera.ScreenToWorld(MousePosition());
        MouseFollowNode.Rotation = TotalTimeInScene / 10f;
        MouseFollowNode.Children.First().Rotation = TotalTimeInScene / 5f;

        if (KeyDown(Key.Up)) { Camera!.Position.ChangeBy(new(0, -50 * TickTime, 0)); }
        if (KeyDown(Key.Down)) { Camera!.Position.ChangeBy(new(0, 50 * TickTime, 0)); }
        if (KeyDown(Key.Left)) { Camera!.Position.ChangeBy(new(-50 * TickTime, 0, 0)); }
        if (KeyDown(Key.Right)) { Camera!.Position.ChangeBy(new(50 * TickTime, 0, 0)); }
    }

    public override void Render() {
        base.Render();

        //SpriteBatcher!.DrawSprite(new(100 + (float)Math.Sin(TotalTimeInScene) * 100, 100), 0, Atlas, "square");
        //SpriteBatcher!.DrawSprite(new Graphics.Sprite(Atlas!) {
        //    Texture = "square",
        //    Transform = {
        //        Scale = new(10, 10, 1),
        //        Position = new(100, 100, 1)
        //    }
        //});
        //SpriteBatcher!.DrawSprite(new Graphics.Sprite(Atlas!) {
        //    Texture = "otherSquare",
        //    Transform = {
        //        Scale = new(10, 10, 1),
        //        Position = new(100, 100, 0),
        //        Rotation = new(0, TotalTimeInScene / 20f, 0.2f)
        //    }
        //});

        DrawCircle(new Vector3(1, 1, 1), MousePosition(), 4);

        Nodes.DrawNodes();
        Nodes.DrawDebugNodes();
        Nodes.DrawDebugConnections();
    }

    public override void Unload() { }
}