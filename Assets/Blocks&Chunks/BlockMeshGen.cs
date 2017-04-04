using UnityEngine;
using System.Collections;



public static class BlockMeshGen
{

    public static readonly byte LFT = 1;
    public static readonly byte RGT = 2;
    public static readonly byte BTM = 4;
    public static readonly byte TOP = 8;
    public static readonly byte BCK = 16;
    public static readonly byte FWD = 32;


    public static PrimVUT Draw(byte data, byte block, int x, int y, int z)
    {
        byte vLength;
        byte vCount = 0;

        vLength = (byte)(
            ((data & LFT) << 2) + //each side will have 4 vertices; since left = 1, left * 4 = 4; right * 2 = 4; and so on...
            ((data & RGT) << 1) +
            (data & BTM) +
            ((data & TOP) >> 1) +
            ((data & BCK) >> 2) +
            ((data & FWD) >> 3)
        );

        Vector3[] vertices = new Vector3[vLength];
        Vector2[] uvs = new Vector2[vLength];
        int[] triangles = new int[vLength * 3 / 2]; //Amount of Triangles is always 3/2 of the Amount of Vertices

        if ((data & LFT) == LFT)
        {
            //Debug.Log ("Left");
            vertices = fillVertices(vertices, Quad.left, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;
        }
        if ((data & RGT) == RGT)
        {
            //Debug.Log ("Right");
            vertices = fillVertices(vertices, Quad.right, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & BTM) == BTM)
        {
            //Debug.Log ("Bottom");
            vertices = fillVertices(vertices, Quad.bottom, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & TOP) == TOP)
        {
            //Debug.Log ("Top");
            vertices = fillVertices(vertices, Quad.top, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & BCK) == BCK)
        {
            //Debug.Log ("Back");
            vertices = fillVertices(vertices, Quad.back, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & FWD) == FWD)
        {
            //Debug.Log ("Front");
            vertices = fillVertices(vertices, Quad.fwd, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        vertices = addPos(vertices, x, y, z);
        triangles = coordinateTriangles(triangles);
        return new PrimVUT(vertices, uvs, triangles); ;
    }
    public static PrimVUT DrawBlock(byte block, byte data, Index index)
    {
        byte vLength;
        byte vCount = 0;

        vLength = (byte)(
            ((data & LFT) << 2) + //each side will have 4 vertices; since left = 1, left * 4 = 4; right * 2 = 4; and so on...
            ((data & RGT) << 1) +
            (data & BTM) +
            ((data & TOP) >> 1) +
            ((data & BCK) >> 2) +
            ((data & FWD) >> 3)
        );

        Vector3[] vertices = new Vector3[vLength];
        Vector2[] uvs = new Vector2[vLength];
        int[] triangles = new int[vLength * 3 / 2]; //Amount of Triangles is always 3/2 of the Amount of Vertices

        if ((data & LFT) == LFT)
        {
            //Debug.Log ("Left");
            vertices = fillVertices(vertices, Quad.left, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;
        }
        if ((data & RGT) == RGT)
        {
            //Debug.Log ("Right");
            vertices = fillVertices(vertices, Quad.right, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & BTM) == BTM)
        {
            //Debug.Log ("Bottom");
            vertices = fillVertices(vertices, Quad.bottom, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & TOP) == TOP)
        {
            //Debug.Log ("Top");
            vertices = fillVertices(vertices, Quad.top, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & BCK) == BCK)
        {
            //Debug.Log ("Back");
            vertices = fillVertices(vertices, Quad.back, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & FWD) == FWD)
        {
            //Debug.Log ("Front");
            vertices = fillVertices(vertices, Quad.fwd, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        vertices = addPos(vertices, index);
        triangles = coordinateTriangles(triangles);
        return new PrimVUT(vertices, uvs, triangles); ;
    }
    public static PrimVUT DrawQuad(byte block, byte data, Index index)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6];

        if ((data & LFT) == LFT)
        {
            vertices = fillVertices(vertices, Quad.left, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);
        }
        else if ((data & RGT) == RGT)
        {
            vertices = fillVertices(vertices, Quad.right, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);

        }
        else if ((data & BTM) == BTM)
        {
            vertices = fillVertices(vertices, Quad.bottom, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);

        }
        else if ((data & TOP) == TOP)
        {
            vertices = fillVertices(vertices, Quad.top, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);

        }
        else if ((data & BCK) == BCK)
        {
            vertices = fillVertices(vertices, Quad.back, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);
        }
        else
        {
            vertices = fillVertices(vertices, Quad.fwd, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);
        }
        vertices = addPos(vertices, index);
        triangles = coordinateTriangles(triangles);
        return new PrimVUT(vertices, uvs, triangles);
    }

    static Vector3[] fillVertices(Vector3[] what, Vector3[] with, byte start)
    {
        for (byte i = 0; i < with.Length; i++)
        {
            what[i + start] = with[i];
        }
        return what;
    }
    static Vector2[] fillUVS(Vector2[] what, Vector2[] with, byte start)
    {
        for (byte i = 0; i < with.Length; i++)
        {
            what[i + start] = with[i];
        }
        return what;
    }

    static int[] coordinateTriangles(int[] what)
    {
        int j = 0;
        int length = (what.Length << 1) / 3;
        for (int i = 0; i < length; i += 4)
        {
            what[j++] = i;
            what[j++] = 1 + i;
            what[j++] = 2 + i;
            what[j++] = 2 + i;
            what[j++] = 1 + i;
            what[j++] = 3 + i;
        }
        return what;
    }
    static Vector3[] addPos(Vector3[] what, int x, int y, int z)
    {
        Vector3 addition = new Vector3(x, y, z);
        for (int i = 0; i < what.Length; i++)
        {
            what[i] += addition;
        }
        return what;
    }
    static Vector3[] addPos(Vector3[] what, Index index)
    {
        Vector3 addition = index.Vector;
        for (int i = 0; i < what.Length; i++)
        {
            what[i] += addition;
        }
        return what;
    }

    public static void setMesh(Mesh what, byte data)
    {
        PrimVUT vut = DrawBlock(1, data, new Index(0, 0, 0));
        what.vertices = vut.vertices;
        what.triangles = vut.triangles;
        what.uv = vut.uvs;
    }
}