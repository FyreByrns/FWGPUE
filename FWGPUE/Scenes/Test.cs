using System.Reflection;
using System.Numerics;
using Silk.NET.OpenGL;

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
        static Dictionary<char, Vector2[][]> CharsToLetterDefinitions = new();
        const string Alphabet = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
        static Letters() {
            List<Vector2[]> _B = new();
            _B.AddRange(GenerateCircleGeo(new(1 - 0.3f, 0.3f), 0.5f, 0.42f, 0.3f, 0.1f));
            _B.AddRange(GenerateCircleGeo(new(1 - 0.3f, 0.7f), 0, -0.42f, 0.3f, 0.1f));
            _B.AddRange(new[] {
                GenerateRectGeo(new(0, 0), new(0.1f, 1f)).ToArray(),
                GenerateRectGeo(new(0, 0), new(1-0.3f, 0.1f)).ToArray(),
                GenerateRectGeo(new(0, 0.45f), new(0.9f, 0.55f)).ToArray(),
                GenerateRectGeo(new(0, 0.9f), new(1-0.3f, 1)).ToArray(),
            });
            B = _B.ToArray();

            //C = GenerateCircleGeo(new(0.5f, 0.5f), -0.1f, 0.7f, 0.5f, 0.1f).ToArray();
            List<Vector2[]> _C = new();
            _C.AddRange(GenerateCircleGeo(new(0.25f, 0.25f), 0.25f, 0.25f, 0.25f, 0.1f));
            _C.AddRange(GenerateCircleGeo(new(0.25f, 0.75f), 1.00f, 0.25f, 0.25f, 0.1f));
            _C.AddRange(GenerateCircleGeo(new(0.75f, 0.25f), 0.50f, 0.25f, 0.25f, 0.1f));
            _C.AddRange(GenerateCircleGeo(new(0.75f, 0.75f), 0.75f, 0.25f, 0.25f, 0.1f));
            _C.AddRange(new[] {
                GenerateRectGeo(new(0.25f, 0.00f), new(0.75f, 0.10f)).ToArray(),
                GenerateRectGeo(new(0.25f, 0.90f), new(0.75f, 1.00f)).ToArray(),
                GenerateRectGeo(new(0.00f, 0.25f), new(0.10f, 0.75f)).ToArray(),
            });
            C = _C.ToArray();

            List<Vector2[]> _D = new();
            _D.AddRange(GenerateCircleGeo(new(0.5f, 0.5f), 0, -0.5f, 0.5f, 0.1f));
            _D.AddRange(new[] {
                GenerateRectGeo(new(0, 0), new(0.1f, 1f)).ToArray(),
                GenerateRectGeo(new(0, 0), new(0.5f, 0.1f)).ToArray(),
                GenerateRectGeo(new(0, 0.9f), new(0.5f, 1f)).ToArray(),
            });
            D = _D.ToArray();

            List<Vector2[]> _E = new();
            _E.AddRange(new[] {
                GenerateRectGeo(new(0, 0), new(1, 0.1f)).ToArray(),
                GenerateRectGeo(new(0, 0.45f), new(0.75f, 0.55f)).ToArray(),
                GenerateRectGeo(new(0, 0.9f), new(1, 1)).ToArray(),
                GenerateRectGeo(new(0, 0), new(0.1f, 1)).ToArray(),
            });
            E = _E.ToArray();

            List<Vector2[]> _F = new();
            _F.AddRange(new[] {
                GenerateRectGeo(new(0, 0), new(1, 0.1f)).ToArray(),
                GenerateRectGeo(new(0, 0.45f), new(0.75f, 0.55f)).ToArray(),
                GenerateRectGeo(new(0, 0), new(0.1f, 1)).ToArray(),
            });
            F = _F.ToArray();

            List<Vector2[]> _G = new();
            _G.AddRange(GenerateCircleGeo(new(0.25f, 0.25f), 0.25f, 0.25f, 0.25f, 0.1f));
            _G.AddRange(GenerateCircleGeo(new(0.25f, 0.75f), 1.00f, 0.25f, 0.25f, 0.1f));
            _G.AddRange(GenerateCircleGeo(new(0.75f, 0.25f), 0.50f, 0.25f, 0.25f, 0.1f));
            _G.AddRange(GenerateCircleGeo(new(0.75f, 0.75f), 0.75f, 0.25f, 0.25f, 0.1f));
            _G.AddRange(new[] {
                GenerateRectGeo(new(0.25f, 0.00f), new(0.75f, 0.10f)).ToArray(),
                GenerateRectGeo(new(0.25f, 0.90f), new(0.75f, 1.00f)).ToArray(),
                GenerateRectGeo(new(0.00f, 0.25f), new(0.10f, 0.75f)).ToArray(),
                GenerateRectGeo(new(0.90f, 0.50f), new(1.00f, 0.75f)).ToArray(),
                GenerateRectGeo(new(0.50f, 0.45f), new(1.00f, 0.55f)).ToArray(),
            });
            G = _G.ToArray();

            List<Vector2[]> _H = new();
            _H.AddRange(new[] {
                GenerateRectGeo(new(0,0), new(0.1f, 1)).ToArray(),
                GenerateRectGeo(new(0.9f,0), new(1, 1)).ToArray(),
                GenerateRectGeo(new(0f, 0.45f), new(1.00f, 0.55f)).ToArray(),
            });
            H = _H.ToArray();

            List<Vector2[]> _I = new();
            _I.AddRange(new[] {
                GenerateRectGeo(new(0, 0), new(1, 0.1f)).ToArray(),
                GenerateRectGeo(new(0, 0.9f), new(1, 1)).ToArray(),
                GenerateRectGeo(new(0.45f, 0f), new(0.55f, 1)).ToArray(),
            });
            I = _I.ToArray();

            List<Vector2[]> _J = new();
            _J.AddRange(GenerateCircleGeo(new(0.75f, 0.75f), 0.75f, 0.25f, 0.25f, 0.1f));
            _J.AddRange(new[] {
                GenerateRectGeo(new(0, 0), new(1, 0.1f)).ToArray(),
                GenerateRectGeo(new(0.9f, 0), new(1, 0.75f)).ToArray(),
                GenerateRectGeo(new(0, 0.9f), new(0.75f, 1)).ToArray(),
            });
            J = _J.ToArray();

            List<Vector2[]> _K = new();
            _K.AddRange(new[] {
                GenerateRectGeo(new(0, 0), new(0.1f, 1)).ToArray(),
                GenerateRectGeo(new(0.1f, 0.45f), new(0.55f, 0.55f)).ToArray(),
                GenerateLineGeo(new(0.5f, 0.5f), new(0.85f, 0.02f), 0.1f).ToArray(),
                GenerateLineGeo(new(0.5f, 0.5f), new(0.85f, 0.98f), 0.1f).ToArray(),
            });
            K = _K.ToArray();

            L = new Vector2[][] {
                GenerateRectGeo(new(0, 0), new(0.1f, 1)).ToArray(),
                GenerateRectGeo(new(0.1f, 0.9f), new(1, 1)).ToArray()
            };
            M = new Vector2[][] {
                GenerateRectGeo(new(0, 0), new(0.1f, 1)).ToArray(),
                GenerateRectGeo(new(0.9f, 0), new(1, 1)).ToArray(),
                GenerateLineGeo(new(0f, 0f), new(0.5f, 0.5f), 0.1f).ToArray(),
                GenerateLineGeo(new(1, 0f), new(0.5f, 0.5f), 0.1f).ToArray(),
            };
            N = new Vector2[][] {
                GenerateRectGeo(new(0, 0), new(0.1f, 1)).ToArray(),
                GenerateRectGeo(new(0.9f, 0), new(1, 1)).ToArray(),
                GenerateLineGeo(new(0, 0), new(1, 1), 0.1f).ToArray(),
            };
            O = GenerateCircleGeo(new(0.5f), 0, 1, 0.5f, 0.1f).ToArray();

            Type letters = typeof(Letters);
            foreach (FieldInfo info in letters.GetFields()) {
                if (Alphabet.Contains(info.Name.First())) {
                    CharsToLetterDefinitions[info.Name.First()] = (Vector2[][])info.GetValue(null);
                }
            }
        }

        public static IEnumerable<Vector2[]> GetTextPolygons(string text) {
            float totalOffset = 0;
            foreach (char c in text) {
                if (CharsToLetterDefinitions.ContainsKey(c)) {
                    foreach (Vector2[] letter in CharsToLetterDefinitions[c]) {
                        yield return letter.TransformAll(new Vector2(totalOffset, 0)).ToArray();
                    }
                }
                totalOffset += 1.1f;
            }
        }

        /* "A"
        0    -        a
                     /|\
        0.25 -      / d \
                   / / \ \
        0.5  -    g-/---\-h
                 / i-----j \
        0.75 -  / /       \ \
               / /         \ \
        1    -b-c           e-f */
        public static Vector2[][] A = new Vector2[][] {
            new Vector2[] { new(0.5f, 0), new(0, 1), new(0.1f, 1), new(0.5f, 0.2f) },
            new Vector2[] { new(0.5f, 0), new(1, 1), new(0.9f, 1), new(0.5f, 0.2f) },
            new Vector2[] { new(0.25f, 0.5f), new(0.75f, 0.5f), new(0.75f, 0.6f), new(0.25f, 0.6f) }
        };
        public static Vector2[][] a;
        public static Vector2[][] B;
        public static Vector2[][] b;
        public static Vector2[][] C;
        public static Vector2[][] c;
        public static Vector2[][] D;
        public static Vector2[][] d;
        public static Vector2[][] E;
        public static Vector2[][] e;
        public static Vector2[][] F;
        public static Vector2[][] f;
        public static Vector2[][] G;
        public static Vector2[][] g;
        public static Vector2[][] H;
        public static Vector2[][] h;
        public static Vector2[][] I;
        public static Vector2[][] i;
        public static Vector2[][] J;
        public static Vector2[][] j;
        public static Vector2[][] K;
        public static Vector2[][] k;
        public static Vector2[][] L;
        public static Vector2[][] l;
        public static Vector2[][] M;
        public static Vector2[][] m;
        public static Vector2[][] N;
        public static Vector2[][] n;
        public static Vector2[][] O;
        public static Vector2[][] o;
        public static Vector2[][] P;
        public static Vector2[][] p;
        public static Vector2[][] Q;
        public static Vector2[][] q;
        public static Vector2[][] R;
        public static Vector2[][] r;
        public static Vector2[][] S;
        public static Vector2[][] s;
        public static Vector2[][] T;
        public static Vector2[][] t;
        public static Vector2[][] U;
        public static Vector2[][] u;
        public static Vector2[][] V;
        public static Vector2[][] v;
        public static Vector2[][] W;
        public static Vector2[][] w;
        public static Vector2[][] X;
        public static Vector2[][] x;
        public static Vector2[][] Y;
        public static Vector2[][] y;
        public static Vector2[][] Z;
        public static Vector2[][] z;

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

        foreach (var poly in Letters.GetTextPolygons("ABCDEFGHIJKLMNO")) {
            Renderer.PushConvexPolygon(10, col, true, false, 2, poly.ScaleAll(new Vector2(50, 50)).TransformAll(new Vector2(10, 100)).ToArray());
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