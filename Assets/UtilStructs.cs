using UnityEngine;
using System.Collections.Generic;

public struct BlockVector
{
    public static readonly Vector3 zero = new Vector3(-.5f, -.5f, -.5f);
    public static readonly Vector3 fwd = new Vector3(-.5f, -.5f, .5f);
    public static readonly Vector3 top = new Vector3(-.5f, .5f, -.5f);
    public static readonly Vector3 topFwd = new Vector3(-.5f, .5f, .5f);
    public static readonly Vector3 right = new Vector3(.5f, -.5f, -.5f);
    public static readonly Vector3 rightFwd = new Vector3(.5f, -.5f, .5f);
    public static readonly Vector3 rightTop = new Vector3(.5f, .5f, -.5f);
    public static readonly Vector3 rightTopFwd = new Vector3(.5f, .5f, .5f);
}
public struct Quad
{
    //UVS
    public static readonly Vector2[] uvDefault = { new Vector2(0.05f, .05f), new Vector2(.05f, .95f), new Vector2(.95f, .05f), new Vector2(.95f, .95f) };
    //NORMALS
    public static readonly Vector3[] nLeft = { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
    public static readonly Vector3[] nRight = { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
    public static readonly Vector3[] nBottom = { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
    public static readonly Vector3[] nTop = { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
    public static readonly Vector3[] nBack = { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
    public static readonly Vector3[] nFwd = { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
    //VERTICES
    public static readonly Vector3[] left = { BlockVector.zero, BlockVector.fwd, BlockVector.top, BlockVector.topFwd };
    public static readonly Vector3[] right = { BlockVector.rightFwd, BlockVector.right, BlockVector.rightTopFwd, BlockVector.rightTop };
    public static readonly Vector3[] bottom = { BlockVector.rightFwd, BlockVector.fwd, BlockVector.right, BlockVector.zero };
    public static readonly Vector3[] top = { BlockVector.rightTop, BlockVector.top, BlockVector.rightTopFwd, BlockVector.topFwd };
    public static readonly Vector3[] back = { BlockVector.right, BlockVector.zero, BlockVector.rightTop, BlockVector.top };
    public static readonly Vector3[] fwd = { BlockVector.fwd, BlockVector.rightFwd, BlockVector.topFwd, BlockVector.rightTopFwd };

}
public struct VUT
{
    public List<Vector3> vertices;
    public List<Vector2> uvs;
    public List<int> triangles;

    public VUT(List<Vector3> v, List<Vector2> u, List<int> t)
    {
        vertices = v;
        uvs = u;
        triangles = t;
    }
    public void ToMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
/*
public struct MyVut<T>
{
    public IEnumerable<T> vertices;
    public IEnumerable<T> uvs;
    public IEnumerable<T> triangles;

    public MyVut(IEnumerable<T> v, IEnumerable<T> u, IEnumerable<T> t)
    {
        vertices = v;
        uvs = u;
        triangles = t;
    }
    public void Add(T vertex, T uv, T triangle)
    public void ex()
    {
        MyVut<Vector3[]> hi = new MyVut<Vector3[]>();
        MyVut<List<Vector3>> hola = new MyVut<List<Vector3>>();
    }
}*/
public struct PrimVUT
{
    public Vector3[] vertices;
    public Vector2[] uvs;
    public int[] triangles;

    public PrimVUT(Vector3[] v, Vector2[] u, int[] t)
    {
        vertices = v;
        uvs = u;
        triangles = t;
    }
    public void ToMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    public VUT ToVUT()
    {
        List<Vector3> v = new List<Vector3>(vertices);
        List<Vector2> u = new List<Vector2>(uvs);
        List<int> t = new List<int>(triangles);
        return new VUT(v, u, t);
    }
}
public struct Index
{
    public int i, j, k;

    public Vector3 Position
    {
        get
        {
            return new Vector3(i * Chunk.WIDTH + World.position.x, j * Chunk.HEIGHT + World.position.y, k * Chunk.WIDTH + World.position.z);
        }
    }
    public Vector3 Vector
    {
        get
        {
            return new Vector3(i, j, k);
        }
    }
    public Index(int i, int k)
    {
        this.i = i;
        this.j = 0;
        this.k = k;
    }
    public Index(int i, int j, int k)
    {
        this.i = i;
        this.j = j;
        this.k = k;
    }
    public Index(Index index)
    {
        this.i = index.i;
        this.j = index.j;
        this.k = index.k;
    }
    public override string ToString()
    {
        return string.Format("({0}, {1}, {2})[{3}, {4}, {5}]", i, j, k, i * Chunk.WIDTH, j * Chunk.HEIGHT, k * Chunk.WIDTH);
    }
}
public struct Index2
{
    public Index chunkIndex;
    public Index blockIndex;
    public Vector3 Position
    {
        get
        {
            return chunkIndex.Position + blockIndex.Vector;
        }
    }

    public Index2(int i, int j, int k, int i2, int j2, int k2)
    {
        this.chunkIndex = new Index(i, j, k);
        this.blockIndex = new Index(i2, j2, k2);
    }
    public Index2(Index chunkIndex, Index blockIndex)
    {
        this.chunkIndex = chunkIndex;
        this.blockIndex = blockIndex;
    }
    public override string ToString()
    {
        return string.Format(
            "CHUNK:({0}, {1}, {2})BLOCK:({3}, {4}, {5})",
            chunkIndex.i, chunkIndex.j, chunkIndex.k,
            blockIndex.i, blockIndex.j, blockIndex.k
        );
    }
}