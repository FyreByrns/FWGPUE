using FWGPUE.Graphics;
using FWGPUE.IO;
using System.Numerics;

namespace FWGPUE.Nodes;
class Node2D {
    public NodeCollection Collection { get; set; }

    public string? Name { get; set; }

    public bool Visible { get; set; } = true;

    public Node2D? Parent { get; protected set; }
    public bool IsBase => Parent is null;

    // local attributes
    Vector2 _localOffset;
    float _localZ;
    Vector2 _localScale = Vector2.One;
    float _localRotation;

    public Vector2 LocalOffset {
        get {
            return _localOffset;
        }
        set {
            _localOffset = value;
            CacheReloadNeeded = true;
        }
    }
    public float LocalZ {
        get {
            return _localZ;
        }
        set {
            _localZ = value;
            CacheReloadNeeded = true;
        }
    }
    public Vector2 LocalScale {
        get {
            return _localScale;
        }
        set {
            _localScale = value;
            CacheReloadNeeded = true;
        }
    }
    public float LocalRotation {
        get {
            return _localRotation;
        }
        set {
            _localRotation = value;
            CacheReloadNeeded = true;
        }
    }

    // attributes /relative to all preceding nodes/
    public bool CacheReloadNeeded { get; protected set; } = true;

    Vector2 _cachedOffset = Vector2.Zero;
    float _cachedZ = 0;
    Vector2 _cachedScale = Vector2.One;
    float _cachedRotation = 0;

    public Vector2 Offset {
        get {
            ReloadCacheIfNeeded();
            return _cachedOffset;
        }
    }
    public float Z {
        get {
            ReloadCacheIfNeeded();
            return _cachedZ;
        }
    }
    public Vector2 Scale {
        get {
            ReloadCacheIfNeeded();
            return _cachedScale;
        }
    }
    public float Rotation {
        get {
            ReloadCacheIfNeeded();
            return _cachedRotation;
        }
    }

    public void ReloadCacheIfNeeded() {
        if (CacheReloadNeeded) {
            ReloadCache();
        }
    }
    public void ReloadCache() {
        // recalculate values
        _cachedOffset = RelativeOffset();
        _cachedZ = RelativeZ();
        _cachedScale = RelativeScale();
        _cachedRotation = RelativeRotation();

        // register position in grid
        Collection?.Grid.RemoveRegistery(this);
        Collection?.Grid.RegisterPosition(this, _cachedOffset);
    }

    /// <summary>
    /// Get transform relative to the base node.
    /// </summary>
    public Vector2 RelativeOffset() {
        Vector2 offset = new(0);
        float rotation = 0;

        // loop through all nodes adding offsets
        foreach (Node2D node in NodesToThisFromBase()) {
            float cos = (float)Math.Cos(TurnsToRadians(rotation));
            float sin = (float)Math.Sin(TurnsToRadians(rotation));

            float dx = node.LocalOffset.X;
            float dy = node.LocalOffset.Y;

            float x = cos * dx - sin * dy;
            float y = sin * dx + cos * dy;

            offset.X += x;
            offset.Y += y;
            rotation += node.LocalRotation;
        }

        return offset;
    }
    public float RelativeZ() {
        float result = 0;
        foreach (Node2D node in NodesToThisFromBase()) {
            result += node.LocalZ;
        }

        return result;
    }
    public Vector2 RelativeScale() {
        Vector2 result = Vector2.One;
        foreach (Node2D node in NodesToThisFromBase()) {
            result *= node.LocalScale;
        }

        return result;
    }
    public float RelativeRotation() {
        float rot = 0;
        foreach (Node2D node in NodesToThisFromBase()) {
            rot += node.LocalRotation;
        }
        return rot;
    }

    public HashSet<Node2D> Children { get; } = new();
    public Node2D AddChild(Node2D node) {
        node.Parent = this;
        Children.Add(node);
        return node;
    }
    public Node2D? AddSibling(Node2D node) {
        if (IsBase) {
            Log.Error("can't add sibling to base node");
            return this;
        }
        return Parent?.AddChild(node);
    }

    public virtual IEnumerable<Node2D> AllNodes() {
        foreach (Node2D child in Children) {
            foreach (Node2D c in child.AllNodes()) {
                yield return c;
            }
        }
        yield return this;
    }
    public IEnumerable<Node2D> NodesToThisFromBase() {
        // get path to current node
        Node2D? current = this;
        Stack<Node2D> path = new();

        while (current != null) {
            path.Push(current);
            current = current.Parent;
        }

        while (path.TryPop(out var node)) {
            yield return node;
        }
    }

    public virtual void Tick() { }
    public virtual void Draw() { }
}
