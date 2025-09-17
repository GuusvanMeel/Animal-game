using Godot;
using System;

public enum BreakType
{
    Slam,
    Cut,
    Burn
}

public partial class BreakableObstacle : StaticBody2D
{
    [Export] public BreakType RequiredBreakType { get; set; }

    private bool clicked = false;
    private Sprite2D sprite;
    public Mob assignedMob;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (!clicked)
            {
                RequestBreaker();
            }
            else
            {
                DismissBreaker();                
            }
        }
    }

    private void RequestBreaker()
    {
        var world = GetTree().Root.GetNode<World>("World");
        assignedMob = world.AssignMobToObstacle(this); // Pass this obstacle
        if (assignedMob == null)
        {
            sprite.Modulate = Colors.Yellow;
            GetTree().CreateTimer(0.5).Timeout += () => sprite.Modulate = Colors.White;
        }
        else
        {
            sprite.Modulate = Colors.Red;
            clicked = true;
        }
    }

    private void DismissBreaker()
    {
        var world = GetTree().Root.GetNode<World>("World");
        world.DismissMobFromObstacle(this);

        sprite.Modulate = Colors.White;
        clicked = false;
    }

    public void Break()
    {
        GD.Print($"{RequiredBreakType} obstacle destroyed!");
        QueueFree();
    }
}
