using Godot;
using System;
using System.Collections.Generic;
using Enums;

public partial class Mob : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 300f;
    [Export] public float MoveTimeMax { get; set; } = 2f;
    [Export] public float MoveTimeMin { get; set; } = 0.5f;
    [Export] public float IdleTimeMax { get; set; } = 1.6f;
    [Export] public float IdleTimeMin { get; set; } = 1f;
    [Export] public BreakType CanBreakType { get; set; }
    [Export] public Gender gender{ get; set; }


    private BreakableObstacle currentTarget;
    private Node2D skeleton;
    private Timer idleTimer;
    private Timer actionTimer;
    private Vector2 direction = Vector2.Zero;
    private bool isMoving = false;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Sprite2D sprite;

    private Vector2[] currentPath;
    private int pathIndex;
    public bool WalkingToTarget = false;
    private bool IsBreaking = false;

    private static readonly Vector2I[] SurroundOffsets = new Vector2I[]
{
    new Vector2I(-1, -1), new Vector2I(0, -1), new Vector2I(1, -1),
    new Vector2I(-1,  0),                     new Vector2I(1,  0),
    new Vector2I(-1,  1), new Vector2I(0,  1), new Vector2I(1,  1),
};
    private bool clicked = false;
    public override void _Ready()
    {


        sprite = GetNode<Sprite2D>("Node2D/TorsoSprite");
        rng.Randomize();
        idleTimer = GetNode<Timer>("MovementTimer");
        actionTimer = GetNode<Timer>("ActionTimer");
        skeleton = GetNode<Node2D>("Node2D");
        
        actionTimer.OneShot = true;
        actionTimer.Timeout += OnActionFinished;
        idleTimer.Timeout += OnidleTimeout;
        float IdleTime = rng.RandfRange(IdleTimeMin, IdleTimeMax);

        idleTimer.WaitTime = IdleTime;
        idleTimer.Start();
    }
    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {   
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
        {     GD.Print("HERE");
            if (!clicked)
            {
              
                clicked = true;
                LookingForMate();

            }
            else
            {
                clicked = false;
            }
        }
    }
    private void LookingForMate()
    {
        //check if any other mobs in the scene are looking for a mate, if yes, choose the closest mob and make them walk to each other. meet in the middle type shit, or meet near the nearest Nest
        sprite.Modulate = Colors.Red;
    }
    public override void _PhysicsProcess(double delta)
    {

        if (IsBreaking)
        {
            if (Velocity != Vector2.Zero)
            {
                Velocity = Vector2.Zero;

            }

            return;
        }

        if (WalkingToTarget && currentTarget != null)
        {
            MoveToGridSpace();
        }
        else if (isMoving)
        {
            Velocity = direction * Speed;
            MoveAndSlide();

            if (GetSlideCollisionCount() > 0)
            {
                GD.Print("Collided");
                StopMoving();
            }
        }
        else
        {
            Velocity = Vector2.Zero;
        }

        // --- Handle animation ---
        if (Velocity.Length() > 1) // mob is moving
        {

            // Flip skeleton based on direction
            if (Velocity.X < 0)
                skeleton.Scale = new Vector2(1, 1);  // facing right
            else if (Velocity.X > 0)
                skeleton.Scale = new Vector2(-1, 1); // facing left
        }

    }

    private void MoveToGridSpace()
    {
        Vector2 desiredVelocity = (currentPath[pathIndex] - GlobalPosition).Normalized() * Speed;
        Velocity = desiredVelocity;
        MoveAndSlide();
        if (GlobalPosition.DistanceTo(currentPath[pathIndex]) < 4f)
        {
            pathIndex++;
            if(pathIndex >= currentPath.Length)
            {
                OnTargetReached();
            }
        }

    }



    private void OnidleTimeout()
    {
        if (isMoving)
        {
            StopMoving();
        }
        else
        {
            // Pick a new random direction
            direction = new Vector2(
                rng.RandfRange(-1f, 1f),
                rng.RandfRange(-1f, 1f)
            ).Normalized();

            // Check if that direction is blocked right away
            var testOffset = direction * Speed * 0.1f; // small step, e.g. 0.1s worth
            if (TestMove(GlobalTransform, testOffset))
            {
                // Blocked immediately → go back to idle instead of moving
                StopMoving();
                return;
            }

            // Otherwise start moving
            isMoving = true;
            float movetime = rng.RandfRange(MoveTimeMin, MoveTimeMax);
            idleTimer.WaitTime = movetime;
            idleTimer.Start();
        }
    }
    private void StopMoving()
    {
        isMoving = false;
        float IdleTime = rng.RandfRange(IdleTimeMin, IdleTimeMax);
        idleTimer.WaitTime = IdleTime;
        idleTimer.Start();
    }

    public bool GoToWork(BreakableObstacle target)
    {
        Vector2[] bestPath = null;
        foreach (var offset in SurroundOffsets)
        {
            Vector2I targetCell = GridManager.ToCell(target.GlobalPosition, GridManager.TileSize);
            Vector2I neighborCell = targetCell + offset;

            if (!GridManager.Grid.IsPointSolid(neighborCell))
            {
                // This neighbor is walkable, test path to it
                Vector2I mobCell = GridManager.ToCell(GlobalPosition, GridManager.TileSize);
                Vector2[] path = GridManager.Grid.GetPointPath(mobCell, neighborCell);

                if (path.Length > 0)
                {
                    float totalDistance = GetPathLength(path);

                    if (bestPath == null || totalDistance < GetPathLength(bestPath))
                    {
                        bestPath = path;
                    }
                }
            }
        }
        if (bestPath != null)
                {
                    Vector2 tileCenterOffset = new Vector2(GridManager.TileSize / 2f, GridManager.TileSize / 2f);
                    currentPath = bestPath;
                    for (int i = 0; i < currentPath.Length; i++)
                    {
                        currentPath[i] += tileCenterOffset;
                    }
                    pathIndex = 0;
                    WalkingToTarget = true;
                    sprite.Modulate = Colors.Purple;
                    currentTarget = target;
                    return true;
                }
        return false;
    }
       private float GetPathLength(Vector2[] path)
{
    float total = 0f;

    for (int i = 0; i < path.Length - 1; i++)
    {
        total += path[i].DistanceTo(path[i + 1]);
    }

    return total;
}
    public void WorkDismissed()
    {
        sprite.Modulate = Colors.White;

        IsBreaking = false;
        currentTarget = null;
        WalkingToTarget = false;
    }
    private void OnTargetReached()
    {
        IsBreaking = true;


        if (currentTarget != null)
        {
            actionTimer.Start(3.0);
        }
    }
 
    private void OnActionFinished()
    {
        if (currentTarget != null)
        {
            currentTarget.Break(); // call obstacle’s break method
        }

        WorkDismissed();

    }
}
   
