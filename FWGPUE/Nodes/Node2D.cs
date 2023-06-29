using FWGPUE.Graphics;
using FWGPUE.IO;
using System.Numerics;

namespace FWGPUE.Nodes;
class Node2D {
    public string? Name { get; set; }

    public bool Visible { get; set; } = true;

    public Node2D? Parent { get; protected set; }
    public bool IsBase => Parent is null;

    // local attributes
    public Vector2 LocalOffset;
    public float LocalZ;
    public Vector2 LocalScale = Vector2.One;
    public float LocalRotation;

    // attributes /relative to all preceding nodes/
    public Vector2 Offset => RelativeOffset();
    public float Z => RelativeZ();
    public Vector2 Scale => RelativeScale();
    public float Rotation => RelativeRotation();

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
