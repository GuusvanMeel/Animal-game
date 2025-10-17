using Godot;

public static class GridManager
{
    public static AStarGrid2D Grid { get; private set; }
    public static int TileSize;

    public static void InitializeGrid(Vector2I size, int tileSize, Vector2I offset)
    {
        TileSize = tileSize;
        Grid = new AStarGrid2D();   
        Grid.Region = new Rect2I(offset, size);
        Grid.CellSize = new Vector2(tileSize, tileSize);
        Grid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles;
        Grid.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Octile;
        Grid.Update();
         for (int x = 0; x < size.X; x++)
    {
        for (int y = 0; y < size.Y; y++)
        {
            Grid.SetPointSolid(new Vector2I(x, y), false);
        }
    }

        Grid.Update();
    }
    public static Vector2I ToCell(Vector2 worldPos, int cellSize) //changes a position to a cell in the grid
    {
        return new Vector2I(
            Mathf.FloorToInt(worldPos.X / cellSize),
            Mathf.FloorToInt(worldPos.Y / cellSize)
        );
    }
    public static Vector2 CellToWorldCenter(Vector2I cell) //changes a cell to an actual tile inside the grid.
    {
        return (Vector2)cell * TileSize + new Vector2(TileSize / 2f, TileSize / 2f);
    }
public static bool IsObstacle(Vector2I cell)
    {
        return !Grid.IsPointSolid(cell);
    }
}
