using Silk.NET.Windowing;
using Silk.NET.Maths;
using FWGPUE.IO;

using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Core.Native;
using System.Text;
using Silk.NET.OpenGL;
using System.Runtime.InteropServices;

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

        // compile vertex shader
        Blob vertexErrors = default;
        unsafe {
            HResult result = compiler.Compile(
                in shaderBytes[0],
                (nuint)Source.Length,
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
            }
        }
    }
    public void Create(Device device) {
        unsafe {
            SilkMarshal.ThrowHResult(device.CreateVertexShader(
                vertexCode.GetBufferPointer(),
                vertexCode.GetBufferSize(),
                ref nullref<ID3D11ClassLinkage>(),
                ref vertexShader
            ));
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

class D3D11Renderer {
    #region
#if DEBUG
    public bool LogInfo { get; } = true;
#else
    public bool LogInfo { get; }= false;
#endif
    #endregion

    float[] vertices =
{
    //X    Y      Z
    0.5f,  0.5f, 0.0f,
    0.5f, -0.5f, 0.0f,
    -0.5f, -0.5f, 0.0f,
    -0.5f,  0.5f, 0.5f
};

    uint[] indices =
    {
    0, 1, 3,
    1, 2, 3
};

    float[] backgroundColour = new[] { 0.0f, 0.0f, 0.0f, 1.0f };

    public static D3D11Renderer Instance { get; private set; }

    Shader shader;

    public IWindow Window { get; private set; }

    public DXGI dxgi = DXGI.GetApi();
    public D3D11 d3d11 = D3D11.GetApi();
    public D3DCompiler compiler = D3DCompiler.GetApi();

    Factory factory = default;
    Swapchain swapchain = default;
    Device device = default;
    DeviceContext deviceContext = default;
    VertexBuffer<float> vertexBuffer;
    Buffer<uint> indexBuffer;
    //Buffer vertexBuffer = default;
    //Buffer indexBuffer = default;
    VertexShader vertexShader = default;
    PixelShader pixelShader = default;

    public void Setup() {
        shader = new Shader("assets/d3d11testing.shader");
        shader.Load();

        CreateDevice();
        CreateSwapchain();

        vertexBuffer = new(device, vertices, 3, 0);
        indexBuffer = new(device, indices, BindFlag.IndexBuffer);

        shader.Compile(compiler);
        shader.Create(device);
    }
    void CreateDevice() {
        unsafe {
            SilkMarshal.ThrowHResult(d3d11.CreateDevice(
                default(DXGIAdapter),
                D3DDriverType.Hardware,
                Software: default,
                (uint)CreateDeviceFlag.Debug,
                null,
                0,
                D3D11.SdkVersion,
                ref device,
                null,
                ref deviceContext
            ));

            if (LogInfo) {
                device.SetInfoQueueCallback(msg => Log.Info(SilkMarshal.PtrToString((nint)msg.PDescription) ?? "null message", true, "D3D Device", "", 0));
            }
        }
    }
    void CreateSwapchain() {
        // describe swapchain
        SwapChainDesc1 swapChainDescription = new() {
            BufferCount = 2, // double-buffered
            Format = Format.FormatR8G8B8A8Unorm,
            BufferUsage = DXGI.UsageRenderTargetOutput,
            SwapEffect = SwapEffect.FlipDiscard,
            SampleDesc = new SampleDesc(1, 0)
        };

        // create factory to create the swapchain
        factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();

        // create swapchain
        unsafe {
            SilkMarshal.ThrowHResult(factory.CreateSwapChainForHwnd(
                device,
                Window.Native!.DXHandle!.Value,
                in swapChainDescription,
                null,
                ref nullref<IDXGIOutput>(),
                ref swapchain
            ));
        }
    }

    public void FramebufferResize(Vector2D<int> newSize) {
        SilkMarshal.ThrowHResult(swapchain.ResizeBuffers(0, (uint)newSize.X, (uint)newSize.Y, Format.FormatR8G8B8A8Unorm, 0));
    }

    enum DXGI_ERROR :uint {
        DXGI_ERROR_DEVICE_HUNG = 0x887A0006,
        DXGI_ERROR_DEVICE_REMOVED = 0x887A0005,
        DXGI_ERROR_DEVICE_RESET = 0x887A0007,
        DXGI_ERROR_DRIVER_INTERNAL_ERROR = 0x887A0020,
        DXGI_ERROR_INVALID_CALL
    }

    public void Render() {
        unsafe {
            // Obtain the framebuffer for the swapchain's backbuffer.
            using var framebuffer = swapchain.GetBuffer<ID3D11Texture2D>(0);

            // Create a view over the render target.
            RenderTargetView renderTargetView = default;
            try {
                SilkMarshal.ThrowHResult(device.CreateRenderTargetView(framebuffer, null, ref renderTargetView));
            }
            catch (COMException e){
                // device removed error probably
                // .. see <https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11device-getdeviceremovedreason>
                // ..     <https://learn.microsoft.com/en-us/windows/win32/direct3ddxgi/dxgi-error>
                Log.Error($"probably device removed: {(DXGI_ERROR)device.GetDeviceRemovedReason()}");
                return;
            }
            // Clear the render target to be all black ahead of rendering.
            deviceContext.ClearRenderTargetView(renderTargetView, ref backgroundColour[0]);

            // Update the rasterizer state with the current viewport.
            var viewport = new Viewport(0, 0, Window.FramebufferSize.X, Window.FramebufferSize.Y, 0, 1);
            deviceContext.RSSetViewports(1, in viewport);

            // Tell the output merger about our render target view.
            deviceContext.OMSetRenderTargets(1, ref renderTargetView, ref nullref<ID3D11DepthStencilView>());

            // Update the input assembler to use our shader input layout, and associated vertex & index buffers.
            deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
            deviceContext.IASetInputLayout(shader.inputLayout);
            deviceContext.IASetVertexBuffers(0, 1, vertexBuffer.D3DBuffer, in vertexBuffer.ByteStride, in vertexBuffer.Offset);
            deviceContext.IASetIndexBuffer(indexBuffer.D3DBuffer, Format.FormatR32Uint, 0);

            // Bind our shaders.
            deviceContext.VSSetShader(vertexShader, ref nullref<ClassInstance>(), 0);
            deviceContext.PSSetShader(pixelShader, ref nullref<ClassInstance>(), 0);

            // Draw the quad.
            deviceContext.DrawIndexed(6, 0, 0);

            // Present the drawn image.
            swapchain.Present(1, 0);

            // Clean up any resources created in this method.
            renderTargetView.Dispose();
        }
    }

    public D3D11Renderer(IWindow window) {
        Instance = this;

        Window = window;
    }
}
