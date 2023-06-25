using System.Numerics;

namespace FWGPUE;

public static class Extensions {
    public static float AngleTo(this Vector2 me, Vector2 other) {
        float dx = other.X - me.X;
        float dy = other.Y - me.Y;

        return RadiansToTurns(MathF.Atan2(dy, dx));
    }

    /// <summary>
    /// Get the Vector2 a distance away from this Vector2 at a given angle.
    /// </summary>
    public static Vector2 Along(this Vector2 start, float distance, float angle) {
        float angleRadians = TurnsToRadians(angle);

        Matrix3x2 rotation = Matrix3x2.CreateRotation(angleRadians);
        Matrix3x2 translation = Matrix3x2.CreateTranslation(new(0, distance));

        return start + Vector2.Transform(new(), translation * rotation);
    }

    public static IEnumerable<Vector2> ScaleAll(this IEnumerable<Vector2> me, Vector2 scale) {
        foreach (Vector2 v in me) {
            yield return v * scale;
        }
    }
    public static IEnumerable<Vector2> TransformAll(this IEnumerable<Vector2> me, Vector2 transform) {
        foreach (Vector2 v in me) {
            yield return v + transform;
        }
    }
    public static IEnumerable<Vector2> RotateAll(this IEnumerable<Vector2> me, Vector2 origin, float angle) {
        foreach(Vector2 v in me) {
            float sin = MathF.Sin(TurnsToRadians(angle));
            float cos = MathF.Cos(TurnsToRadians(angle));

            Vector2 working = v - origin;
            yield return origin + new Vector2(working.X * cos - working.Y * sin, working.X*sin+working.Y*cos);
        }
    }

    public static IEnumerable<Vector2[]> ScaleAll(this IEnumerable<Vector2[]> me, Vector2 scale) {
        foreach(Vector2[] v in me) {
            yield return v.ScaleAll(scale).ToArray();
        }
    }
    public static IEnumerable<Vector2[]> TransformAll(this IEnumerable<Vector2[]> me, Vector2 transform) {
        foreach (Vector2[] v in me) {
            yield return v.TransformAll(transform).ToArray();
        }
    }

    public static Vector2 XY(this Vector3 me) {
        return new(me.X, me.Y);
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

    public static float Lerp(float from, float to, float t) {
        float range = to - from;
        return t * range;
    }

    #endregion math helper extensions
}
