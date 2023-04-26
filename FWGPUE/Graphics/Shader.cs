using FWGPUE.IO;

using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Core.Native;
using System.Text;

namespace FWGPUE.Graphics;

class Shader : ByteFile {
    public string Source;
    public VertexShader vertexShader;
    public PixelShader pixelShader;
    public InputLayout inputLayout = default;

    Blob vertexCode = default;
    Blob pixelCode = default;

    public bool Compiled { get; private set; }

    public void SetLayout(params (string name, Type type)[] variables) {
        foreach ((string name, Type type) in variables) {

        }
    }

    public void Compile(D3DCompiler compiler) {
        byte[] shaderBytes = Data!;
        bool success = false;

        // compile vertex shader
        Blob vertexErrors = default;
        unsafe {
            HResult result = compiler.Compile(
                in shaderBytes[0],
                (nuint)shaderBytes.Length,
                nameof(Source),
                null,
                ref nullref<ID3DInclude>(),
                "vs_main",
                "vs_5_0",
                0,
                0,
                ref vertexCode,
                ref vertexErrors
            );
            if (result.IsFailure) {
                if (vertexErrors.Handle is not null) {
                    Log.Error(SilkMarshal.PtrToString((nint)vertexErrors.GetBufferPointer(), NativeStringEncoding.LPWStr) ?? "null message");
                }
            }
            else {
                success = true;
            }
        }

        // compile pixel shader
        Blob pixelErrors = default;
        unsafe {
            HResult result = compiler.Compile(
                in shaderBytes[0],
                (nuint)shaderBytes.Length,
                nameof(Source),
                null,
                ref nullref<ID3DInclude>(),
                "ps_main",
                "ps_5_0",
                0,
                0,
                ref pixelCode,
                ref pixelErrors
            );
            if (result.IsFailure) {
                if (pixelErrors.Handle is not null) {
                    Log.Error(SilkMarshal.PtrToString((nint)pixelErrors.GetBufferPointer(), NativeStringEncoding.LPWStr) ?? "null message");
                }
                success = false;
            }
        }

        // set compiled flag
        Compiled = success;
    }
    public void Create(Device device) {
        unsafe {
            // create vertex shader
            SilkMarshal.ThrowHResult(device.CreateVertexShader(
                vertexCode.GetBufferPointer(),
                vertexCode.GetBufferSize(),
                ref nullref<ID3D11ClassLinkage>(),
                ref vertexShader
            ));
            // create pixel shader
            SilkMarshal.ThrowHResult(device.CreatePixelShader(
                pixelCode.GetBufferPointer(),
                pixelCode.GetBufferSize(),
                ref nullref<ID3D11ClassLinkage>(),
                ref pixelShader
            ));

            // describe layout
            fixed (byte* name = SilkMarshal.StringToMemory("POS")) {
                InputElementDesc inputElement = new() {
                    SemanticName = name,
                    SemanticIndex = 0,
                    Format = Format.FormatR32G32B32Float,
                    InputSlot = 0,
                    AlignedByteOffset = 0,
                    InputSlotClass = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                };

                SilkMarshal.ThrowHResult(device.CreateInputLayout(
                    in inputElement,
                    1,
                    vertexCode.GetBufferPointer(),
                    vertexCode.GetBufferSize(),
                    ref inputLayout
                ));
            }
        }
    }

    protected override void ReadData(byte[] data) {
        try {
            Source = Encoding.ASCII.GetString(data);
            Data = Encoding.ASCII.GetBytes(Source);
        }
        catch (Exception e) {
            Log.Error($"error loading shader: {e}");
        }
    }

    protected override byte[] SaveData() {
        return Encoding.ASCII.GetBytes(Source);
    }

    public Shader(EngineFileLocation location) : base(location) { }
}
