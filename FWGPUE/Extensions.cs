using System.Numerics;

namespace FWGPUE;

public static class Extensions {
    public static void ChangeBy(ref this Vector3 me, Vector3 change) {
        me += change;
    }
}
