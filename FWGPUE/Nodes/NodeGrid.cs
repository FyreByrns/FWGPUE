using System.Numerics;

namespace FWGPUE.Nodes;

class NodeGrid {
    public const int GridSize = 100;

    /// <summary>
    /// Stores the nodes.
    /// </summary>
    public Dictionary<NodeGridCoordinates, List<NodePositionPair>> Grid = new();
    public void Clear() {
        Grid.Clear();
    }

    public bool TryGetSquare(NodeGridCoordinates coordinates, out List<NodePositionPair> nodesInSquare) {
        return Grid.TryGetValue(coordinates, out nodesInSquare);
    }

    public void RegisterPosition(Node2D node, Vector2 position) {
        NodeGridCoordinates coordinates = GetGridCoordinates(position);

        if (!Grid.ContainsKey(coordinates)) {
            Grid.Add(coordinates, new List<NodePositionPair>());
        }
        Grid[coordinates].Add(new(position, node));
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
                    foreach (var nodePositionPair in nodes) {
                        if (area.PointWithin(nodePositionPair.Position)) {
                            yield return nodePositionPair.Node;
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
