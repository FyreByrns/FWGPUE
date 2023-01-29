using System.Numerics;

namespace FWGPUE;

public static class Extensions {
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
