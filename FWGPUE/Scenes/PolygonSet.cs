using System.Numerics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FWGPUE.Scenes;

// for future refactoring ease
using Vertex = Vector2;

class PolygonSet : EngineFile, IEnumerable<Vertex>, IEnumerable<List<Vertex>> {
    public const int SaveVersion = 1;
    /// <summary>
    /// 2 floats (x, y)
    /// </summary>
    public const int VertexLength = 2;

    public List<List<Vertex>> Polygons = new();

    public PolygonSet(EngineFileLocation? location) : base(location ?? "assets/default.fwvf") { }
    public PolygonSet() : this(default(EngineFileLocation)) { }
    public PolygonSet(Vertex[][] polygons) : this(default(EngineFileLocation)) {
        foreach (Vertex[] v in polygons) {
            Polygons.Add(v.ToList());
        }
    }

    public static implicit operator PolygonSet(Vertex[][] polygons) {
        return new PolygonSet(polygons);
    }

    public void Add(IEnumerable<Vertex> poly) {
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
        if (polyOrSet is IEnumerable<IEnumerable<Vertex>> set) {
            foreach (var polyInSet in set) {
                Add(polyInSet);
            }
        }
        if (polyOrSet is IEnumerable<Vertex> poly) {
            Add(poly);
        }
    }

    #region save / load
    protected override void ReadData(byte[] data) {
        int readIndex = 0;

        int polygonCount = BitConverter.ToInt32(data, readIndex);
        readIndex += sizeof(int);

        Polygons = new(polygonCount);
        for(int polygon = 0; polygon < polygonCount; polygon++) {
            int vertexCount = BitConverter.ToInt32(data, readIndex);
            readIndex += sizeof(int);

            List<Vertex> vertices = new(vertexCount);
            for(int vertex = 0; vertex < vertexCount; vertex++) {
                int vertexSize = BitConverter.ToInt32(data, readIndex);
                readIndex += sizeof(int);

                if(vertexSize != VertexLength) {
                    Log.Warn($"serialized vertex size {vertexSize} does not match expected value {VertexLength}");
                }

                float x = BitConverter.ToSingle(data, readIndex);
                readIndex += sizeof(float);
                float y = BitConverter.ToSingle(data, readIndex);
                readIndex += sizeof(float);

                vertices.Add(new(x, y));
            }

            Polygons.Add(vertices);
        }
    }

    protected override byte[] SaveData() {
        List<byte> data = new List<byte>();

        data.AddRange(BitConverter.GetBytes(Polygons.Count));
        foreach(List<Vertex> polygon in Polygons) {
            data.AddRange(BitConverter.GetBytes(polygon.Count));

            foreach(Vertex vertex in polygon) {
                data.AddRange(BitConverter.GetBytes(VertexLength));

                data.AddRange(BitConverter.GetBytes(vertex.X));
                data.AddRange(BitConverter.GetBytes(vertex.Y));
            }
        }

        return data.ToArray();
    }
    #endregion

    #region enumerable
    public IEnumerator GetEnumerator() {
        foreach (var poly in Polygons) {
            foreach (var vertex in poly) {
                yield return vertex;
            }
        }
    }
    IEnumerator<Vertex> IEnumerable<Vertex>.GetEnumerator() {
        foreach (var poly in Polygons) {
            foreach (var vertex in poly) {
                yield return vertex;
            }
        }
    }
    IEnumerator<List<Vertex>> IEnumerable<List<Vertex>>.GetEnumerator() {
        foreach (var poly in Polygons) {
            yield return poly;
        }
    }
    #endregion enumerable
}
