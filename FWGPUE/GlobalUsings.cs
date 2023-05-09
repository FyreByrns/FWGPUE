global using Key = Silk.NET.Input.Key;
global using MouseButton = Silk.NET.Input.MouseButton;
global using FileType = FWGPUE.IO.EngineFileLocation.FileType;

global using static FWGPUE.Engine;
global using static FWGPUE.Input;
global using static FWGPUE.GlobalHelpers;
global using static FWGPUE.IO.ConfigFile;

global using static FWGPUE.Graphics.Renderer;

namespace FWGPUE
{
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
}