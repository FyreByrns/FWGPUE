using Silk.NET.Windowing;
using Silk.NET.Maths;
using FWGPUE.IO;
using System.Numerics;

using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Direct3D.Compilers;

using FWGPUE.Graphics;
using FWGPUE.Scenes;
using Silk.NET.SDL;
using Silk.NET.Core.Native;
using System.Runtime.CompilerServices;
using System.Text;

namespace FWGPUE;

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

    uint vertexStride = 3U * sizeof(float);
    uint vertexOffset = 0U;

    const string shaderSource = @"
struct vs_in {
    float3 position_local : POS;
};
struct vs_out {
    float4 position_clip : SV_POSITION;
};
vs_out vs_main(vs_in input) {
    vs_out output = (vs_out)0;
    output.position_clip = float4(input.position_local, 1.0);
    return output;
}
float4 ps_main(vs_out input) : SV_TARGET {
    return float4( 1.0, 0.5, 0.2, 1.0 );
}
";

    float[] backgroundColour = new[] { 0.0f, 0.0f, 0.0f, 1.0f };

    public static D3D11Renderer Instance { get; private set; }

    public IWindow Window { get; private set; }

    public DXGI dxgi = DXGI.GetApi();
    public D3D11 d3d11 = D3D11.GetApi();
    public D3DCompiler compiler = D3DCompiler.GetApi();

    ComPtr<IDXGIFactory2> factory = default;
    ComPtr<IDXGISwapChain1> swapchain = default;
    ComPtr<ID3D11Device> device = default;
    ComPtr<ID3D11DeviceContext> deviceContext = default;
    ComPtr<ID3D11Buffer> vertexBuffer = default;
    ComPtr<ID3D11Buffer> indexBuffer = default;
    ComPtr<ID3D11VertexShader> vertexShader = default;
    ComPtr<ID3D11PixelShader> pixelShader = default;
    ComPtr<ID3D11InputLayout> inputLayout = default;

    public void Setup() {
        CreateDevice();
        CreateSwapchain();

        #region buffers
        BufferDesc bufferDescription = new() {
            ByteWidth = (uint)(vertices.Length * sizeof(float)),
            Usage = Usage.Default,
            BindFlags = (uint)BindFlag.VertexBuffer
        };
        unsafe {
            fixed (float* vertexData = vertices) {
                SubresourceData subData = new() {
                    PSysMem = vertexData
                };
                SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDescription, in subData, ref vertexBuffer));
            }
        }

        bufferDescription = new() {
            ByteWidth = (uint)(indices.Length * sizeof(uint)),
            Usage = Usage.Default,
            BindFlags = (uint)BindFlag.IndexBuffer
        };
        unsafe {
            fixed (uint* indexData = indices) {
                SubresourceData subData = new() {
                    PSysMem = indexData
                };
                SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDescription, in subData, ref indexBuffer));
            }
        }
        #endregion buffers
        #region shader 
        byte[] shaderBytes = Encoding.ASCII.GetBytes(shaderSource);

        // vertex shader
        ComPtr<ID3D10Blob> vertexCode = default;
        ComPtr<ID3D10Blob> vertexErrors = default;
        unsafe {
            HResult result = compiler.Compile(
                in shaderBytes[0],
                (nuint)shaderBytes.Length,
                nameof(shaderSource),
                null,
                ref Unsafe.NullRef<ID3DInclude>(),
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

        // pixel shader
        ComPtr<ID3D10Blob> pixelCode = default;
        ComPtr<ID3D10Blob> pixelErrors = default;
        unsafe {
            HResult result = compiler.Compile(
                in shaderBytes[0],
                (nuint)shaderBytes.Length,
                nameof(shaderSource),
                null,
                ref Unsafe.NullRef<ID3DInclude>(),
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

        // create shaders
        unsafe {
            SilkMarshal.ThrowHResult(device.CreateVertexShader(
                vertexCode.GetBufferPointer(),
                vertexCode.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref vertexShader
            ));
            SilkMarshal.ThrowHResult(device.CreatePixelShader(
                pixelCode.GetBufferPointer(),
                pixelCode.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref pixelShader
            ));
        }

        // describe shader input data layout
        unsafe {
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
        #endregion shader

        vertexCode.Dispose();
        vertexErrors.Dispose();
        pixelCode.Dispose();
        pixelErrors.Dispose();
    }
    void CreateDevice() {
        unsafe {
            SilkMarshal.ThrowHResult(d3d11.CreateDevice(
                default(ComPtr<IDXGIAdapter>),
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
                ref Unsafe.NullRef<IDXGIOutput>(),
                ref swapchain
            ));
        }
    }

    public void FramebufferResize(Vector2D<int> newSize) {
        SilkMarshal.ThrowHResult(swapchain.ResizeBuffers(0, (uint)newSize.X, (uint)newSize.Y, Format.FormatR8G8B8A8Unorm, 0));
    }

    public void Render() {
        unsafe {
            // Obtain the framebuffer for the swapchain's backbuffer.
            using var framebuffer = swapchain.GetBuffer<ID3D11Texture2D>(0);

            // Create a view over the render target.
            ComPtr<ID3D11RenderTargetView> renderTargetView = default;
            SilkMarshal.ThrowHResult(device.CreateRenderTargetView(framebuffer, null, ref renderTargetView));

            // Clear the render target to be all black ahead of rendering.
            deviceContext.ClearRenderTargetView(renderTargetView, ref backgroundColour[0]);

            // Update the rasterizer state with the current viewport.
            var viewport = new Viewport(0, 0, Window.FramebufferSize.X, Window.FramebufferSize.Y, 0, 1);
            deviceContext.RSSetViewports(1, in viewport);

            // Tell the output merger about our render target view.
            deviceContext.OMSetRenderTargets(1, ref renderTargetView, ref Unsafe.NullRef<ID3D11DepthStencilView>());

            // Update the input assembler to use our shader input layout, and associated vertex & index buffers.
            deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
            deviceContext.IASetInputLayout(inputLayout);
            deviceContext.IASetVertexBuffers(0, 1, vertexBuffer, in vertexStride, in vertexOffset);
            deviceContext.IASetIndexBuffer(indexBuffer, Format.FormatR32Uint, 0);

            // Bind our shaders.
            deviceContext.VSSetShader(vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            deviceContext.PSSetShader(pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);

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

        Setup();
    }
}

static class Engine {
    #region timing

    public static float TotalSeconds { get; private set; } = 0;
    public static float LastFrameTime { get; private set; }
    public static float TickTimer { get; private set; }
    public static float TickTime => 1f / Config.TickRate;

    #endregion timing

    #region scene management

    public static Scene? CurrentScene { get; private set; }
    public static Scene? NextScene { get; private set; }
    public static bool WaitingToChangeScenes { get; private set; }

    public static void ChangeToScene(Scene? scene) {
        WaitingToChangeScenes = true;
        NextScene = scene;
    }

    #endregion scene management

    #region rendering

    public static D3D11Renderer Renderer { get; private set; }

    public static BaseCamera Camera;

    public static IWindow? Window { get; private set; }

    public enum TextAlignment {
        None = 0,

        TopLeft,
        MiddleLeft,
        BottomLeft,

        TopMiddle,
        MiddleMiddle,
        BottomMiddle,

        TopRight,
        MiddleRight,
        BottomRight,

        Normal = TopLeft,
        Center = MiddleMiddle,
    }
    public record TextDrawData(string text, Vector2 location, TextColour colour, float size, Vector2 scale, float rotation, TextAlignment alignment);
    public static HashSet<TextDrawData> TextThisFrame = new();

    public static void DrawText(string text, Vector2 location, TextColour colour, float size = 10, TextAlignment alignment = TextAlignment.Normal) {
        TextThisFrame.Add(new(text, location, colour, size, new(1, 1), 0, alignment));
    }
    public static void DrawTextRotated(string text, Vector2 location, float rotation, TextColour colour, float size = 10, TextAlignment alignment = TextAlignment.Normal) {
        TextThisFrame.Add(new(text, location, colour, size, new(1, 1), rotation, alignment));
    }

    /// <summary>
    /// Draw an image from the current scene's atlas.
    /// </summary>
    public static void DrawImage(string name, Vector2 location, float z = 0, float size = 1, float rotation = 0) {
    }

    public record VertexDrawData(Vector3 colour, Vector3 position);
    public static List<VertexDrawData> VerticesThisFrame = new();

    public static void DrawTriangle(Vector3 colour, Vector2 a, Vector2 b, Vector2 c) {
        VerticesThisFrame.Add(new VertexDrawData(colour, new(a, 1)));
        VerticesThisFrame.Add(new VertexDrawData(colour, new(b, 1)));
        VerticesThisFrame.Add(new VertexDrawData(colour, new(c, 1)));
    }
    public static void DrawLine(Vector3 colour, Vector2 start, Vector2 end, float thickness = 0.5f) {
        float angleBetween = RadiansToTurns((float)Math.Atan2(start.Y - end.Y, start.X - end.X));
        float anglePlusHalf = angleBetween + 0.5f;

        thickness /= 2;

        Vector2 a = start.Along(+thickness, anglePlusHalf);
        Vector2 b = start.Along(-thickness, anglePlusHalf);
        Vector2 c = end.Along(+thickness, anglePlusHalf);
        Vector2 d = end.Along(-thickness, anglePlusHalf);

        DrawTriangle(colour, a, c, b);
        DrawTriangle(colour, b, c, d);
    }
    public static void DrawCircle(Vector3 colour, Vector2 location, float radius) {
        const int vertices = 10;
        const float turnsPer = 1f / vertices;
        Vector2[] points = new Vector2[vertices];

        for (int i = 0; i < vertices; i++) {
            points[i] = location.Along(radius, turnsPer);
        }

        for (int i = 0; i < vertices; i++) {
            int next = i + 1;
            if (next >= vertices) {
                next = 0;
            }

            DrawLine(colour, points[i], points[next], 4);
        }
    }

    #endregion rendering

    #region initialization

    static void Start() {
        Init();
        MainLoop();
        End();
    }

    static void Init() {
        InitWindow();
        Input.Init();
        InitGraphics();

        ChangeToScene(new StartupSplash());
    }

    static void FramebufferResize(Vector2D<int> obj) {
        D3D11Renderer.Instance.FramebufferResize(obj);
    }

    static void InitWindow() {
        WindowOptions options = WindowOptions.Default with {
            Size = new Vector2D<int>(Config.ScreenWidth, Config.ScreenHeight),
            Title = "FWGPUE",
            API = GraphicsAPI.None,
        };

        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Load += Load;
        Window.Update += Update;
        Window.Render += Render;
        Window.Closing += Closing;

        Window.FramebufferResize += FramebufferResize;

        Window.Initialize();
    }

    static void InitGraphics() {

    }

    #endregion initialization

    #region engine meta-state

    public static bool ShutdownComplete { get; private set; } = false;

    static void MainLoop() {
        Window!.Run();
    }

    static void Closing() { }

    static void End() {
        ShutdownComplete = true;
    }

    static void Load() {
        Renderer = new D3D11Renderer(Window!);
        Renderer.Setup();
    }

    #endregion engine meta-state

    #region per-frame

    private static void Update(double elapsed) {
        LastFrameTime = (float)elapsed;
        TotalSeconds += LastFrameTime;
        TickTimer += LastFrameTime;

        Input.Update((float)elapsed);

        Log.Inane(TickTimer);
        while (TickTimer > TickTime) {
            Tick();
            TickTimer -= TickTime;
        }
    }

    public static void Tick() {
        if (CurrentScene == null && !WaitingToChangeScenes) {
            Window!.Close();
        }

        CurrentScene?.Tick();

        if (WaitingToChangeScenes) {
            CurrentScene?.Unload();
            CurrentScene = NextScene;
            CurrentScene?.Load();
            WaitingToChangeScenes = false;
        }
    }

    private static void Render(double elapsed) {
        // update ImGui

        // clear backbuffer

        // render the current frame
        CurrentScene?.Render();

        // draw all batched sprites

        // draw all batched fonts

        // draw all batched raw geometry

        // render ImGui
    }

    #endregion per-frame    

    public static void Begin() {
        Log.Info("loading config");
        if (Config.Location!.Exists()) {
            Config.Load();
        }
        else {
            Log.Info("writing default config");
            Config.Save();
        }

        Log.Info("starting game");
        try {
            Start();
        }
        finally {
            if (!ShutdownComplete) {
                End();
            }
        }
    }
}
