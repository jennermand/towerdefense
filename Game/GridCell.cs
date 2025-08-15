using VectorTD.Core;

namespace VectorTD.Game;

public enum CellType
{
    Empty,
    Wall,
    Tower,
    Path
}

public class GridCell
{
    public CellType Type { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 WorldPosition { get; set; }
    public bool IsWalkable => Type == CellType.Empty || Type == CellType.Path;
    public Tower? Tower { get; set; }

    public GridCell(int x, int y, float cellSize)
    {
        Position = new Vector2(x, y);
        WorldPosition = new Vector2(x * cellSize + cellSize * 0.5f, y * cellSize + cellSize * 0.5f);
        Type = CellType.Empty;
    }
}
