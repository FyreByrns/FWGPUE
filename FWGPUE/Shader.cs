using Silk.NET.OpenGL;
using FWGPUE.IO;
using System.Numerics;

namespace FWGPUE;

class Shader : IDisposable {
    public uint Handle { get; }
    public GL Gl { get; }

    uint LoadShader(ShaderType type, string source) {
        uint handle = Gl.CreateShader(type);

        Gl.ShaderSource(handle, source);
        Gl.CompileShader(handle);

        string log = Gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrEmpty(log)) {
            Log.Error($"{type} shader compilation error: {log}");
        }

        return handle;
    }

    public void Use() {
        Gl.UseProgram(Handle);
    }

    public void SetUniform<T>(string name, T value) {
        int location = Gl.GetUniformLocation(Handle, name);
        if (location == -1) {
            Log.Error($"uniform {name} not found in shader");
        }

        if (value is int i) { Gl.Uniform1(location, i); }
        if (value is float f) { Gl.Uniform1(location, f); }
        if (value is double d) { Gl.Uniform1(location, d); }
        unsafe {
            if (value is Matrix4x4 m) { Gl.UniformMatrix4(location, 1, false, (float*)&m); }
            if (value is Vector2 v) { Gl.Uniform2(location, v); }
        }
    }

    public Shader(GL gl, ShaderFile shaderFile) {
        Gl = gl;
        shaderFile.Load();

        // create shaders
        uint vertex = LoadShader(ShaderType.VertexShader, shaderFile.Vertex);
        uint fragment = LoadShader(ShaderType.FragmentShader, shaderFile.Fragment);

        // bind shader program
        Handle = Gl.CreateProgram();
        Gl.AttachShader(Handle, vertex);
        Gl.AttachShader(Handle, fragment);
        Gl.LinkProgram(Handle);
        Gl.GetProgram(Handle, GLEnum.LinkStatus, out var status);
        if (status == 0) {
            Log.Error($"failed to link shader: {Gl.GetProgramInfoLog(Handle)}");
        }

        // clean up
        Gl.DetachShader(Handle, vertex);
        Gl.DetachShader(Handle, fragment);
        Gl.DeleteShader(vertex);
        Gl.DeleteShader(fragment);
    }

    public void Dispose() {
        Gl.DeleteProgram(Handle);
    }
}
