using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * TODO
 */ 
public delegate void Process(Index chunkIndex, Index blockIndex);

public interface IShape
{
    void Draw(Index chunkIndex, Index blockIndex, Process custom);
}
public static class Shape
{
    public static readonly One ONE = new One();
}
public class One : IShape
{
    public One()
    {

    }
    public void Draw(Index chunkIndex, Index blockIndex, Process custom)
    {
        custom(chunkIndex, blockIndex);
    }
}
public class Cubic : IShape
{
    public int x, y, z;
    public float[] delays;

    public Cubic(int x, int y, int z, float[] delays)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.delays = delays;
    }
    public void Draw(Index chunkIndex, Index blockIndex, Process custom)
    {
        Index newChunkIndex = new Index(chunkIndex);
        Index newBlockIndex = new Index(blockIndex);
        for (int i = blockIndex.i; i < blockIndex.i + x; i++)
        {
            for (int j = blockIndex.j; j < blockIndex.j + y; j++)
            {
                for (int k = blockIndex.k; k < blockIndex.k + z; k++)
                {
                    custom(newChunkIndex, newBlockIndex);
                    newChunkIndex.k = chunkIndex.k + (k / Chunk.WIDTH);
                    newBlockIndex.k = k % Chunk.WIDTH;
                }
                newChunkIndex.j = chunkIndex.j + (j / Chunk.HEIGHT);
                newBlockIndex.j = j % Chunk.HEIGHT;
            }
            newChunkIndex.i = chunkIndex.i + (i / Chunk.WIDTH);
            newBlockIndex.i = i % Chunk.WIDTH;
        }
    }
    public override string ToString()
    {
        return string.Format("Cubic, Dimensions: ({0}, {1}, {2})", x, y, z);
    }
}

public class FlatCircle : IShape
{
    public int radius;
    public float[] delays;

    public FlatCircle(int radius, float[] delays)
    {
        this.radius = radius;
        this.delays = delays;
    }
    public void Draw(Index chunkIndex, Index blockIndex, Process custom)
    {
        //Trig
    }
}
public class LayeredFlatCircles : IShape
{
    public int[] radius;
    public float[] delays;

    public LayeredFlatCircles(int[] radius, float[] delays)
    {
        this.radius = radius;
        this.delays = delays;
    }
    public void Draw(Index chunkIndex, Index blockIndex, Process custom)
    {

    }
}
public class FlatDisc : IShape
{
    public int radius;
    public float[] delays;

    public FlatDisc(int radius, float[] delays)
    {
        this.radius = radius;
        this.delays = delays;
    }
    public void Draw(Index chunkIndex, Index blockIndex, Process custom)
    {
        Index newChunkIndex = new Index(chunkIndex);
        Index newBlockIndex = new Index(blockIndex);
        for (int i = 0; i < radius * 2; i++)
        {
            for (int k = 0; k < radius * 2; k++)
            {
                int x = i - radius;
                int z = k - radius;
                if (x * x + z * z <= radius * radius)
                {
                    newChunkIndex.i = chunkIndex.i + ((blockIndex.i + x) / Chunk.WIDTH);
                    newChunkIndex.k = chunkIndex.k + ((blockIndex.k + z) / Chunk.WIDTH);
                    newBlockIndex.i = (blockIndex.i + x) % Chunk.WIDTH;
                    newBlockIndex.k = (blockIndex.k + z) % Chunk.WIDTH;
                    custom(newChunkIndex, newBlockIndex);

                }
            }
        }
    }
    public override string ToString()
    {
        return string.Format("Disc, Radius: {0}", radius);
    }
}
public class LayeredFlatDiscs : IShape
{
    public int[] radius;
    public float[] delays;

    public LayeredFlatDiscs(int[] radius, float[] delays)
    {
        this.radius = radius;
        this.delays = delays;
    }

    public void Draw(Index chunkIndex, Index blockIndex, Process custom)
    {

    }
}
/*
public struct TallCircle
{
    public int radius;
    public float[] delays;

    public TallCircle(int radius, float[] delays)
    {
        this.radius = radius;
        this.delays = delays;
    }
    public void Draw(Index chunkIndex, Index blockIndex, Process custom)
    {
        Index newChunkIndex = new Index(chunkIndex);
        Index newBlockIndex = new Index(blockIndex);
        int centerX = (blockIndex.i + radius * 2) / 2;
        int centerY = (blockIndex.j + radius * 2) / 2;
        for (int i = blockIndex.i; i < blockIndex.i + radius * 2; i++)
        {
            if ()
                custom(newChunkIndex, newBlockIndex);
            newChunkIndex.i = chunkIndex.i + (i / Chunk.WIDTH);
            newBlockIndex.i = i % Chunk.WIDTH;
        }
    }
}
public struct LayeredTallCircles
{
    public int[] radius;
    public float[] delays;

    public LayeredTallCircles(int[] radius, float[] delays)
    {
        this.radius = radius;
        this.delays = delays;
    }
}*/


