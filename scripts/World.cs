using Godot;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public partial class World : Node
{   
    public Mob AssignMobToObstacle(BreakableObstacle target)
    {

        Mob nearest = null;
        float bestDist = float.MaxValue;

        foreach (var mob in GetTree().GetNodesInGroup("mobs"))
        {
            if (mob is Mob m && (m.CanBreakType & target.RequiredBreakType) != 0 && m.isBusy == false) // custom bool property
            {
                float dist = m.GlobalPosition.DistanceTo(target.GlobalPosition);
                if (dist < bestDist)
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
    
}
