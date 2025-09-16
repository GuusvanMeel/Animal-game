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
    [Export] public bool CanSlam { get; set; } = true;


    private SlamObstacle currentTarget;
    private NavigationAgent2D agent;


    private Timer idleTimer;
    private Timer actionTimer;
    private Vector2 direction = Vector2.Zero;
    private bool isMoving = false;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Sprite2D sprite;

    public bool isBusy = false;
    private bool isWorking = false;

    public override void _Ready()
    {
        agent = GetNode<NavigationAgent2D>("NavigationAgent2D");

        sprite = GetNode<Sprite2D>("Sprite2D");
        AddToGroup("mobs");
        rng.Randomize();
        idleTimer = GetNode<Timer>("MovementTimer");
        actionTimer = GetNode<Timer>("ActionTimer");
        actionTimer.OneShot = true;
        actionTimer.Timeout += OnActionFinished;
        idleTimer.Timeout += OnidleTimeout;
        agent.TargetReached += OnTargetReached;
        agent.TargetDesiredDistance = 15f; // how close to goal counts as "arrived"
        agent.PathDesiredDistance = 4f; // tolerance for following path
        float IdleTime = rng.RandfRange(IdleTimeMin, IdleTimeMax);
        GD.Print(IdleTime);
        idleTimer.WaitTime = IdleTime;
        idleTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {  
        if (isWorking)
        {
            Velocity = Vector2.Zero;
            return;
        }

        if (isBusy && currentTarget != null)
        {
            Vector2 nextPoint = agent.GetNextPathPosition();
            Vector2 dir = (nextPoint - GlobalPosition).Normalized();
            Velocity = dir * Speed;
            MoveAndSlide();
            return; // don’t also do wander logic
        }
        if (isMoving)
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
                GD.Print("blocked by wall");
                StopMoving();
                return;
            }

            // Otherwise start moving
            isMoving = true;
            float movetime = rng.RandfRange(MoveTimeMin, MoveTimeMax);
            GD.Print("MoveTime is:" + movetime);
            idleTimer.WaitTime = movetime;
            idleTimer.Start();
        }
    }
    private void StopMoving()
    {
        isMoving = false;
        float IdleTime = rng.RandfRange(IdleTimeMin, IdleTimeMax);
        GD.Print("Idletime is:" + IdleTime);
        idleTimer.WaitTime = IdleTime;
        idleTimer.Start();
    }
    public void GoToWork(SlamObstacle target)
    {
        sprite.Modulate = Colors.Purple;
        isBusy = true;
        currentTarget = target;
        agent.TargetPosition = target.GlobalPosition;
    }
    public void WorkDismissed()
    {
        sprite.Modulate = Colors.White;
        isBusy = false;
        isWorking = false;
        currentTarget = null;
    }
    private void OnTargetReached()
    {
        GD.Print("Reached obstacle, slamming now!");
        isWorking = true;

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
