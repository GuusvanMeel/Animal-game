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

    public override void _EnterTree()
    {   
        TileMapLayer tilemap = GetNode<TileMapLayer>("NavigationRegion2D/Ground");
        GridManager.InitializeGrid(new Vector2I(200, 200), (Vector2I)(tilemap.Position / 16f));
        GridManager.Grid.Update();
        // foreach (BreakableObstacle obstacle in GetTree().GetNodesInGroup("Obstacles"))
        // {
        //     Console.WriteLine("Here");  
            // obstacle.RegisterOnGrid();
        // }
       
    }

    public Mob AssignMobToObstacle(BreakableObstacle target)
    {
        Mob nearest = null;
        int bestDist = int.MaxValue;
        
        foreach (var mob in GetTree().GetNodesInGroup("mobs")) //get all the mobs in the scene.
        {
            if (mob is Mob m && (m.CanBreakType & target.RequiredBreakType) != 0 && m.WalkingToTarget == false) // Check if the mob has correct type, and if its occupied
            {
                Vector2I mobCell = GridManager.ToCell(m.GlobalPosition);
                Vector2I targetCell = GridManager.ToCell(target.GlobalPosition); //transform mob and targetlocations into vector2I to be used in an astargrid

                var path = GridManager.Grid.GetPointPath(mobCell, targetCell);

                int dist = path.Length;
                if (dist < bestDist) //cycles through all the mobs, lowest distance mob gets the job.
                {
                    bestDist = dist;
                    nearest = m;
                }
            }
        }

        if (nearest != null)
        {
            GD.Print($"Assigning {nearest.Name} to break {target.Name}");
            bool succes = nearest.GoToWork(target);
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
    public void AssignMobToNest(Mob mob)
    {

        foreach (Nest nest in GridManager.nests)
        {
            if (nest.CanAccept(mob))
            {
                nest.AssignMob(mob);
                
                break;
            }
        }
        
    }
   

    
}
