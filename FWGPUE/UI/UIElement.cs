using FWGPUE.Graphics;
using System.Numerics;

namespace FWGPUE.UI;

class UIElement {
    public bool MouseOver { get; protected set; }

    public Style CurrentStyle = Style.DefaultNeutral;

    public Style Neutral = Style.DefaultNeutral;
    public Style Hovered = Style.DefaultHovered;
    public Style Depressed = Style.DefaultDepressed;
    public Style Disabled = Style.DefaultDisabled;

    public string Text;

    public Vector2 Middle;
    public Vector2 Dimensions;

    public Vector2 TopLeft => Middle - Dimensions / 2f;
    public Vector2 BottomRight => TopLeft + Dimensions / 2f;

    public virtual void Press() {}

    public virtual void Render(double elapsed) {
        if (MouseOver && MouseButtonPressed(MouseButton.Left)) {
            CurrentStyle = Depressed;

            Press();
        }
        else if (MouseOver) {
            CurrentStyle = Hovered;
        }

        Renderer.PushRect(TopLeft, BottomRight, 10, CurrentStyle.BackgroundColour, CurrentStyle.HasBorder ? 0 : CurrentStyle.BorderWidth, CurrentStyle.HasBorder ? CurrentStyle.BorderColour : null);

        if (Text != null) {
            Renderer.PushString(new(TopLeft, 11), Text, 10, CurrentStyle.TextColour);
        }
    }

    void OnMouseMove(Vector2 oldMouse, Vector2 newMouse) {
        if (newMouse.X > TopLeft.X && newMouse.X < BottomRight.X && newMouse.Y > TopLeft.Y && newMouse.Y < BottomRight.Y) {
            MouseOver = true;
            CurrentStyle = Hovered;
        }
        else {
            MouseOver = false;
            CurrentStyle = Neutral;
        }
    }

    public UIElement() {
        Renderer.OnRenderObjectsRequired += Render;
        MouseMove += OnMouseMove;
    }
}

