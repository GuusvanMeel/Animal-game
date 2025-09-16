using Godot;
using System;

public partial class World : Node
{
    public Mob AssignMobToSlamObstacle(SlamObstacle target)
    {

        Mob nearest = null;
        float bestDist = float.MaxValue;

        foreach (var mob in GetTree().GetNodesInGroup("mobs"))
        {
            if (mob is Mob m && m.CanSlam && m.isBusy == false) // custom bool property
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
            nearest.GoToWork(target);

        }
        return nearest;
    }
    public void DismissMobFromSlamObstacle(SlamObstacle obstacle)
    {
        obstacle.assignedMob.WorkDismissed();
    }
}
