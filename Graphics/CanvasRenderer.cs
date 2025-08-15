using Microsoft.JSInterop;
using VectorTD.Core;

namespace VectorTD.Graphics;

public class DrawCommand
{
    public string Type { get; set; } = "";
    public object[] Parameters { get; set; } = Array.Empty<object>();
}

public class CanvasRenderer : IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string _canvasId;
    private int _viewportWidth;
    private int _viewportHeight;
    private List<DrawCommand> _drawCommands = new();

    public CanvasRenderer(IJSRuntime jsRuntime, string canvasId)
    {
        _jsRuntime = jsRuntime;
        _canvasId = canvasId;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("CanvasRenderer.initialize", _canvasId);
            Console.WriteLine("Canvas renderer initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize canvas renderer: {ex.Message}");
            throw;
        }
    }

    public void SetViewport(int width, int height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
    }

    public void Clear(Color color)
    {
        _drawCommands.Clear();
        _drawCommands.Add(new DrawCommand
        {
            Type = "clear",
            Parameters = new object[] { color.R, color.G, color.B, color.A }
        });
    }

    public void BeginFrame()
    {
        _drawCommands.Clear();
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1.0f)
    {
        _drawCommands.Add(new DrawCommand
        {
            Type = "drawLine",
            Parameters = new object[] { start.X, start.Y, end.X, end.Y, color.R, color.G, color.B, color.A, thickness }
        });
    }

    public void DrawRectangle(Vector2 position, Vector2 size, Color color)
    {
        _drawCommands.Add(new DrawCommand
        {
            Type = "drawRectangle",
            Parameters = new object[] { position.X, position.Y, size.X, size.Y, color.R, color.G, color.B, color.A }
        });
    }

    public void DrawCircle(Vector2 center, float radius, Color color, int segments = 32)
    {
        _drawCommands.Add(new DrawCommand
        {
            Type = "drawCircle",
            Parameters = new object[] { center.X, center.Y, radius, color.R, color.G, color.B, color.A }
        });
    }

    public async Task EndFrame()
    {
        if (_drawCommands.Count > 0)
        {
            await _jsRuntime.InvokeVoidAsync("CanvasRenderer.renderFrame", _drawCommands.ToArray());
        }
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
