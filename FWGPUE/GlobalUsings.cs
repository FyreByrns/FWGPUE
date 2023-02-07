global using TextColour = FontStashSharp.FSColor;
global using TextAlignment = FWGPUE.Engine.TextAlignment;

global using static FWGPUE.Engine;
global using static FWGPUE.Input;
global using static FWGPUE.GlobalHelpers;
global using static FWGPUE.IO.ConfigFile;

namespace FWGPUE {
    public static class GlobalHelpers {
        public static float DegreesToRadians(float degrees) {
            return MathF.PI / 180f * degrees;
        }
        public static float TurnsToRadians(float turns) {
            return turns * MathF.PI * 2f;
        }
    }
}