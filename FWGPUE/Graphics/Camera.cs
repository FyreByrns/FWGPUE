using Silk.NET.Input;
using Silk.NET.OpenAL;
using System.Numerics;

namespace FWGPUE.Graphics;

class Camera {
    #region location
    public Vector3 Position { get; protected set; }
    public Vector3 Target { get; protected set; }

    /// <summary>
    /// Reset the target to be exactly (Position.xy, 0)
    /// </summary>
    public void ResetTarget() {
        Target = new Vector3(Position.X, Position.Y, 0);
    }

    /// <summary>
    /// Set the camera position.
    /// </summary>
    /// <param name="moveTarget">Whether to keep the camera target directly below.</param>
    public void SetPosition(Vector3 position, bool moveTarget = true) {
        Position = position;
        if (moveTarget) {
            ResetTarget();
        }
    }
    /// <summary>
    /// Lerp the camera position from one location to another by t.
    /// </summary>
    /// <param name="moveTarget">Whether to keep the camera target directly below.</param>
    public void LerpPosition(Vector3 from, Vector3 to, float t, bool moveTarget = true) {
        SetPosition(Vector3.Lerp(from, to, t), moveTarget);
    }
    #endregion location

    #region projection stuff
    public Vector3 Direction => Vector3.Normalize(Position - Target);
    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Direction));
    public Vector3 Up => Vector3.Cross(Direction, Right);

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Target, Up);
    public Matrix4x4 ProjectionMatrix(float width, float height) =>
        Matrix4x4.CreateOrthographic(width, height, 0.1f, 200f);
        //Matrix4x4.CreatePerspectiveFieldOfView(Engine.DegreesToRadians(45.0f), width / height, 0.1f, 200.0f);
    #endregion projection stuff

    public Camera(Vector2 position, float height) {
        Position = new Vector3(position, height);
        ResetTarget();
    }
}
