namespace FWGPUE.Nodes;

static class NodeFilters {
    public static NodeFilter And(this NodeFilter me, NodeFilter other) {
        return (n, d) => me(n, d) && other(n, d);
    }
    public static NodeFilter Or(this NodeFilter me, NodeFilter other) {
        return (n, d) => me(n, d) || other(n, d);
    }

    public static bool ByName(Node2D n, object d) {
        return n.Name == (string)d;
    }

    public static bool HasParent(Node2D n, object d) {
        return n.Parent == (Node2D)d;
    }

    /// <summary>
    /// Whether the node is visible.
    /// <para>This requires this node to be visible and all parents to be visible.</para>
    /// </summary>
    public static bool Visible(Node2D n, object d) {
        if (n.Visible) {
            foreach (Node2D above in n.NodesToThisFromBase()) {
                if (!above.Visible) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public static NodeFilter ChildOf(Node2D parent) {
        return (n, d) => parent == n;
    }
    public static NodeFilter SiblingOf(Node2D node) {
        return (n, d) => n != node && (n.Parent?.Children.Contains(node) ?? false);
    }
}
