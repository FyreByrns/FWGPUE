using System.Numerics;

namespace FWGPUE.Graphics;

class OrthoCamera2D : ICamera {
    public Vector3 Position;
    public Vector3 Target => new(Position.X, Position.Y, 0);

    public Vector3 Direction => Vector3.Normalize(Position - Target);
    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Direction));
    public Vector3 Up => Vector3.Cross(Direction, Right);

    public Vector2 Center => ScreenToWorld((Vector2)Window!.Size / 2);

    #region camera interface requirements

    public Vector2 WorldToScreen(Vector2 worldSpace) {
        return new(worldSpace.X + Position.X, worldSpace.Y + Position.Y);
    }

    public Vector2 ScreenToWorld(Vector2 screenSpace) {
        //screenSpace.X += Window!.Size.X / 2;
        //screenSpace.Y += Window.Size.Y / 2;

        //screenSpace.Y *= -1;
        //screenSpace.Y += Window!.Size.Y;

        //Vector3 ssv3 = new(screenSpace, 0);
        //ssv3 = Vector3.Transform(ssv3, ViewMatrix * ProjectionMatrix);
        //ssv3 *= new Vector3(Window!.Size.X, Window.Size.Y, 0);

        //return new(ssv3.X / 2, ssv3.Y / 2);

        //screenSpace.X += Position.X;
        //screenSpace.Y += Position.Y;

        return screenSpace + new Vector2(Position.X, Position.Y);
    }

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Target, Up);

    public Matrix4x4 ProjectionMatrix => Matrix4x4.CreateOrthographicOffCenter(0, Window!.Size.X, Window.Size.Y, 0, 0.1f, 200f);

    #endregion camera interface requirements

    public OrthoCamera2D(Vector2 position, float height) {
        Position = new Vector3(position, height);
    }
}
