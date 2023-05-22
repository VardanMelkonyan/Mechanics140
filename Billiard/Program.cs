using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace mechanics;

internal class Game : GameWindow
{
    // A simple vertex shader possible. Just passes through the position vector.
    private const string VertexShaderSource = @"
            #version 330
            layout(location = 0) in vec3 position;
            layout(location = 1) in vec2 texcoord;

            out vec2 v_texCoord;

            void main(void)
            {
                gl_Position = vec4(position, 1.0);
                v_texCoord = texcoord;
            }
        ";

    // A simple fragment shader. Just a constant red color.
    private const string FragmentShaderSource = @"
            #version 330
            out vec4 outputColor;
            in vec2 v_texCoord;
            uniform sampler2D texture0;
            void main(void)
            {
                outputColor = texture(texture0, v_texCoord);
            }
        ";

    // Points of a triangle in normalized device coordinates.
    private readonly float[] Points =
    {
        -1, 1, 0, 0, 0,
        -1, -1, 0, 0, 1,
        1, -1, 0, 1, 1,
        -1, 1, 0, 0, 0,
        1, 1, 0, 1, 0,
        1, -1, 0, 1, 1
    };

    private Utils.Circle circle;

    private ExpressionHandler expressionHandler;
    private int FragmentShader;

    private List<Vector2> interceptionPoints;

    private double lastUpdate;
    private Vector2 lastPoint;
    private Vector2 lastDirection;
    
    private int ShaderProgram;
    private Surface surface;
    private double timer;
    private int VertexArrayObject;
    private int VertexBufferObject;

    private int VertexShader;

    public Game(NativeWindowSettings settings) : base(GameWindowSettings.Default, settings)
    {
    }

    private static void Main(string[] args)
    {
        var settings = NativeWindowSettings.Default;

        settings.Profile = ContextProfile.Core;
        settings.APIVersion = new Version(4, 1);
        settings.Flags |= ContextFlags.ForwardCompatible;
        settings.Title = "";
        using (var game = new Game(settings))
        {
            game.Run();
        }
    }

    protected override void OnLoad()
    {
        // Load the source of the vertex shader and compile it.
        VertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(VertexShader, VertexShaderSource);
        GL.CompileShader(VertexShader);

        // Load the source of the fragment shader and compile it.
        FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(FragmentShader, FragmentShaderSource);
        GL.CompileShader(FragmentShader);

        // Create the shader program, attach the vertex and fragment shaders and link the program.
        ShaderProgram = GL.CreateProgram();
        GL.AttachShader(ShaderProgram, VertexShader);
        GL.AttachShader(ShaderProgram, FragmentShader);
        GL.LinkProgram(ShaderProgram);

        // Create the vertex buffer object (VBO) for the vertex data.
        VertexBufferObject = GL.GenBuffer();
        // Bind the VBO and copy the vertex data into it.
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, Points.Length * sizeof(float), Points, BufferUsageHint.StaticDraw);

        // Retrive the position location from the program.
        var positionLocation = GL.GetAttribLocation(ShaderProgram, "position");
        var texCoordLocation = GL.GetAttribLocation(ShaderProgram, "texcoord");

        // Create the vertex array object (VAO) for the program.
        VertexArrayObject = GL.GenVertexArray();
        // Bind the VAO and setup the position attribute.
        GL.BindVertexArray(VertexArrayObject);
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float),
            3 * sizeof(float));
        GL.EnableVertexAttribArray(positionLocation);
        GL.EnableVertexAttribArray(texCoordLocation);


        // Set the clear color to blue
        GL.ClearColor(0.0f, 0.0f, 1.0f, 0.0f);

        surface = new Surface(ClientSize.X, ClientSize.Y);
        interceptionPoints = new List<Vector2>();
        expressionHandler = new ExpressionHandler(surface);
        circle = new Utils.Circle(surface.width / 2, surface.height / 2, surface.height * 40 / 100,
            surface.width * 30 / 100);
        expressionHandler.addExpression(new Utils.OuterCircle(circle));
        expressionHandler.addExpression(circle);
        // var origin = Utils.RandomPoint(surface);

        lastPoint = Utils.RandomPoint(surface);
        Vector2 direction = Utils.RandomPoint(surface);
        Vector2 nextInterception = Utils.CalculateInterceptionPoint(circle, lastPoint, direction);
        lastPoint = nextInterception;
        lastDirection = direction;
        Console.WriteLine(nextInterception);
        interceptionPoints.Add(nextInterception);

        expressionHandler.addExpression(new Utils.LineSegment(lastPoint, lastPoint - lastDirection));

        base.OnLoad();
    }

    protected override void OnUnload()
    {
        // Unbind all the resources by binding the targets to 0/null.
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        // Delete all the resources.
        GL.DeleteBuffer(VertexBufferObject);
        GL.DeleteVertexArray(VertexArrayObject);
        GL.DeleteProgram(ShaderProgram);
        GL.DeleteShader(FragmentShader);
        GL.DeleteShader(VertexShader);

        base.OnUnload();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        surface.Resize(e.Width, e.Height);
        Console.WriteLine("Resized {0}, {1}", e.Width, e.Height);
        GL.Viewport(0, 0, e.Width, e.Height);
        base.OnResize(e);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        // Clear the color buffer.
        GL.Clear(ClearBufferMask.ColorBufferBit);
        expressionHandler.Render();
        surface.UpdateTexture();
        GL.BindTexture(TextureTarget.Texture2D, surface.textureId);
        // Bind the VBO
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
        // Bind the VAO
        GL.BindVertexArray(VertexArrayObject);
        // Use/Bind the program
        GL.UseProgram(ShaderProgram);
        // This draws the triangle.

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        // Swap the front/back buffers so what we just rendered to the back buffer is displayed in the window.
        Context.SwapBuffers();
        base.OnRenderFrame(e);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        timer += e.Time;
        if (timer - lastUpdate >= 3.0)
        {
            lastDirection = Utils.ReflectVector(lastDirection, circle, lastPoint);
            lastPoint = Utils.CalculateInterceptionPoint(circle, lastPoint, lastDirection);
            expressionHandler.addExpression(new Utils.LineSegment(lastPoint, lastPoint - lastDirection));
            lastUpdate = timer;
        }
    }
}

public class Surface
{
    public int[] pixels;
    public int textureId;
    public int width, height;

    public Surface(int w, int h)
    {
        Resize(w, h);
    }

    public void GenTexture()
    {
        textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra,
            PixelType.UnsignedByte, pixels);
    }

    public void Resize(int w, int h)
    {
        if (pixels != null) GL.DeleteTextures(1, ref textureId);
        width = w;
        height = h;
        pixels = new int[w * h];
        GenTexture();
    }

    public void UpdateTexture()
    {
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Bgra,
            PixelType.UnsignedByte, pixels);
    }

    public void SetPixel(int x, int y, int c)
    {
        pixels[x + y * width] = c;
    }

    // Byte values in the range [0, 255].
    public void SetPixel(int x, int y, byte r, byte g, byte b)
    {
        SetPixel(x, y, (r << 16) | (g << 8) | b);
    }

    // Float values in the range [0, 1].
    public void SetPixel(int x, int y, float r, float g, float b)
    {
        SetPixel(x, y,
            ((int)(Math.Clamp(r, 0, 1) * 255) << 16) | ((int)(Math.Clamp(g, 0, 1) * 255) << 8) |
            (int)(Math.Clamp(b, 0, 1) * 255));
    }

    public void SetPixel(int x, int y, Vector3 color)
    {
        SetPixel(x, y, color.X, color.Y, color.Z);
    }
}