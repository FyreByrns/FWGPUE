using System.Numerics;

namespace FWGPUE;

public static class Extensions {
    /// <summary>
    /// Get the Vector2 a distance away from this Vector2 at a given angle.
    /// </summary>
    public static Vector2 Along(this Vector2 start, float distance, float angle) {
        float angleRadians = TurnsToRadians(angle);

        Matrix3x2 rotation = Matrix3x2.CreateRotation(angleRadians);
        Matrix3x2 translation = Matrix3x2.CreateTranslation(new(0, distance));

        return start + Vector2.Transform(new(), translation * rotation);
    }

    public static void ChangeBy(ref this Vector3 me, Vector3 change) {
        me += change;
    }

    #region math helper extensions

    public static int Squared(this int me) {
        return me * me;
    }
    public static float Squared(this float me) {
        return me * me;
    }

    #endregion math helper extensions
}
