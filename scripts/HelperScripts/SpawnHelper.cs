
using Godot;

public static class SpawnHelper
{
    public static Vector2I GetRandomWalkableCellInCamera(Camera2D camera)
    {   
        
                float randomFractionY = (float)GD.RandRange(0.1, 0.9); // easier with Godot’s RNG
                float randomFractionX = (float)GD.RandRange(0.1, 0.9); // easier with Godot’s RNG
                Vector2 screenPosToSpawn = new Vector2(
                    camera.GetViewport().GetVisibleRect().Size.X * randomFractionX,
                    camera.GetViewport().GetVisibleRect().Size.Y * randomFractionY
                );
                var worldPos = camera.GetCanvasTransform().AffineInverse() * screenPosToSpawn;
                Vector2I cell  = GridManager.ToCell(worldPos, GridManager.TileSize);

            if (GridManager.IsObstacle(cell))
                return cell;
        

        return Vector2I.Zero;
    }
}