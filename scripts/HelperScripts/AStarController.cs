using System.Collections.Generic;
using Godot;
using Enums;
public static class GridManager
{
    public static AStarGrid2D Grid { get; private set; }
    public static Dictionary<Vector2I, WorldObject> GridObjects = new();
    public static List<WorldObject> nests = new();
    public static List<WorldObject> obstacles = new();
    public static int TileSize = 16;
    public static void InitializeGrid(Vector2I size, Vector2I offset)
    {

        Grid = new AStarGrid2D
        {
            Region = new Rect2I(offset, size),
            CellSize = new Vector2(TileSize, TileSize),
            DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles,
            DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Octile
        };
        Grid.Update();

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                Grid.SetPointSolid(new Vector2I(x, y), false);
                
            }
        }

        Grid.Update();
                GD.Print($"✅ Grid initialized: Offset={offset}, Size={size}, Region={Grid.Region}");
    }
    
    public static Vector2I ToCell(Vector2 worldPos) //changes a position to a cell in the grid
    {
        Vector2I cell = new Vector2I(
            Mathf.FloorToInt(worldPos.X / TileSize),
            Mathf.FloorToInt(worldPos.Y / TileSize)
        );

        // 🔹 Debug check for invalid cells
        if (!Grid.Region.HasPoint(cell))
        {
            GD.PrintErr($"⚠️ [ToCell] Out of bounds! WorldPos={worldPos}, ResultCell={cell}, Region={Grid.Region}");
        }

        return cell;
        
    }
    public static Vector2 CellToWorldCenter(Vector2I cell) //changes a cell to an actual tile inside the grid.
    {
        return (Vector2)cell * TileSize + new Vector2(TileSize / 2f, TileSize / 2f);
    }
    public static bool IsObstacle(Vector2I cell)
    {
        return !Grid.IsPointSolid(cell);
    }
    public static void RegisterObject(WorldObject obj)
    {
        GridObjects.Add(obj.GridCell, obj);
        if (obj is Nest) nests.Add(obj);
        if (obj is BreakableObstacle) obstacles.Add(obj);
        if (obj.BlocksPath)        
        {
            Grid.SetPointSolid(obj.GridCell, obj.BlocksPath);
        }
    }
    public static Vector2I ClampToBounds(Vector2I cell)
{
    if (Grid == null)
    {
        GD.PrintErr("⚠️ Tried to clamp before grid was initialized!");
        return cell;
    }

    var region = Grid.Region;

    int clampedX = Mathf.Clamp(cell.X, region.Position.X, region.End.X - 1);
    int clampedY = Mathf.Clamp(cell.Y, region.Position.Y, region.End.Y - 1);

    return new Vector2I(clampedX, clampedY);
}
  
}
