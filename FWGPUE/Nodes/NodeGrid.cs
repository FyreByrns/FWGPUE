using System.Numerics;

namespace FWGPUE.Nodes;

using NodePositionSet = Dictionary<Node2D, Vector2>;

class NodeGrid {
    public const int GridSize = 100;

    public Dictionary<Node2D, NodeGridCoordinates> PositionsByNode = new();
    /// <summary>
    /// Stores the nodes.
    /// </summary>
    public Dictionary<NodeGridCoordinates, NodePositionSet> Grid = new();
    public void Clear() {
        Grid.Clear();
    }

    public bool TryGetSquare(NodeGridCoordinates coordinates, out NodePositionSet nodesInSquare) {
        return Grid.TryGetValue(coordinates, out nodesInSquare);
    }

    /// <summary>
    /// Remove the recorded position of a node.
    /// </summary>
    public void RemoveRegistery(Node2D node) {
        // can't remove anything if there isn't anything
        if (Grid.Count == 0) {
            return;
        }

        if (PositionsByNode.TryGetValue(node, out var grid)) {
            if (!Grid.ContainsKey(grid)) {
                Log.Warn(Grid.Count);
                Log.Warn("attempted removal of node from grid failed: can't find node");
            }
            else {
                Grid[grid].Remove(node);
            }
        }
    }

    public void RegisterPosition(Node2D node, Vector2 position) {
        NodeGridCoordinates coordinates = GetGridCoordinates(position);
        PositionsByNode[node] = coordinates;

        if (!Grid.ContainsKey(coordinates)) {
            Grid.Add(coordinates, new NodePositionSet());
        }
        Grid[coordinates][node] = position;
    }

    public IEnumerable<Node2D> GetNodesInArea(AABB area) {
        NodeGridCoordinates topLeftSearchSquare = GetGridCoordinates(area.TopLeft - new Vector2(GridSize));
        NodeGridCoordinates bottomRightSearchSquare = GetGridCoordinates(area.BottomRight + new Vector2(GridSize));

        for (int x = topLeftSearchSquare.X; x <= bottomRightSearchSquare.X; x++) {
            for (int y = topLeftSearchSquare.Y; y <= bottomRightSearchSquare.Y; y++) {
                NodeGridCoordinates currentSquare = new(x, y);

                // if there's a grid square here .. 
                if (TryGetSquare(currentSquare, out var nodes)) {
                    // .. check the fine coordinates of all nodes, return all within aabb
                    NodePositionSet snapshot = new(nodes);
                    foreach (var nodePositionPair in snapshot) {
                        if (area.PointWithin(nodePositionPair.Value)) {
                            yield return nodePositionPair.Key;
                        }
                    }
                }
            }
        }
    }
    public IEnumerable<Node2D> GetNodesInCircle(Circle circle) {
        AABB circleBounds = new(circle.position - new Vector2(circle.radius), circle.position + new Vector2(circle.radius));

        foreach (var node in GetNodesInArea(circleBounds)) {
            if ((node.Offset - circle.position).Length() <= circle.radius) {
                yield return node;
            }
        }
    }

    public NodeGridCoordinates GetGridCoordinates(Vector2 worldPosition) {
        return new((int)MathF.Floor(worldPosition.X / GridSize), (int)MathF.Floor(worldPosition.Y / GridSize));
    }
}
