
using System;
using Godot;
namespace Enums
{
    [Flags]
    public enum BreakType
    {
        None = 0,
        Slam = 1 << 0, // 1
        Cut = 1 << 1, // 2
        Burn = 1 << 2  // 4
                       // Add more if needed: e.g. Freeze = 1 << 3 (8), etc.
    }
    public enum Gender
    {
        Male,
        Female,
Both
    }
}
