using Godot;
using System;
using System.Data.SqlTypes;

public partial class Mob : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 100f;
    [Export] public float MoveTimeMax { get; set; } = 2f;
    [Export] public float MoveTimeMin { get; set; } = 0.5f;
    [Export] public float IdleTimeMax { get; set; } = 1.6f;
    [Export] public float IdleTimeMin { get; set; } = 1f;
    [Export] public BreakType CanBreakType { get; set; }




    private BreakableObstacle currentTarget;
    private NavigationAgent2D agent;
    private AnimationPlayer animationPlayer;
    private Skeleton2D skeleton;
    private Timer idleTimer;
    private Timer actionTimer;
    private Vector2 direction = Vector2.Zero;
    private bool isMoving = false;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Sprite2D sprite;

    public bool isBusy = false;
    private bool isWorking = false;
    private static readonly Vector2I[] SurroundOffsets = new Vector2I[]
{
    new Vector2I(-1, -1), new Vector2I(0, -1), new Vector2I(1, -1),
    new Vector2I(-1,  0),                     new Vector2I(1,  0),
    new Vector2I(-1,  1), new Vector2I(0,  1), new Vector2I(1,  1),
};

    public override void _Ready()
    {
        agent = GetNode<NavigationAgent2D>("NavigationAgent2D");

        sprite = GetNode<Sprite2D>("Skeleton2D/TorsoBone/TorsoSprite");
        AddToGroup("mobs");
        rng.Randomize();
        idleTimer = GetNode<Timer>("MovementTimer");
        actionTimer = GetNode<Timer>("ActionTimer");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        skeleton = GetNode<Skeleton2D>("Skeleton2D");
        actionTimer.OneShot = true;
        actionTimer.Timeout += OnActionFinished;
        idleTimer.Timeout += OnidleTimeout;
        agent.TargetReached += OnTargetReached;
        agent.TargetDesiredDistance = 1f; //how close to goal counts as "arrived"
        agent.PathDesiredDistance = 4f; // tolerance for following path
        float IdleTime = rng.RandfRange(IdleTimeMin, IdleTimeMax);
        GD.Print(IdleTime);
        idleTimer.WaitTime = IdleTime;
        idleTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
{
    if (isWorking)
    {   if (Velocity != Vector2.Zero)
            {
            Velocity = Vector2.Zero;

            animationPlayer.Stop();
            }
       
            return;
    }

    if (isBusy && currentTarget != null)
    {
        Vector2 nextPoint = agent.GetNextPathPosition();
        Vector2 dir = (nextPoint - GlobalPosition).Normalized();
        Velocity = dir * Speed;
        MoveAndSlide();
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
        if (!animationPlayer.IsPlaying())
                animationPlayer.Play("NoLegJointWalkingAnim");

            // Flip skeleton based on direction
            if (Velocity.X < 0)
            skeleton.Scale = new Vector2(1, 1);  // facing right
        else if (Velocity.X > 0)
            skeleton.Scale = new Vector2(-1, 1); // facing left
    }
    else
    {
        if (animationPlayer.IsPlaying())
            animationPlayer.Stop();
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

        sprite.Modulate = Colors.Purple;
        isBusy = true;
        currentTarget = target;
        float cellSize = 16f;

        Vector2 bestPos = target.GlobalPosition;
        float bestDist = float.MaxValue;

        foreach (var offset in SurroundOffsets)
        {
            Vector2 candidate = target.GlobalPosition + (Vector2)offset * cellSize;

            // Snap to nearest navmesh point
            Vector2 navPoint = NavigationServer2D.MapGetClosestPoint(agent.GetNavigationMap(), candidate);

            float dist = GlobalPosition.DistanceTo(navPoint);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestPos = navPoint;
            }
        }

        agent.TargetPosition = bestPos;

        if (agent.GetNextPathPosition() == this.GlobalPosition)
        {
            GD.Print($"{Name}: No path to {target.Name}");
            WorkDismissed();        // reset state
            return false;
        }
        return true;

}
    public void WorkDismissed()
    {
        sprite.Modulate = Colors.White;

        isWorking = false;
        currentTarget = null;
        isBusy = false;
    }
    private void OnTargetReached()
    {
        GD.Print(this.Position.X + this.Position.Y);
        GD.Print("Reached obstacle, slamming now!");
        isWorking = true;
        isBusy = false;

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
