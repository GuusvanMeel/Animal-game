
using Godot;
public partial class Nest : WorldObject
{    
    public Mob Manimal { get; private set; }
    public Mob Fanimal { get; private set; }
    public Mob Zanimal { get; private set; }
    [Export] PackedScene mobScene;

    // 🔒 Full = two compatible occupants
    public bool IsFull => OccupantCount >= 2;

    private int OccupantCount
    {
        get
        {
            int count = 0;
            if (Manimal != null) count++;
            if (Fanimal != null) count++;
            if (Zanimal != null) count++;
            return count;
        }
    }
   public bool CanAccept(Mob mob)
    {
        if (IsFull) return false;

        switch (mob.gender)
        {
            case Enums.Gender.Male:
                // A male can join if no male or “both” male-equivalent already here
                return Manimal == null && (Fanimal == null || Zanimal == null);
            case Enums.Gender.Female:
                return Fanimal == null && (Manimal == null || Zanimal == null);
            case Enums.Gender.Both:
                // Both can join if there’s room for *anyone*
                return OccupantCount < 2;
            default:
                return false;
        }
    }
    public Nest AssignMob(Mob mob)
    {   
        switch (mob.gender)
        {
            case Enums.Gender.Male:
                Manimal = mob;                
                break;
            case Enums.Gender.Female:
                Fanimal = mob;
                break;
            case Enums.Gender.Both:
                Zanimal = mob;
                break;
        }
        return this;
    }

    public override bool Interact(Mob mob)
    {
        mob.HasToStop = true;
        if (IsFull)
        {
            var child = mobScene.Instantiate<Mob>();
            AddChild(child);
            child.GlobalPosition = this.GlobalPosition;
            Clear();
        }
        return true;
    }
 private void Clear()
{
    if (Manimal is { } m) { m.HasToStop = false; m.clicked = false; Manimal = null; }
    if (Fanimal is { } f) { f.HasToStop = false;f.clicked = false; Fanimal = null; }
    if (Zanimal is { } z) { z.HasToStop = false;z.clicked = false; Zanimal = null; }
}

}
