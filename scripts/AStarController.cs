using Godot;

public static class GridManager
{
    public static AStarGrid2D Grid { get; private set; }
    public static int TileSize;

    public static void InitializeGrid(Vector2I size, int tileSize)
    {
        TileSize = tileSize;
        Grid = new AStarGrid2D();   
        Grid.Region = new Rect2I(Vector2I.Zero, size);
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
   public static Vector2I ToCell(Vector2 worldPos, int cellSize)
{
    return new Vector2I(
        Mathf.FloorToInt(worldPos.X / cellSize),
        Mathf.FloorToInt(worldPos.Y / cellSize)
    );
}
}
