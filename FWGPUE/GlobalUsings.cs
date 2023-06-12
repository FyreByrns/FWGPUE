global using Key = Silk.NET.Input.Key;
global using MouseButton = Silk.NET.Input.MouseButton;
global using FileType = FWGPUE.IO.EngineFileLocation.FileType;

global using static FWGPUE.Engine;
global using static FWGPUE.Input;
global using static FWGPUE.GlobalHelpers;
global using static FWGPUE.IO.ConfigFile;

global using static FWGPUE.Graphics.RenderManager;

global using FWGPUE.IO;
using System.Numerics;

namespace FWGPUE {
    public static class GlobalHelpers {
        public static float DegreesToRadians(float degrees) {
            return MathF.PI / 180f * degrees;
        }
        public static float TurnsToRadians(float turns) {
            return turns * MathF.PI * 2f;
        }
        public static float RadiansToTurns(float radians) {
            return radians / MathF.PI / 2f;
        }

        public static int NearestPowerOfTwo(int input) {
            return NearestPowerOf(2, input);
        }
        public static int NearestPowerOf(int power, int input) {
            return (int)Math.Pow(power, Math.Ceiling(Math.Log(input) / Math.Log(power)));
        }
    }

    public static class GeometryGeneration {
        public static IEnumerable<Vector2> GenerateLineGeo(Vector2 a, Vector2 b, float thickness = 1) {
            float angleBetween = RadiansToTurns((float)Math.Atan2(a.Y - b.Y, a.X - b.X));
            float anglePlusHalf = angleBetween + 0.5f;

            thickness /= 2;

            Vector2 _a = a.Along(+thickness, anglePlusHalf);
            Vector2 _b = a.Along(-thickness, anglePlusHalf);
            Vector2 _c = b.Along(+thickness, anglePlusHalf);
            Vector2 _d = b.Along(-thickness, anglePlusHalf);

            yield return _a;
            yield return _c;
            yield return _d;
            yield return _b;
        }
        public static IEnumerable<Vector2[]> GenerateCircleGeo(Vector2 center, float fromAngle, float length, float radius, float thickness, int steps = 30) {
            for (int i = 1; i <= steps; i++) {
                float angleChange = length / (float)steps;
                float lastAngle = fromAngle + angleChange * (i - 1);
                float angle = fromAngle + angleChange * i;

                yield return new Vector2[] {
                    center.Along(radius-thickness, lastAngle),
                    center.Along(radius-thickness, angle),
                    center.Along(radius, angle),
                    center.Along(radius, lastAngle),
                };
            }
        }
        public static IEnumerable<Vector2> GenerateRectGeo(Vector2 topLeft, Vector2 bottomRight) {
            yield return topLeft;
            yield return new(bottomRight.X, topLeft.Y);
            yield return bottomRight;
            yield return new(topLeft.X, bottomRight.Y);
        }
    }

}