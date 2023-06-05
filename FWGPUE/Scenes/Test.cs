using System.Reflection;
using System.Numerics;

namespace FWGPUE.Scenes;

class UIElement {
    public Vector2 Position;
    public Vector2 Dimensions;
    public Vector2 BottomRight => Position + Dimensions;

    public List<UIElement> Children = new();
}

class Test : Scene {
    class Point {
        public Vector2 Position;
        public Point(Vector2 position) {
            Position = position;
        }
    }

    List<UIElement> UI = new();
    UIElement? Hovered = null;

    public override void Load() {
        Load<Test>();

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
        static Dictionary<char, PolygonSet> CharsToLetterDefinitions = new();
        const string Alphabet = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
        static Letters() {
            CharsToLetterDefinitions['A'] = new() {
                new Vector2[] { new(0.5f, 0), new(0, 1), new(0.1f, 1), new(0.5f, 0.2f) },
                new Vector2[] { new(0.5f, 0), new(1, 1), new(0.9f, 1), new(0.5f, 0.2f) },
                new Vector2[] { new(0.25f, 0.5f), new(0.75f, 0.5f), new(0.75f, 0.6f), new(0.25f, 0.6f) }
            };
            CharsToLetterDefinitions['B'] = new() {
                GenerateCircleGeo(new(0.73f, 0.27f), 0.5f, 0.5f, 0.27f, 0.1f),
                GenerateCircleGeo(new(0.73f, 0.73f), 0.5f, 0.5f, 0.27f, 0.1f),
                GenerateRectGeo(new(0, 0), new(0.1f, 1f)),
                GenerateRectGeo(new(0, 0), new(0.73f, 0.1f)),
                GenerateRectGeo(new(0, 0.45f), new(0.73f, 0.55f)),
                GenerateRectGeo(new(0, 0.9f), new(0.73f, 1)),
            };
            CharsToLetterDefinitions['C'] = new() {
                GenerateCircleGeo(new(0.25f, 0.25f), 0.25f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.25f, 0.75f), 1.00f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.25f), 0.50f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.75f), 0.75f, 0.25f, 0.25f, 0.1f),
                GenerateRectGeo(new(0.25f, 0.00f), new(0.75f, 0.10f)),
                GenerateRectGeo(new(0.25f, 0.90f), new(0.75f, 1.00f)),
                GenerateRectGeo(new(0.00f, 0.25f), new(0.10f, 0.75f)),
            };
            CharsToLetterDefinitions['D'] = new() {
                GenerateCircleGeo(new(0.5f, 0.5f), 0, -0.5f, 0.5f, 0.1f),
                GenerateRectGeo(new(0, 0), new(0.1f, 1f)),
                GenerateRectGeo(new(0, 0), new(0.5f, 0.1f)),
                GenerateRectGeo(new(0, 0.9f), new(0.5f, 1f)),
            };
            CharsToLetterDefinitions['E'] = new() {
                GenerateRectGeo(new(0, 0), new(1, 0.1f)),
                GenerateRectGeo(new(0, 0.45f), new(0.75f, 0.55f)),
                GenerateRectGeo(new(0, 0.9f), new(1, 1)),
                GenerateRectGeo(new(0, 0), new(0.1f, 1)),
            };
            CharsToLetterDefinitions['F'] = new() {
                GenerateRectGeo(new(0, 0), new(1, 0.1f)),
                GenerateRectGeo(new(0, 0.45f), new(0.75f, 0.55f)),
                GenerateRectGeo(new(0, 0), new(0.1f, 1)),
            };
            CharsToLetterDefinitions['G'] = new() {
                GenerateCircleGeo(new(0.25f, 0.25f), 0.25f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.25f, 0.75f), 1.00f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.25f), 0.50f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.75f), 0.75f, 0.25f, 0.25f, 0.1f),
                GenerateRectGeo(new(0.25f, 0.00f), new(0.75f, 0.10f)),
                GenerateRectGeo(new(0.25f, 0.90f), new(0.75f, 1.00f)),
                GenerateRectGeo(new(0.00f, 0.25f), new(0.10f, 0.75f)),
                GenerateRectGeo(new(0.90f, 0.50f), new(1.00f, 0.75f)),
                GenerateRectGeo(new(0.50f, 0.45f), new(1.00f, 0.55f)),
            };
            CharsToLetterDefinitions['H'] = new() {
                GenerateRectGeo(new(0,0), new(0.1f, 1)),
                GenerateRectGeo(new(0.9f,0), new(1, 1)),
                GenerateRectGeo(new(0f, 0.45f), new(1.00f, 0.55f)),
            };
            CharsToLetterDefinitions['I'] = new() {
                GenerateRectGeo(new(0, 0), new(1, 0.1f)),
                GenerateRectGeo(new(0, 0.9f), new(1, 1)),
                GenerateRectGeo(new(0.45f, 0f), new(0.55f, 1)),
            };
            CharsToLetterDefinitions['J'] = new() {
                GenerateCircleGeo(new(0.75f, 0.75f), 0.75f, 0.25f, 0.25f, 0.1f),
                GenerateRectGeo(new(0, 0), new(1, 0.1f)),
                GenerateRectGeo(new(0.9f, 0), new(1, 0.75f)),
                GenerateRectGeo(new(0, 0.9f), new(0.75f, 1)),
            };
            CharsToLetterDefinitions['K'] = new() {
                GenerateRectGeo(new(0, 0), new(0.1f, 1)),
                GenerateRectGeo(new(0.1f, 0.45f), new(0.55f, 0.55f)),
                GenerateLineGeo(new(0.5f, 0.5f), new(0.85f, 0.02f), 0.1f),
                GenerateLineGeo(new(0.5f, 0.5f), new(0.85f, 0.98f), 0.1f),
            };
            CharsToLetterDefinitions['L'] = new() {
                GenerateRectGeo(new(0, 0), new(0.1f, 1)).ToArray(),
                GenerateRectGeo(new(0.1f, 0.9f), new(1, 1)).ToArray()
            };
            CharsToLetterDefinitions['M'] = new() {
                GenerateRectGeo(new(0, 0), new(0.1f, 1)).ToArray(),
                GenerateRectGeo(new(0.9f, 0), new(1, 1)).ToArray(),
                GenerateRectGeo(new(0.45f, 0.5f), new(0.55f, 1)),
                GenerateCircleGeo(new(0, 0.55f), 0.5f, 0.25f, 0.55f, 0.1f),
                GenerateCircleGeo(new(1, 0.55f), 0.25f, 0.25f, 0.55f, 0.1f)
            };
            CharsToLetterDefinitions['N'] = new() {
                GenerateRectGeo(new(0, 0), new(0.1f, 1)).ToArray(),
                GenerateRectGeo(new(0.9f, 0), new(1, 1)).ToArray(),
                GenerateCircleGeo(new(0, 1), 0.5f, 0.25f, 1, 0.1f),
            };
            CharsToLetterDefinitions['O'] = new() {
                GenerateCircleGeo(new(0.25f, 0.25f), 0.25f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.25f, 0.75f), 1.00f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.25f), 0.50f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.75f), 0.75f, 0.25f, 0.25f, 0.1f),
                GenerateRectGeo(new(0.25f, 0.00f), new(0.75f, 0.10f)),
                GenerateRectGeo(new(0.25f, 0.90f), new(0.75f, 1.00f)),
                GenerateRectGeo(new(0.00f, 0.25f), new(0.10f, 0.75f)),
                GenerateRectGeo(new(0.90f, 0.25f), new(1.00f, 0.75f)),
            };
            CharsToLetterDefinitions['P'] = new() {
                GenerateRectGeo(new(0,0), new(0.1f, 1)),
                GenerateRectGeo(new(0, 0), new(0.75f, 0.1f)),
                GenerateRectGeo(new(0, 0.45f), new(0.75f, 0.55f)),
                GenerateCircleGeo(new(0.75f, 0.25f), 0.5f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.30f), 0.75f, 0.25f, 0.25f, 0.1f),
                GenerateRectGeo(new(0.9f, 0.25f), new(1, 0.30f)),
            };
            CharsToLetterDefinitions['Q'] = new() {
                GenerateCircleGeo(new(0.25f, 0.25f), 0.25f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.25f, 0.75f), 1.00f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.25f), 0.50f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.75f), 0.75f, 0.25f, 0.25f, 0.1f),
                GenerateRectGeo(new(0.25f, 0.00f), new(0.75f, 0.10f)),
                GenerateRectGeo(new(0.25f, 0.90f), new(0.75f, 1.00f)),
                GenerateRectGeo(new(0.00f, 0.25f), new(0.10f, 0.75f)),
                GenerateRectGeo(new(0.90f, 0.25f), new(1.00f, 0.75f)),
                GenerateCircleGeo(new(0.5f, 1), 0.5f, 0.25f, 0.5f, 0.1f),
                //GenerateLineGeo(new(0.75f, 0.75f), new(0.95f, 0.95f), 0.1f)
            };
            CharsToLetterDefinitions['R'] = new() {
                GenerateRectGeo(new(0,0), new(0.1f, 1)),
                GenerateRectGeo(new(0, 0), new(0.75f, 0.1f)),
                GenerateRectGeo(new(0, 0.45f), new(0.75f, 0.55f)),
                GenerateCircleGeo(new(0.75f, 0.25f), 0.5f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.30f), 0.75f, 0.25f, 0.25f, 0.1f),
                GenerateRectGeo(new(0.9f, 0.25f), new(1, 0.30f)),
                GenerateCircleGeo(new(0.75f, 0.70f), 0.5f, 0.25f, 0.25f, 0.1f),
                GenerateRectGeo(new(0.9f, 0.70f), new(1, 1)),
            };
            CharsToLetterDefinitions['S'] = new() {
                GenerateCircleGeo(new(0.25f, 0.25f), 0.25f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.25f, 0.30f), 0.0f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.70f), 0.5f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.75f), 0.75f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.75f, 0.25f), 0.5f, 0.25f, 0.25f, 0.1f),
                GenerateCircleGeo(new(0.25f, 0.75f), 0.0f, 0.25f, 0.25f, 0.1f),
                GenerateRectGeo(new(0.25f, 0), new(0.75f, 0.1f)),
                GenerateRectGeo(new(0.25f, 0.45f), new(0.75f, 0.55f)),
                GenerateRectGeo(new(0.25f, 0.9f), new(0.75f, 1)),
                GenerateRectGeo(new(0, 0.20f), new(0.1f, 0.30f)),
                GenerateRectGeo(new(0.9f, 0.70f), new(1, 0.80f))
            };
    }

        public static IEnumerable<Vector2[]> GetTextPolygons(string text) {
            float xOffset = 0;
            float yOffset = 0;
            foreach (char c in text) {
                if(c == '\n') {
                    xOffset = 0;
                    yOffset += 1.1f;
                    continue;
                }

                if (CharsToLetterDefinitions.ContainsKey(c)) {
                    foreach (var polygon in CharsToLetterDefinitions[c].Polygons) {
                        yield return polygon.TransformAll(new Vector2(xOffset, yOffset)).ToArray();
                    }
                }

                xOffset += 1.1f;
            }
        }

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

        foreach (var poly in Letters.GetTextPolygons("AaBbCcDdEeFfGgHhIiJjKkLl\nMmNnOoPpQqRrSsTtUuVvWwXx\nYyZz")) {
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