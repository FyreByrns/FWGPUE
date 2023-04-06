global using TextColour = FontStashSharp.FSColor;
global using TextAlignment = FWGPUE.Engine.TextAlignment;
global using Key = Silk.NET.Input.Key;
global using MouseButton = Silk.NET.Input.MouseButton;
global using FileType = FWGPUE.IO.EngineFileLocation.FileType;

global using static FWGPUE.Engine;
global using static FWGPUE.Input;
global using static FWGPUE.GlobalHelpers;
global using static FWGPUE.IO.ConfigFile;

#region ease-of-use d3d aliases
global using Factory = Silk.NET.Core.Native.ComPtr<Silk.NET.DXGI.IDXGIFactory2>;
global using Swapchain = Silk.NET.Core.Native.ComPtr<Silk.NET.DXGI.IDXGISwapChain1>;
global using Device = Silk.NET.Core.Native.ComPtr<Silk.NET.Direct3D11.ID3D11Device>;
global using DeviceContext = Silk.NET.Core.Native.ComPtr<Silk.NET.Direct3D11.ID3D11DeviceContext>;
global using VertexShader = Silk.NET.Core.Native.ComPtr<Silk.NET.Direct3D11.ID3D11VertexShader>;
global using PixelShader = Silk.NET.Core.Native.ComPtr<Silk.NET.Direct3D11.ID3D11PixelShader>;
global using InputLayout = Silk.NET.Core.Native.ComPtr<Silk.NET.Direct3D11.ID3D11InputLayout>;
global using DXGIAdapter = Silk.NET.Core.Native.ComPtr<Silk.NET.DXGI.IDXGIAdapter>;
global using RenderTargetView = Silk.NET.Core.Native.ComPtr<Silk.NET.Direct3D11.ID3D11RenderTargetView>;
global using ClassInstance = Silk.NET.Core.Native.ComPtr<Silk.NET.Direct3D11.ID3D11ClassInstance>;
global using Blob = Silk.NET.Core.Native.ComPtr<Silk.NET.Core.Native.ID3D10Blob>;
#endregion

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

        public static ref T nullref<T>() {
            return ref System.Runtime.CompilerServices.Unsafe.NullRef<T>();
        }
    }
}