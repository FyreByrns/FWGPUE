﻿namespace FWGPUE.Nodes;

delegate bool NodeFilter(Node2D node, object data);
delegate Node2D? NodeTransformer(Node2D node, object data);

class NodeCollection {
    /// <summary>
    /// Root for heirarchical tree based representation of nodes.
    /// </summary>
    public Node2D Root { get; } = new();
    /// <summary>
    /// Grid system for spatial representation of nodes.
    /// <para> Cleared and rebuilt per-frame. </para>
    /// </summary>
    public NodeGrid Grid { get; } = new();

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

    public Node2D? AddChild(Node2D node, NodeFilter parentFilter, object data = null) {
        IEnumerable<Node2D> possibleParents = GetNodes(parentFilter, data);
        if (!possibleParents.Any()) {
            Log.Error("filter for parent yielded no results.");
            return null;
        }

        return possibleParents.First().AddChild(node);
    }
    public bool Remove(NodeFilter removalFilter, object data = null) {
        IEnumerable<Node2D> removalCollection = GetNodes(removalFilter, data);
        if (!removalCollection.Any()) {
            return false;
        }

        foreach (Node2D node in removalCollection) {
            // if the parent node is null, that means we're trying to delete the root
            if (node.Parent is not null) {
                node.Parent.Children.Remove(node);
            }
        }
        return true;
    }

    #endregion wrangling

    #region drawing

    public void TickNodes() {
        // get all nodes
        var nodes = Root.AllNodes();

        //// clear nodegrid
        //Grid.Clear();
        //// register all nodes in the grid
        //foreach (Node2D node in nodes) {
        //    Grid.RegisterPosition(node, node.Offset);
        //}

        foreach (Node2D node in nodes) {
            if (node.Collection != this) {
                node.Collection = this;
                node.ReloadCache();
            }
            node.Tick();
        }
    }

    public void DrawNodes() {
        int screenEdgeBuffer = 800;
        AABB screen = new(Camera.ScreenToWorld(new(-screenEdgeBuffer, -screenEdgeBuffer)), Camera.ScreenToWorld(new(Config.ScreenWidth + screenEdgeBuffer, Config.ScreenHeight + screenEdgeBuffer)));

        foreach (Node2D node in Grid.GetNodesInArea(screen).OrderByDescending(x => x.Z)) {
            node.Draw();
        }
    }

    /// <summary>
    /// Draw connections between nodes.
    /// </summary>
    public void DrawDebugConnections() {
        foreach (Node2D node in GetNodes(NodeFilters.Visible).Reverse()) {
            if (node.Parent != null) {
                Renderer.PushLine(node.RelativeOffset(), node.Parent.RelativeOffset(), 1, System.Numerics.Vector3.One);
            }
        }
    }
    public void DrawDebugNodes() {

    }

    #endregion drawing
}
