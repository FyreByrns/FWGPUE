using FWGPUE.IO;
using System.Numerics;

namespace FWGPUE;

class Node2D {
    public Node2D? Parent { get; protected set; }
    public bool IsBase => Parent is null;

    public Vector2 Offset;
    public float Rotation;

    public float RelativeRotation() {
        float rot = 0;
        foreach (Node2D node in NodesToThisFromBase()) {
            rot += node.Rotation;
        }
        return rot;
    }

    /// <summary>
    /// Get transform relative to the base node.
    /// </summary>
    public Vector2 RelativeOffset() {
        Vector2 offset = new(0);
        float rotation = 0;

        // loop through all nodes adding offsets
        foreach (Node2D node in NodesToThisFromBase()) {
            float cos = (float)Math.Cos(Engine.TurnsToRadians(rotation));
            float sin = (float)Math.Sin(Engine.TurnsToRadians(rotation));

            float dx = node.Offset.X;
            float dy = node.Offset.Y;

            float x = cos * dx - sin * dy;
            float y = sin * dx + cos * dy;

            offset.X += x;
            offset.Y += y;
            rotation += node.Rotation;
        }

        return offset;
    }

    public HashSet<Node2D> Children { get; } = new();
    public Node2D AddChild(Node2D node) {
        node.Parent = this;
        Children.Add(node);
        return node;
    }
    public Node2D AddSibling(Node2D node) {
        if (IsBase) {
            Log.Error("can't add sibling to base node");
            return this;
        }
        return Parent.AddChild(node);
    }

    public IEnumerable<Node2D> AllNodes() {
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
        Stack<Node2D> path = new Stack<Node2D>();

        while (current != null) {
            path.Push(current);
            current = current.Parent;
        }

        while (path.TryPop(out var node)) {
            yield return node;
        }
    }
}
