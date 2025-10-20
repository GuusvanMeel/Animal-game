
using Godot;
public partial class Nest : WorldObject
{    
    public Mob Xanimal { get; private set; }
    public Mob Yanimal { get; private set; }
    public Mob Zanimal { get; private set; }

   
    public void AssignMob(Mob mob)
    {
        if (mob.gender == Enums.Gender.Both)
        {
            Zanimal = mob;
        }
        else
        {
            
        }
    }

    public override void Interact(Mob mob)
    {
        throw new System.NotImplementedException();
    }

}
