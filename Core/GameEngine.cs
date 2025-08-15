using Microsoft.JSInterop;
using VectorTD.Graphics;
using VectorTD.Game;

namespace VectorTD.Core;

public class GameEngine : IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private CanvasRenderer? _renderer;
    private GameState? _gameState;
    private bool _isRunning;
    private DateTime _lastFrameTime;

    public event Action<float>? OnUpdate;
    public event Action<CanvasRenderer>? OnRender;

    public GameEngine(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _lastFrameTime = DateTime.Now;
    }

    public async Task InitializeAsync(string canvasId)
    {
        try
        {
            // Initialize Canvas 2D renderer
            _renderer = new CanvasRenderer(_jsRuntime, canvasId);
            await _renderer.InitializeAsync();

            // Initialize game state
            _gameState = new GameState();

            Console.WriteLine("Game engine initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize game engine: {ex.Message}");
            throw;
        }
    }

    public void SetViewport(int width, int height)
    {
        _renderer?.SetViewport(width, height);
        _gameState?.SetViewport(width, height);
    }

    public void Start()
    {
        _isRunning = true;
        _lastFrameTime = DateTime.Now;
        _ = Task.Run(GameLoop);
    }

    public void Stop()
    {
        _isRunning = false;
    }

    private async Task GameLoop()
    {
        while (_isRunning)
        {
            var currentTime = DateTime.Now;
            var deltaTime = (float)(currentTime - _lastFrameTime).TotalSeconds;
            _lastFrameTime = currentTime;

            // Cap delta time to prevent large jumps
            deltaTime = Math.Min(deltaTime, 1.0f / 30.0f);

            Update(deltaTime);
            Render();

            // Target 60 FPS
            await Task.Delay(16);
        }
    }

    private void Update(float deltaTime)
    {
        _gameState?.Update(deltaTime);
        OnUpdate?.Invoke(deltaTime);
    }

    private async void Render()
    {
        if (_renderer == null) return;

        _renderer.Clear(new Color(0.05f, 0.05f, 0.05f, 1.0f));
        _renderer.BeginFrame();

        _gameState?.Render(_renderer);
        OnRender?.Invoke(_renderer);

        await _renderer.EndFrame();
    }

    public void HandleMouseClick(float x, float y)
    {
        _gameState?.HandleMouseClick(new Vector2(x, y));
    }

    public void HandleMouseMove(float x, float y)
    {
        _gameState?.HandleMouseMove(new Vector2(x, y));
    }

    public void HandleMouseDown(float x, float y)
    {
        _gameState?.HandleMouseDown(new Vector2(x, y));
    }

    public void HandleMouseUp(float x, float y)
    {
        _gameState?.HandleMouseUp(new Vector2(x, y));
    }

    public GameState? GetGameState() => _gameState;

    public void Dispose()
    {
        Stop();
        _renderer?.Dispose();
    }
}
