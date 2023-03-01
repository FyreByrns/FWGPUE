namespace FWGPUE.Nodes;

class NodeCollection {
    public delegate bool NodeFilter(Node2D node, object data);
    public delegate Node2D? NodeTransformer(Node2D node, object data);

    public Node2D Root { get; } = new();

    #region wrangling

    /// <summary>
    /// Get nodes matching a specified filter.
    /// </summary>
    public IEnumerable<Node2D> GetNodes(NodeFilter filter) {
        return GetNodes(filter, null);
    }
    /// <summary>
    /// <inheritdoc cref="GetNodes(NodeFilter)"/>
    /// </summary>
    public IEnumerable<Node2D> GetNodes(NodeFilter filter, object data) {
        foreach (Node2D node in Root.AllNodes()) {
            if (filter(node, data)) {
                yield return node;
            }
        }
    }

    public Node2D? TransformNode(NodeTransformer transformer, object data, Node2D node) {
        return transformer(node, data);
    }
    public void TransformNodes(NodeTransformer transformer, object data, params Node2D[] nodes) => TransformNodes(transformer, data, nodes);
    public IEnumerable<Node2D> TransformNodes(NodeTransformer transformer, object data, IEnumerable<Node2D> toTransform) {
        foreach (Node2D node in toTransform) {
            Node2D? result = TransformNode(transformer, data, node);

            if (result is not null) {
                yield return result;
            }
        }
    }

    #endregion wrangling

    #region drawing

    public void TickNodes() {
        foreach (Node2D node in Root.AllNodes()) {
            node.Tick();
        }
    }

    public void DrawNodes() {
        foreach (Node2D node in GetNodes(NodeFilters.Visible)) {
            node.Draw();
        }
    }

    /// <summary>
    /// Draw connections between nodes.
    /// </summary>
    public void DrawDebugConnections() {
        foreach (Node2D node in Root.AllNodes()) {
            if (node.Parent is not null && !node.Parent.IsBase) {
                DrawLine(new(1, 1, 1), node.Parent.RelativeOffset(), node.RelativeOffset());
            }
        }
    }

    #endregion drawing
}
