using Silk.NET.Input;
using System.Numerics;

namespace FWGPUE;

/// <summary>
/// Used with a <see cref="QuadTree"/> to get the position of an object.
/// </summary>
/// <typeparam name="T"></typeparam>
interface IPositioner<T> {
    public Vector2 GetPosition(T positioned);
}

/// <summary>
/// Quadtree.  Good for quick lookup by location of items.
/// </summary>
/// <typeparam name="TContents">Type of the contents.</typeparam>
/// <typeparam name="TPositioner">Positioner for the type.</typeparam>
class QuadTree<TContents, TPositioner>
    where TPositioner : IPositioner<TContents> {

    public AABB Bounds;
    public int SplitThreshold;
    IPositioner<TContents> Positioner;

    public List<TContents> LeafContents;
    public bool Leaf => LeafContents != null;

    QuadTree<TContents, TPositioner> TopLeft;
    QuadTree<TContents, TPositioner> TopRight;
    QuadTree<TContents, TPositioner> BottomLeft;
    QuadTree<TContents, TPositioner> BottomRight;
    public QuadTree<TContents, TPositioner>[] Children => new[] { TopLeft, TopRight, BottomLeft, BottomRight };

    void CreateChildren() {
        float halfWidth = Bounds.Width / 2;
        float halfHeight = Bounds.Height / 2;

        /* TopLeft is 1ACB
           TopRight is A2DC
           BottomLeft is BCE3
           BottomRight is CD4E
           1---A---2
           |       |
           B   C   D 
           |       |
           3---E---4 */
        Vector2 topMid = Bounds.TopLeft + new Vector2(halfWidth, 0);             //A
        Vector2 leftMid = Bounds.TopLeft + new Vector2(0, halfHeight);           //B
        Vector2 middle = Bounds.TopLeft + new Vector2(halfWidth, halfHeight);       //C
        Vector2 rightMid = Bounds.TopLeft + new Vector2(Bounds.Width, halfHeight);  //D
        Vector2 bottomMid = topMid + new Vector2(0, Bounds.Height);              //E

        TopLeft = new QuadTree<TContents, TPositioner>(new AABB(Bounds.TopLeft, middle));
        TopRight = new QuadTree<TContents, TPositioner>(new AABB(topMid, rightMid));
        BottomLeft = new QuadTree<TContents, TPositioner>(new AABB(leftMid, bottomMid));
        BottomRight = new QuadTree<TContents, TPositioner>(new AABB(middle, Bounds.BottomRight));
    }
    void MoveContentsToChildren() {
        // copy contents
        IEnumerable<TContents> contents = LeafContents;
        // no more contents, this tree is a branch now
        LeafContents = null;
        foreach (TContents item in contents) {
            Add(item);
        }
    }

    public bool Contains(TContents item, out QuadTree<TContents, TPositioner> container) {
        if (Leaf) {
            container = this;
            return LeafContents.Contains(item);
        }

        foreach (QuadTree<TContents, TPositioner> child in Children) {
            if (child.Contains(item, out QuadTree<TContents, TPositioner> _container)) {
                container = _container;
                return true;
            }
        }

        container = null;
        return false;
    }
    public IEnumerable<TContents> AllContents() {
        // if this tree is a leaf ..
        if (Leaf) {
            // .. just return all contents
            foreach (TContents item in LeafContents) {
                yield return item;
            }
        }
        // otherwise ..
        else {
            // recursively find leaves and return contents
            foreach (QuadTree<TContents, TPositioner> child in Children) {
                if (child != null) {
                    foreach (TContents item in child.AllContents()) {
                        yield return item;
                    }
                }
            }
        }
    }
    public IEnumerable<QuadTree<TContents, TPositioner>> AllChildren() {
        yield return this;
        foreach (QuadTree<TContents, TPositioner> child in Children) {
            if (child != null) {
                yield return child;
                foreach (QuadTree<TContents, TPositioner> childChild in child.AllChildren()) {
                    yield return childChild;
                }
            }
        }
    }

    public bool Add(TContents item) {
        bool success = false;

        // if this tree is a leaf, simply add to contents
        if (Leaf) {
            LeafContents.Add(item);
            success = true;
        }
        // otherwise, find which child to add to
        else {
            foreach (QuadTree<TContents, TPositioner> child in Children) {
                // if the item is within the bounds of child ..
                if (child.Bounds.PointWithin(Positioner.GetPosition(item))) {
                    // .. add it to child
                    success = child.Add(item);
                    break;
                }
            }
        }

        // if this tree was a leaf and is now too stuffed, split
        if (Leaf && LeafContents.Count > SplitThreshold) {
            CreateChildren();
            MoveContentsToChildren();
        }

        return success;
    }
    public bool Remove(TContents item) {
        // if this tree contains the item ..
        if (Contains(item, out QuadTree<TContents, TPositioner> container)) {
            // .. and if this tree is where the item is ..
            if (container == this) {
                // .. remove the item
                lock (LeafContents) {
                    LeafContents.Remove(item);
                }
                return true;
            }

            // .. otherwise remove from the tree that does contain it
            return container.Remove(item);
        }
        return false;
    }

    public IEnumerable<TContents> GetWithinRect(Vector2 topLeft, Vector2 bottomRight) {
        AABB rect = new AABB(topLeft, bottomRight);

        if (rect.Intersects(Bounds)) {
            if (Leaf) {
                foreach (TContents item in LeafContents) {
                    if (rect.PointWithin(Positioner.GetPosition(item))) {
                        yield return item;
                    }
                }
            }
            else {
                foreach (QuadTree<TContents, TPositioner> child in Children) {
                    if (child != null) {
                        foreach (TContents item in child.GetWithinRect(topLeft, bottomRight)) {
                            yield return item;
                        }
                    }
                }
            }
        }
    }
    public IEnumerable<TContents> GetWithinRadius(Vector2 point, float radius) {
        // query in the square which the circle circumscribes
        foreach (TContents item in GetWithinRect(point - new Vector2(radius), point + new Vector2(radius))) {
            // basic point / circle intersection
            float radiusSquared = radius * radius;
            Vector2 itemPos = Positioner.GetPosition(item);
            if ((itemPos - point).LengthSquared() <= radiusSquared) {
                yield return item;
            }
        }
    }

    public QuadTree(AABB bounds) {
        Bounds = bounds;
        Positioner = Activator.CreateInstance<TPositioner>();
    }
}
