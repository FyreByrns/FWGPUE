using System.Reflection;
using System.Numerics;
using FWGPUE.Graphics;

namespace FWGPUE.Scenes;

class UIElement {
    public Vector2 Position;
    public Vector2 Dimensions;
    public Vector2 BottomRight => Position + Dimensions;

    public List<UIElement> Children = new();
}

class Test : Scene {
    TextManager TextManager = new();
    List<UIElement> UI = new();
    UIElement? Hovered = null;

    public override void Load() {
        Load<Test>();

        TextManager.LoadFont("default");
        Renderer.OnRenderObjectsRequired += OnRender;
        MouseMove += OnMouseMove;

        for (int i = 0; i < 10; i++) {
            UI.Add(new() {
                Position = new(5 + 30 * i + 5 * i, 5),
                Dimensions = new(30, 30)
            });
        }
    }

    public override void Tick() {
        base.Tick();
    }
    private void OnMouseMove(Vector2 oldMouse, Vector2 newMouse) {
        // update UI
        Hovered = null;
        foreach (var element in UI) {
            if (newMouse.X >= element.Position.X && newMouse.Y >= element.Position.Y && newMouse.X < element.BottomRight.X && newMouse.Y < element.BottomRight.Y) {
                Hovered = element;
            }
        }
    }

    static class Letters {
        public static IEnumerable<Vector2> GenerateLineGeo(Vector2 a, Vector2 b, float thickness = 1) {
            float angleBetween = RadiansToTurns((float)Math.Atan2(a.Y - b.Y, a.X - b.X));
            float anglePlusHalf = angleBetween + 0.5f;

            thickness /= 2;

            Vector2 _a = a.Along(+thickness, anglePlusHalf);
            Vector2 _b = a.Along(-thickness, anglePlusHalf);
            Vector2 _c = b.Along(+thickness, anglePlusHalf);
            Vector2 _d = b.Along(-thickness, anglePlusHalf);

            yield return _a;
            yield return _c;
            yield return _d;
            yield return _b;
        }
        public static IEnumerable<Vector2[]> GenerateCircleGeo(Vector2 center, float fromAngle, float length, float radius, float thickness, int steps = 30) {
            for (int i = 1; i <= steps; i++) {
                float angleChange = length / (float)steps;
                float lastAngle = fromAngle + angleChange * (i - 1);
                float angle = fromAngle + angleChange * i;

                yield return new Vector2[] {
                    center.Along(radius-thickness, lastAngle),
                    center.Along(radius-thickness, angle),
                    center.Along(radius, angle),
                    center.Along(radius, lastAngle),
                };
            }
        }
        public static IEnumerable<Vector2> GenerateRectGeo(Vector2 topLeft, Vector2 bottomRight) {
            yield return topLeft;
            yield return new(bottomRight.X, topLeft.Y);
            yield return bottomRight;
            yield return new(topLeft.X, bottomRight.Y);
        }
    }

    private void OnRender(double elapsed) {
        Vector3 col = new(0.8f, 0.9f, 0.99f);

        foreach (var poly in TextManager.GetTextPolygons("default", "AaBbCcDdEeFfGgHhIiJjKkLl\nMmNnOoPpQqRrSsTtUuVvWwXx\nYyZz")) {
            Renderer.PushConvexPolygon(10, col, true, false, 2, poly.ScaleAll(new Vector2(30, 30)).TransformAll(new Vector2(10, 100)).ToArray());
        }

        // draw UI
        foreach (var element in UI) {
            if (element == Hovered) {
                Renderer.PushRect(element.Position, element.Position + element.Dimensions, 2, col, 3, new(0.4f, 0.2f, 0.9f), true);
            }
            else {
                Renderer.PushRect(element.Position, element.Position + element.Dimensions, 2, col, filled: true);
            }
        }
    }

    public override void Unload() { }
}