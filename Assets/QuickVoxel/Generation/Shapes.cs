using System.Collections.Generic;
using UnityEngine;


public struct Process {

    public Index blockIndex;
    public byte byteData;

    public Process(Index blockIndex, byte byteData = 0)
    {
        this.blockIndex = blockIndex;
        this.byteData = byteData;
    }
}

public struct BlockLocation
{
    public Index blockIndex;
    public Chunk chunk;

    public BlockLocation(Chunk chunk, Index blockIndex)
    {
        this.chunk = chunk;
        this.blockIndex = blockIndex;
    }
}

public interface IShape
{
    void Draw(Chunk centerChunk, Index centerOfShape);
    void Optimize();
}

public class Shape : IShape
{
    private List<Process> mainProcesses;
    private List<Index> sideProcesses;
    private static List<Chunk> chunksToBeUpdated = new List<Chunk>();
    private static List<BlockLocation> blocksToBeUpdated = new List<BlockLocation>();


    public Shape(byte block = 0)
    {
        mainProcesses = new List<Process>();
        sideProcesses = new List<Index>();
        mainProcesses.Add(new Process(Index.Zero, block));
    }

    public Shape(List<Process> mainProcesses)
    {
        this.mainProcesses = mainProcesses;
        this.sideProcesses = new List<Index>();
    }
    public Shape(List<Process> mainProcesses, List<Index> sideProcesses)
    {
        this.mainProcesses = mainProcesses;
        this.sideProcesses = sideProcesses;
    }

    public void Draw(Chunk centerChunk, Index centerBlockIndex)
    {
        foreach (Process main in mainProcesses)
        {
            Index relativeIndex = main.blockIndex;
            Chunk targetChunk;
            Index blockIndex = new Index(
                centerBlockIndex.i + relativeIndex.i, centerBlockIndex.j + relativeIndex.j, centerBlockIndex.k + relativeIndex.k
            );
            NormalizeLocation(out targetChunk, ref blockIndex, centerChunk);
            if(targetChunk != null)
            {
                targetChunk.Blocks[blockIndex.i, blockIndex.j, blockIndex.k] = main.byteData;
                addToBeingUpdated(targetChunk, blockIndex);
            }
        }
        foreach (Index sideIndex in sideProcesses)
        {
            Index relativeIndex = sideIndex;
            Chunk targetChunk;
            Index blockIndex = new Index(
                centerBlockIndex.i + relativeIndex.i, centerBlockIndex.j + relativeIndex.j, centerBlockIndex.k + relativeIndex.k
            );
            NormalizeLocation(out targetChunk, ref blockIndex, centerChunk);
            if (targetChunk != null)
            {
                addToBeingUpdated(targetChunk, blockIndex);
            }
        }
        foreach(BlockLocation targetBlock in blocksToBeUpdated)
        {
            ChunkMeshGenerator.UpdateBlock(targetBlock.chunk, targetBlock.blockIndex);
        }
        foreach(Chunk targetChunk in chunksToBeUpdated)
        {
            ChunkMeshGenerator.SpitMesh(targetChunk);
            //Debug.Log("updated chunk " + targetChunk.Name);
        }
        chunksToBeUpdated.Clear();
        blocksToBeUpdated.Clear();
    }
    private void addToBeingUpdated(Chunk chunk, Index blockIndex)
    {
        //Debug.Log("Attempting to add chunk " + chunk.Name);
        AddIfNotInList(chunk, chunksToBeUpdated);
        blocksToBeUpdated.Add(new BlockLocation(chunk, blockIndex));
    }

    // Optimize the side processes for the least steps possible
    public void Optimize()
    {
        List<Index> sideBlockIndicees = new List<Index>();
        List<Index> mainBlockIndicees = new List<Index>();
        foreach (Process p in mainProcesses)
        {
            mainBlockIndicees.Add(p.blockIndex);
        }
        foreach (Process p in mainProcesses)
        {
            Index blockIndex = p.blockIndex;

            //LFT
            blockIndex.i--;
            AddIfNotInList(blockIndex, sideBlockIndicees, mainBlockIndicees);
            //RGT
            blockIndex.i += 2;
            AddIfNotInList(blockIndex, sideBlockIndicees, mainBlockIndicees);
            blockIndex.i--;
            //BTM
            blockIndex.j--;
            AddIfNotInList(blockIndex, sideBlockIndicees, mainBlockIndicees);
            //TOP
            blockIndex.j += 2;
            AddIfNotInList(blockIndex, sideBlockIndicees, mainBlockIndicees);
            blockIndex.j--;
            //BCK
            blockIndex.k--;
            AddIfNotInList(blockIndex, sideBlockIndicees, mainBlockIndicees);
            //FWD
            blockIndex.k += 2;
            AddIfNotInList(blockIndex, sideBlockIndicees, mainBlockIndicees);
        }
        sideProcesses.Clear();
        foreach (Index index in sideBlockIndicees)
        {
            sideProcesses.Add(index);
        }
    }

    private static void NormalizeLocation(out Chunk chunk, ref Index blockIndex, Chunk centerChunk)
    {
        chunk = centerChunk;
        /*
        bool isLeft = blockIndex.i < 0;
        bool isRight = blockIndex.i >= Chunk.Width;
        bool isBottom = blockIndex.j < 0;
        bool isTop = blockIndex.j >= Chunk.Height;
        bool isBack = blockIndex.k < 0;
        bool isForward = blockIndex.k >= Chunk.Width;
        */
        if (blockIndex.i < 0)
        {
            chunk = chunk.LftChunk;
            blockIndex.i = (blockIndex.i % Chunk.Width) + Chunk.Width;
        }
        else if (blockIndex.i >= Chunk.Width)
        {
            chunk = chunk.RgtChunk;
            blockIndex.i %= Chunk.Width;
        }
        if (blockIndex.j < 0)
        {
            chunk = chunk.BtmChunk;
            blockIndex.j = (blockIndex.j % Chunk.Height) + Chunk.Height;
        }
        else if (blockIndex.j >= Chunk.Height)
        {
            chunk = chunk.TopChunk;
            blockIndex.j %= Chunk.Height;
        }
        if (blockIndex.k < 0)
        {
            chunk = chunk.BckChunk;
            blockIndex.k = (blockIndex.k % Chunk.Width) + Chunk.Width;
        }
        else if (blockIndex.k >= Chunk.Width)
        {
            chunk = chunk.FwdChunk;
            blockIndex.k %= Chunk.Width;
        }

    }
    private static void AddIfNotInList<T>(T data, List<T> list, List<T> list2=null)
    {
        if(data != null)
        {
            // [ !A * (B + (!B * C)) ]
            if (!list.Contains(data) && (list2 == null || (list2 != null && !list2.Contains(data))))
            {
                list.Add(data);
                //Debug.Log("Successfully added " + data);
            }
        }
    }
}
/*
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
    public void Draw(Chunk chunk, Index blockIndex, Process custom)
    {
        Index newBlockIndex = new Index(blockIndex);
        for (int i = blockIndex.i; i < blockIndex.i + x; i++)
        {
            for (int j = blockIndex.j; j < blockIndex.j + y; j++)
            {
                for (int k = blockIndex.k; k < blockIndex.k + z; k++)
                {
                    custom(chunk, newBlockIndex);
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
*/
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


