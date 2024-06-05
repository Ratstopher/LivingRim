using UnityEngine;
using Verse;

public class WindowState : IExposable
{
    public Vector2 Position;
    public Vector2 Size;

    public WindowState() { }

    public WindowState(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref Position, "Position", Vector2.zero);
        Scribe_Values.Look(ref Size, "Size", Vector2.zero);
    }
}
