namespace FWGPUE.Nodes;

static class NodeTransformers
{
    public static Node2D? AddChild(Node2D node, Node2D child)
    {
        return node.AddChild(child);
    }
    public static Node2D? AddSibling(Node2D node, Node2D sibling)
    {
        return node.AddSibling(sibling);
    }
}
