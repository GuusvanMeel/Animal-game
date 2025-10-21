using System;

using Godot;
using Enums;


public partial class BreakableObstacle : WorldObject
{
    [Export] public BreakType RequiredBreakType { get; set; }

    private bool clicked = false;
    private Sprite2D sprite;
    public Mob assignedMob;
    private Vector2I gridCell;

    public override void _Ready()
    {   base._Ready();
        GD.Print(RequiredBreakType);
        sprite = GetNode<Sprite2D>("Sprite2D");
        
        var area = GetNode<Area2D>("Area2D");
        area.InputEvent += OnAreaInputEvent;
    
}

    private void OnAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (!clicked)
                RequestBreaker();
            else
                DismissBreaker();
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

    public override void Interact(Mob mob)
    {
        GridManager.Grid.SetPointSolid(gridCell, false);
        GridManager.GridObjects.Remove(this.gridCell);
        QueueFree();
        GridManager.Grid.Update();
    }

}
