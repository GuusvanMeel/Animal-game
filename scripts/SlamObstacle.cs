using Godot;
using System;

public partial class SlamObstacle : StaticBody2D
{

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
            GD.Print("Boulder clicked!");
            if (!clicked)
            {
                sprite.Modulate = Colors.Red; // highlight
                clicked = true;
                RequestBreaker();
            }
            else
            {
                DismissBreaker();
                sprite.Modulate = Colors.White;
                clicked = false;
            }

        }
    }
    private void RequestBreaker()
    {

        var world = GetTree().Root.GetNode<World>("World"); // adjust path if needed
        assignedMob = world.AssignMobToSlamObstacle(this);
    }
    private void DismissBreaker()
    {
        var world = GetTree().Root.GetNode<World>("World"); // adjust path if needed
        world.DismissMobFromSlamObstacle(this);
    }
     public void Break()
    {

        GD.Print("Obstacle destroyed!");
        QueueFree();

    }
}
