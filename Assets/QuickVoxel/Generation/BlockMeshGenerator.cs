using UnityEngine;
using System.Collections;

public static class BlockMeshGenerator
{

    public static readonly byte LFT = 1;
    public static readonly byte RGT = 2;
    public static readonly byte BTM = 4;
    public static readonly byte TOP = 8;
    public static readonly byte BCK = 16;
    public static readonly byte FWD = 32;

    private static readonly int LastBlockIndexXZ = Chunk.Width - 1;
    private static readonly int LastBlockIndexY = Chunk.Height - 1;

    /*
    public static PrimVUT Draw(byte data, byte block)
    {
        byte vLength;
        byte vCount = 0;

        vLength = (byte)(
            ((data & LFT) << 2) +
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
            vertices = fillVertices(vertices, Quad.Lft, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;
        }
        if ((data & RGT) == RGT)
        {
            //Debug.Log ("Right");
            vertices = fillVertices(vertices, Quad.Rgt, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & BTM) == BTM)
        {
            //Debug.Log ("Bottom");
            vertices = fillVertices(vertices, Quad.Btm, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & TOP) == TOP)
        {
            //Debug.Log ("Top");
            vertices = fillVertices(vertices, Quad.Top, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & BCK) == BCK)
        {
            //Debug.Log ("Back");
            vertices = fillVertices(vertices, Quad.Bck, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & FWD) == FWD)
        {
            //Debug.Log ("Front");
            vertices = fillVertices(vertices, Quad.Fwd, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        triangles = coordinateTriangles(triangles);
        return new PrimVUT(vertices, uvs, triangles);
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
            vertices = fillVertices(vertices, Quad.Lft, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;
        }
        if ((data & RGT) == RGT)
        {
            //Debug.Log ("Right");
            vertices = fillVertices(vertices, Quad.Rgt, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & BTM) == BTM)
        {
            //Debug.Log ("Bottom");
            vertices = fillVertices(vertices, Quad.Btm, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & TOP) == TOP)
        {
            //Debug.Log ("Top");
            vertices = fillVertices(vertices, Quad.Top, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & BCK) == BCK)
        {
            //Debug.Log ("Back");
            vertices = fillVertices(vertices, Quad.Bck, vCount);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, vCount);
            vCount += 4;

        }
        if ((data & FWD) == FWD)
        {
            //Debug.Log ("Front");
            vertices = fillVertices(vertices, Quad.Fwd, vCount);
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
            vertices = fillVertices(vertices, Quad.Lft, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);
        }
        else if ((data & RGT) == RGT)
        {
            vertices = fillVertices(vertices, Quad.Rgt, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);

        }
        else if ((data & BTM) == BTM)
        {
            vertices = fillVertices(vertices, Quad.Btm, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);

        }
        else if ((data & TOP) == TOP)
        {
            vertices = fillVertices(vertices, Quad.Top, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);

        }
        else if ((data & BCK) == BCK)
        {
            vertices = fillVertices(vertices, Quad.Bck, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);
        }
        else
        {
            vertices = fillVertices(vertices, Quad.Fwd, 0);
            uvs = fillUVS(uvs, Block.Blocks[block].uv, 0);
        }
        vertices = addPos(vertices, index);
        triangles = coordinateTriangles(triangles);
        return new PrimVUT(vertices, uvs, triangles);
    }
    */
    static Vector3[] fillVertices(Vector3[] what, Vector3[] with, byte start)
    {
        for (byte i = 0; i < with.Length; i++)
        {
            what[i + start] = with[i];
        }
        return what;
    }
    static Vector3[] fillVertices(Vector3[] what, Vector3[] with, byte start, Vector3 offset)
    {
        for (byte i = 0; i < with.Length; i++)
        {
            what[i + start] = with[i] + offset;
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
    public static Vector3[] addPos(Vector3[] what, int x, int y, int z)
    {
        Vector3 addition = new Vector3(x, y, z);
        for (int i = 0; i < what.Length; i++)
        {
            what[i] += addition;
        }
        return what;
    }
    public static PrimVUT addPos(PrimVUT vut, int x, int y, int z)
    {
        Vector3 addition = new Vector3(x, y, z);
        for (int i = 0; i < vut.vertices.Length; i++)
        {
            vut.vertices[i] += addition;
        }
        return vut;
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

    public static PrimVUT Draw(Chunk chunk, int blockX, int blockY, int blockZ, byte block)
    {
        int vLength = 0;
        byte vCount = 0;
        byte data = GetBlockSides(chunk, blockX, blockY, blockZ, out vLength);

        Vector3[] vertices = new Vector3[vLength];
        Vector2[] uvs = new Vector2[vLength];
        int[] triangles = new int[(vLength * 3) >> 1]; //Amount of Triangles is always 3/2 of the Amount of Vertices
        Vector3 offset = new Vector3(blockX, blockY, blockZ);

        if ((data & LFT) == LFT)
        {
            vertices = fillVertices(vertices, Quad.Lft, vCount, offset);
            uvs = fillUVS(uvs, Block.Blocks[block].UV, vCount);
            vCount += 4;
        }
        if ((data & RGT) == RGT)
        {
            vertices = fillVertices(vertices, Quad.Rgt, vCount, offset);
            uvs = fillUVS(uvs, Block.Blocks[block].UV, vCount);
            vCount += 4;
        }
        if ((data & BTM) == BTM)
        {
            vertices = fillVertices(vertices, Quad.Btm, vCount, offset);
            uvs = fillUVS(uvs, Block.Blocks[block].UV, vCount);
            vCount += 4;
        }
        if ((data & TOP) == TOP)
        {
            vertices = fillVertices(vertices, Quad.Top, vCount, offset);
            uvs = fillUVS(uvs, Block.Blocks[block].UV, vCount);
            vCount += 4;
        }
        if ((data & BCK) == BCK)
        {
            vertices = fillVertices(vertices, Quad.Bck, vCount, offset);
            uvs = fillUVS(uvs, Block.Blocks[block].UV, vCount);
            vCount += 4;
        }
        if ((data & FWD) == FWD)
        {
            vertices = fillVertices(vertices, Quad.Fwd, vCount, offset);
            uvs = fillUVS(uvs, Block.Blocks[block].UV, vCount);
            vCount += 4;
        }
        triangles = coordinateTriangles(triangles);
        return new PrimVUT(vertices, uvs, triangles);
    }
    // Block sides is constructed in form a byte
    private static byte GetBlockSides(Chunk chunk, int blockX, int blockY, int blockZ, out int length)
    {
        byte[,,] blocks = chunk.Blocks;
        byte data = 0;
        length = 0;
        #region LEFT
        if (blockX - 1 < 0)
        {
            if (chunk.LftChunk != null && chunk.LftChunk.Blocks[LastBlockIndexXZ, blockY, blockZ] == 0)
            {
                data |= 1;
                length += 4;
            }
        }
        else if (blocks[blockX - 1, blockY, blockZ] == 0)
        {
            data |= 1;
            length += 4;
        }
        #endregion
        #region RIGHT
        if (blockX + 1 > LastBlockIndexXZ)
        {
            if (chunk.RgtChunk != null && chunk.RgtChunk.Blocks[0, blockY, blockZ] == 0)
            {
                data |= 2;
                length += 4;
            }
        }
        else if (blocks[blockX + 1, blockY, blockZ] == 0)
        {
            data |= 2;
            length += 4;
        }
        #endregion
        #region BOTTOM
        if (blockY - 1 < 0)
        {
            if (chunk.BtmChunk != null && chunk.BtmChunk.Blocks[blockX, LastBlockIndexY, blockZ] == 0)
            {
                data |= 4;
                length += 4;
            }
        }
        else if (blocks[blockX, blockY - 1, blockZ] == 0)
        {
            data |= 4;
            length += 4;
        }
        #endregion
        #region TOP
        if (blockY + 1 > LastBlockIndexY)
        {
            if (chunk.TopChunk != null && chunk.TopChunk.Blocks[blockX, 0, blockZ] == 0)
            {
                data |= 8;
                length += 4;
            }
        }
        else if (blocks[blockX, blockY + 1, blockZ] == 0)
        {
            data |= 8;
            length += 4;
        }
        #endregion
        #region BACK
        if (blockZ - 1 < 0)
        {
            if (chunk.BckChunk != null && chunk.BckChunk.Blocks[blockX, blockY, LastBlockIndexXZ] == 0)
            {
                data |= 16;
                length += 4;
            }
        }
        else if (blocks[blockX, blockY, blockZ - 1] == 0)
        {
            data |= 16;
            length += 4;
        }
        #endregion
        #region FORWARD
        if (blockZ + 1 > LastBlockIndexXZ)
        {
            if (chunk.FwdChunk != null && chunk.FwdChunk.Blocks[blockX, blockY, 0] == 0)
            {
                data |= 32;
                length += 4;
            }
        }
        else if (blocks[blockX, blockY, blockZ + 1] == 0)
        {
            data |= 32;
            length += 4;
        }
        #endregion
        return data;
    }
}