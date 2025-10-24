using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

public partial class World : Node
{
    [Export] PackedScene mobScene;
    [Export] private Camera2D camera;
        private static readonly Vector2I[] SurroundOffsets = new Vector2I[]
{
    new Vector2I(-1, -1), new Vector2I(0, -1), new Vector2I(1, -1),
    new Vector2I(-1,  0),                     new Vector2I(1,  0),
    new Vector2I(-1,  1), new Vector2I(0,  1), new Vector2I(1,  1),
};

    public override void _EnterTree()
    {   
        TileMapLayer tilemap = GetNode<TileMapLayer>("NavigationRegion2D/Ground");
        GridManager.InitializeGrid(new Vector2I(200, 200), (Vector2I)(tilemap.Position / 16f));
        GridManager.Grid.Update();
    }

   public Mob AssignMobToObstacle(BreakableObstacle target)
{
    Mob nearest = null;
    float bestDist = float.MaxValue;

    Vector2I targetCell = GridManager.ToCell(target.GlobalPosition);

    foreach (var mobNode in GetTree().GetNodesInGroup("mobs"))
    {
        if (mobNode is Mob m && (m.CanBreakType & target.RequiredBreakType) != 0 && !m.WalkingToTarget)
        {
            Vector2I mobCell = GridManager.ToCell(m.GlobalPosition);
            Vector2[] bestPath = null;

            // 🔹 try to find a walkable neighbor near the obstacle
            foreach (var offset in SurroundOffsets)
            {
                Vector2I neighborCell = targetCell + offset;
                if (GridManager.Grid.IsPointSolid(neighborCell))
                    continue;

                Vector2[] path = GridManager.Grid.GetPointPath(mobCell, neighborCell);
                if (path.Length == 0)
                    continue;

                float totalDist = GridManager.GetPathLength(path);
                if (bestPath == null || totalDist < GridManager.GetPathLength(bestPath))
                    bestPath = path;
            }

            if (bestPath != null)
            {
                float totalDist = GridManager.GetPathLength(bestPath);
                if (totalDist < bestDist)
                {
                    bestDist = totalDist;
                    nearest = m;
                }
            }
        }
    }

        if (nearest != null)
        {
            GD.Print($"Assigning {nearest.Name} to break {target.Name}");
            bool succes = nearest.GoToTarget(target);
            if (succes == false)
            {
                GD.Print("Nearest couldnt find a path");
                return null;
            }
        }
        return nearest;
    }
    public void DismissMobFromObstacle(BreakableObstacle obstacle)
    {
        obstacle.assignedMob.WorkDismissed();
    }
    public void SpawnMob()
    {
        var mob = mobScene.Instantiate<Mob>();
        AddChild(mob);
        mob.GlobalPosition = GridManager.CellToWorldCenter(SpawnHelper.GetRandomWalkableCellInCamera(camera));
    }
    public Nest AssignMobToNest(Mob mob)
    {

        foreach (Nest nest in GridManager.nests)
        {
            if (nest.CanAccept(mob))
            {
                return nest.AssignMob(mob);

            }
        }
        return null;
        
    }
   

    
}
