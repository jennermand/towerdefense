using VectorTD.Core;
using VectorTD.Graphics;

namespace VectorTD.Game;

public class GameGrid
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize { get; private set; }
    
    private GridCell[,] _cells;
    private List<Vector2> _pathPoints;

    public GameGrid(int width, int height, float cellSize)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        _cells = new GridCell[width, height];
        _pathPoints = new List<Vector2>();

        InitializeGrid();
        CreateDefaultLevel();
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _cells[x, y] = new GridCell(x, y, CellSize);
            }
        }
    }

    private void CreateDefaultLevel()
    {
        // Create walls to form a path (classic Vector-TD style)
        // Top and bottom borders
        for (int x = 0; x < Width; x++)
        {
            SetCellType(x, 0, CellType.Wall);
            SetCellType(x, Height - 1, CellType.Wall);
        }

        // Left and right borders
        for (int y = 0; y < Height; y++)
        {
            SetCellType(0, y, CellType.Wall);
            SetCellType(Width - 1, y, CellType.Wall);
        }

        // Create internal walls to form a winding path
        // Horizontal walls
        for (int x = 2; x < Width - 2; x++)
        {
            if (x != 5 && x != 10 && x != 15) // Leave gaps for path
            {
                SetCellType(x, 3, CellType.Wall);
                SetCellType(x, Height - 4, CellType.Wall);
            }
        }

        // Vertical walls
        for (int y = 3; y < Height - 3; y++)
        {
            if (y != 6 && y != 9) // Leave gaps for path
            {
                SetCellType(5, y, CellType.Wall);
                SetCellType(Width - 6, y, CellType.Wall);
            }
        }

        // Generate path points for enemy movement
        GeneratePathPoints();
    }

    private void GeneratePathPoints()
    {
        _pathPoints.Clear();
        
        // Define a simple path through the level
        _pathPoints.Add(new Vector2(1 * CellSize + CellSize * 0.5f, 1 * CellSize + CellSize * 0.5f)); // Start
        _pathPoints.Add(new Vector2(5 * CellSize + CellSize * 0.5f, 1 * CellSize + CellSize * 0.5f));
        _pathPoints.Add(new Vector2(5 * CellSize + CellSize * 0.5f, 6 * CellSize + CellSize * 0.5f));
        _pathPoints.Add(new Vector2(10 * CellSize + CellSize * 0.5f, 6 * CellSize + CellSize * 0.5f));
        _pathPoints.Add(new Vector2(10 * CellSize + CellSize * 0.5f, 9 * CellSize + CellSize * 0.5f));
        _pathPoints.Add(new Vector2(15 * CellSize + CellSize * 0.5f, 9 * CellSize + CellSize * 0.5f));
        _pathPoints.Add(new Vector2(15 * CellSize + CellSize * 0.5f, (Height - 2) * CellSize + CellSize * 0.5f)); // End
    }

    public void SetCellType(int x, int y, CellType type)
    {
        if (IsValidPosition(x, y))
        {
            _cells[x, y].Type = type;
        }
    }

    public GridCell? GetCell(int x, int y)
    {
        return IsValidPosition(x, y) ? _cells[x, y] : null;
    }

    public GridCell? GetCellAtWorldPosition(Vector2 worldPos)
    {
        int x = (int)(worldPos.X / CellSize);
        int y = (int)(worldPos.Y / CellSize);
        return GetCell(x, y);
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public bool CanPlaceTower(Vector2 worldPos)
    {
        var cell = GetCellAtWorldPosition(worldPos);
        return cell != null && cell.Type == CellType.Empty && cell.Tower == null;
    }

    public bool PlaceTower(Vector2 worldPos, Tower tower)
    {
        var cell = GetCellAtWorldPosition(worldPos);
        if (cell != null && CanPlaceTower(worldPos))
        {
            cell.Type = CellType.Tower;
            cell.Tower = tower;
            tower.Position = cell.WorldPosition;
            return true;
        }
        return false;
    }

    public Tower? GetTowerAt(Vector2 worldPos)
    {
        var cell = GetCellAtWorldPosition(worldPos);
        return cell?.Tower;
    }

    public List<Vector2> GetPathPoints()
    {
        return new List<Vector2>(_pathPoints);
    }

    public void Render(CanvasRenderer renderer)
    {
        // Draw grid lines (subtle)
        var gridColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        for (int x = 0; x <= Width; x++)
        {
            renderer.DrawLine(
                new Vector2(x * CellSize, 0),
                new Vector2(x * CellSize, Height * CellSize),
                gridColor, 1.0f);
        }

        for (int y = 0; y <= Height; y++)
        {
            renderer.DrawLine(
                new Vector2(0, y * CellSize),
                new Vector2(Width * CellSize, y * CellSize),
                gridColor, 1.0f);
        }

        // Draw walls
        var wallColor = Color.White;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                var cell = _cells[x, y];
                if (cell.Type == CellType.Wall)
                {
                    renderer.DrawRectangle(
                        new Vector2(x * CellSize, y * CellSize),
                        new Vector2(CellSize, CellSize),
                        wallColor);
                }
            }
        }

        // Draw path (for debugging)
        var pathColor = new Color(0.2f, 0.2f, 0.8f, 0.3f);
        for (int i = 0; i < _pathPoints.Count - 1; i++)
        {
            renderer.DrawLine(_pathPoints[i], _pathPoints[i + 1], pathColor, 3.0f);
        }
    }
}
