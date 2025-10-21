using Godot;
using System;

public abstract partial class WorldObject : Node2D
{
    public Vector2I GridCell { get; protected set; }
    [Export] public bool BlocksPath = false;

    public override void _Ready()
    {
        RegisterOnGrid();
    }
    protected virtual void RegisterOnGrid()
    {
        GridCell = GridManager.ToCell(GlobalPosition);
            GD.Print($"{Name} registering at {GridCell} (GlobalPos={GlobalPosition})");
        GridManager.RegisterObject(this);
    }
    public abstract void Interact(Mob mob = null);
}
