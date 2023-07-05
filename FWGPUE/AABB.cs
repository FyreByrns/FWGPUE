using System.Numerics;

namespace FWGPUE;

class AABB {
    Vector2 _topLeft, _bottomRight;
    public Vector2 TopLeft {
        get => _topLeft;
        set {
            _topLeft = value;
            EnsureOrder();
        }
    }
    public Vector2 BottomRight {
        get => _bottomRight;
        set {
            _bottomRight = value;
            EnsureOrder();
        }
    }
    public float Top {
        get {
            return TopLeft.Y;
        }
        set {
            TopLeft = new(TopLeft.X, Y);
        }
    }
    public float Left {
        get {
            return TopLeft.X;
        }
        set {
            TopLeft = new(value, TopLeft.Y);
        }
    }
    public float Right {
        get {
            return BottomRight.X;
        }
        set {
            BottomRight = new(value, BottomRight.Y);
        }
    }
    public float Bottom {
        get {
            return BottomRight.Y;
        }
        set {
            BottomRight = new(BottomRight.X, value);
        }
    }

    public float Width => Right - Left;
    public float Height => Bottom - Top;

    /// <summary>
    /// Make sure _topLeft is above and to the left of _bottomRight, or on the same point.
    /// </summary>
    void EnsureOrder() {
        _topLeft.X = Math.Min(_topLeft.X, _bottomRight.X);
        _topLeft.Y = Math.Min(_topLeft.Y, _bottomRight.Y);
        _bottomRight.X = Math.Max(_topLeft.X, _bottomRight.X);
        _bottomRight.Y = Math.Max(_topLeft.Y, _bottomRight.Y);
    }

    public bool PointWithin(Vector2 point) {
        return point.X >= TopLeft.X && point.Y >= TopLeft.Y
            && point.X < BottomRight.X && point.Y < BottomRight.Y;
    }

    public bool Intersects(AABB other) {
        return other.TopLeft.X < BottomRight.X && other.TopLeft.Y < BottomRight.Y
            && other.BottomRight.X >= TopLeft.X && other.BottomRight.Y >= TopLeft.Y;
    }

    public AABB(Vector2 topLeft, Vector2 bottomRight) {
        _topLeft = topLeft;
        // use public setter for this, ensures order as a side effect
        BottomRight = bottomRight;
    }
}
