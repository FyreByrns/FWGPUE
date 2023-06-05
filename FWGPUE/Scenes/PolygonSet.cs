using System.Numerics;
using System.Collections;

namespace FWGPUE.Scenes;

class PolygonSet : IEnumerable<Vector2>, IEnumerable<List<Vector2>> {
    public List<List<Vector2>> Polygons = new();

    public PolygonSet() { }
    public PolygonSet(Vector2[][] polygons) {
        foreach (Vector2[] v in polygons) {
            Polygons.Add(v.ToList());
        }
    }

    public static implicit operator PolygonSet(Vector2[][] polygons) {
        return new PolygonSet(polygons);
    }

    public void Add(IEnumerable<Vector2> poly) {
        Polygons.Add(poly.ToList());
    }
    public void Add(PolygonSet set) {
        Polygons.AddRange(set.Polygons);
    }

    /// <summary>
    /// Add either a collection of v2s (individual polygons) or a collection of collections of v2s (collections of polygons)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="polyOrSet"></param>
    public void Add<T>(T polyOrSet) {
        if (polyOrSet is IEnumerable<IEnumerable<Vector2>> set) {
            foreach (var polyInSet in set) {
                Add(polyInSet);
            }
        }
        if (polyOrSet is IEnumerable<Vector2> poly) {
            Add(poly);
        }
    }

    #region enumerable
    public IEnumerator GetEnumerator() {
        foreach (var poly in Polygons) {
            foreach (var vertex in poly) {
                yield return vertex;
            }
        }
    }
    IEnumerator<Vector2> IEnumerable<Vector2>.GetEnumerator() {
        foreach (var poly in Polygons) {
            foreach (var vertex in poly) {
                yield return vertex;
            }
        }
    }
    IEnumerator<List<Vector2>> IEnumerable<List<Vector2>>.GetEnumerator() {
        foreach (var poly in Polygons) {
            yield return poly;
        }
    }
    #endregion enumerable
}
