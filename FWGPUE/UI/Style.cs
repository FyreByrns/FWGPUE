using System.Numerics;

namespace FWGPUE.UI;

class Style {
    public Colour BackgroundColour;
    public Colour BorderColour;
    public Colour TextColour = Vector3.Zero;

    public bool HasBorder = false;
    public float BorderWidth = 5;

    public static readonly Style DefaultNeutral = new() {
        BackgroundColour = Vector3.One,
        TextColour = Vector3.Zero
    };
    public static readonly Style DefaultHovered = new() {
        BackgroundColour = new(0.7f, 0.7f, 0.7f),
        HasBorder = true,
        BorderColour = Vector3.One
    };
    public static readonly Style DefaultDepressed = new() {
        BackgroundColour = new(0.4f, 0.4f, 0.4f),
        HasBorder = true,
        BorderColour = Vector3.Zero,
        BorderWidth = 8
    };
    public static readonly Style DefaultDisabled = new() {
        BackgroundColour = new(0.1f, 0.1f, 0.1f)
    };
}

