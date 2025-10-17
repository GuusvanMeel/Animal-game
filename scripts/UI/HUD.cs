using Godot;
using System;

public partial class HUD : CanvasLayer
{
    [Export] private Button BreedButton;


    public override void _Ready()
    {   
        BreedButton.Pressed += OnBreedPressed;
    }
    private void OnBreedPressed()
    {

        GetParent().Call("SpawnMob");
    }

    
}
