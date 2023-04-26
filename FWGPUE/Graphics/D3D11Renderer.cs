using Silk.NET.Windowing;
using Silk.NET.Maths;
using FWGPUE.IO;

using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Core.Native;
using System.Runtime.InteropServices;

namespace FWGPUE.Graphics;

class D3D11Renderer {
    public const bool ForceDXVK = false;

    #region
#if DEBUG
    public bool LogInfo { get; } = true;
#else
    public bool LogInfo { get; }= false;
#endif
    #endregion

    float[] vertices = {
        //X    Y      Z
        0.5f,  0.5f, 0.0f,
        0.5f, -0.5f, 0.0f,
        -0.5f, -0.5f, 0.0f,
        -0.5f,  0.5f, 0.5f
    };

    uint[] indices = {
        0, 1, 3,
        1, 2, 3
    };

    float[] backgroundColour = new[] { 0.0f, 0.0f, 0.0f, 1.0f };

    public static D3D11Renderer Instance { get; private set; }

    Shader shader;

    public IWindow Window { get; private set; }

    public DXGI dxgi;
    public D3D11 d3d11;
    public D3DCompiler compiler;

    Factory factory = default;
    Swapchain swapchain = default;
    Device device = default;
    DeviceContext deviceContext = default;

    DrawObject drawObject;
    List<DrawObject> drawObjects = new();
    VertexBuffer<float> vertexBuffer;
    Buffer<uint> indexBuffer;

    public void Setup() {
        dxgi = DXGI.GetApi(Window, ForceDXVK);
        d3d11 = D3D11.GetApi(Window, ForceDXVK);
        compiler = D3DCompiler.GetApi();

        CreateDevice();
        CreateSwapchain();

        if (LogInfo) {
            unsafe {
                device.SetInfoQueueCallback(msg => Log.Info($"[DX info]: {SilkMarshal.PtrToString((nint)msg.PDescription)}"));
            }
        }

        vertexBuffer = new(device, vertices, 3, 0);
        indexBuffer = new(device, indices, BindFlag.IndexBuffer);

        shader = new Shader("assets/d3d11testing.shader");
        shader.Load();

        shader.Compile(compiler);
        shader.Create(device);

        drawObject = new(vertexBuffer, indexBuffer, shader, D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
        drawObjects.Add(drawObject);
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

    enum DXGI_ERROR : uint {
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
            catch (COMException e) {
                // device removed error probably
                // .. see <https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11device-getdeviceremovedreason>
                // ..     <https://learn.microsoft.com/en-us/windows/win32/direct3ddxgi/dxgi-error>
                Log.Error($"probably device removed: {(DXGI_ERROR)device.GetDeviceRemovedReason()}");
                return;
            }
            // clear backbuffer
            deviceContext.ClearRenderTargetView(renderTargetView, ref backgroundColour[0]);

            // use the current viewport
            var viewport = new Viewport(0, 0, Window.FramebufferSize.X, Window.FramebufferSize.Y, 0, 1);
            deviceContext.RSSetViewports(1, in viewport);

            deviceContext.OMSetRenderTargets(1, ref renderTargetView, ref nullref<ID3D11DepthStencilView>());

            foreach (DrawObject o in drawObjects) {
                o.Draw(deviceContext);
            }

            swapchain.Present(1, 0);

            renderTargetView.Dispose();
        }
    }

    public D3D11Renderer(IWindow window) {
        Instance = this;

        Window = window;
    }
}
