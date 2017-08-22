using UnityEngine;
using System.Collections.Generic;

// Used for the side thread in ChunkMeshGenerator
public struct Work
{
    public Chunk CenterChunk { get; set; }
    public Index CenterBlockIndex { get; set; }
    public Shape Shape { get; set; }

    public Work(Chunk centerChunk, Index centerBlockIndex, Shape shape)
    {
        CenterChunk = centerChunk;
        CenterBlockIndex = centerBlockIndex;
        Shape = shape;
    }

    public void Start()
    {
        Shape.Draw(CenterChunk, CenterBlockIndex);
    }
}
// The 8 vertices for a Block
public struct BlockVertices
{
    public static readonly Vector3 Zero = new Vector3(-.5f, -.5f, -.5f);
    public static readonly Vector3 Fwd = new Vector3(-.5f, -.5f, .5f);
    public static readonly Vector3 Top = new Vector3(-.5f, .5f, -.5f);
    public static readonly Vector3 TopFwd = new Vector3(-.5f, .5f, .5f);
    public static readonly Vector3 Rgt = new Vector3(.5f, -.5f, -.5f);
    public static readonly Vector3 RgtFwd = new Vector3(.5f, -.5f, .5f);
    public static readonly Vector3 RgtTop = new Vector3(.5f, .5f, -.5f);
    public static readonly Vector3 RgtTopFwd = new Vector3(.5f, .5f, .5f);
}
public struct Quad
{
    //UVS
    public static readonly Vector2[] UvDefault = { new Vector2(0.05f, .05f), new Vector2(.05f, .95f), new Vector2(.95f, .05f), new Vector2(.95f, .95f) };
    //NORMALS
    public static readonly Vector3[] LeftN = { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
    public static readonly Vector3[] RightN = { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
    public static readonly Vector3[] DownN = { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
    public static readonly Vector3[] UpN = { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
    public static readonly Vector3[] BackN = { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
    public static readonly Vector3[] ForwardN = { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
    //VERTICES
    public static readonly Vector3[] Lft = { BlockVertices.Zero, BlockVertices.Fwd, BlockVertices.Top, BlockVertices.TopFwd };
    public static readonly Vector3[] Rgt = { BlockVertices.RgtFwd, BlockVertices.Rgt, BlockVertices.RgtTopFwd, BlockVertices.RgtTop };
    public static readonly Vector3[] Btm = { BlockVertices.RgtFwd, BlockVertices.Fwd, BlockVertices.Rgt, BlockVertices.Zero };
    public static readonly Vector3[] Top = { BlockVertices.RgtTop, BlockVertices.Top, BlockVertices.RgtTopFwd, BlockVertices.TopFwd };
    public static readonly Vector3[] Bck = { BlockVertices.Rgt, BlockVertices.Zero, BlockVertices.RgtTop, BlockVertices.Top };
    public static readonly Vector3[] Fwd = { BlockVertices.Fwd, BlockVertices.RgtFwd, BlockVertices.TopFwd, BlockVertices.RgtTopFwd };

}
// Used for Chunk mesh data
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
    public void Clear()
    {
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
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
// Used for Block mesh data, where it is easy to calculate the number of vertices needed for allocation
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
// Useful for 3D coordinates (when locating a Chunk within the World or maybe a Block within a Chunk)
public struct Index
{
    public int i, j, k;
    public static readonly Index Zero = new Index(0, 0, 0);

    public Vector3 Position
    {
        get
        {
            return new Vector3(i * Chunk.Width, j * Chunk.Height, k * Chunk.Width);
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
        return string.Format("({0}, {1}, {2})[{3}, {4}, {5}]", i, j, k, i * Chunk.Width, j * Chunk.Height, k * Chunk.Width);
    }
}
// Useful for locating a block. (since a Block is essentially in a 6D space comprised of Blocks and wrapper Chunks)
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