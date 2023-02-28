using System.Numerics;

namespace FWGPUE.Graphics;

interface ICamera {
    public Vector2 WorldToScreen(Vector2 world);
    public Vector2 ScreenToWorld(Vector2 screen);

    public Matrix4x4 ViewMatrix { get; }
    public Matrix4x4 ProjectionMatrix { get; }
}
