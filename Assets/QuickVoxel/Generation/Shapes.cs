using System.Collections.Generic;
using UnityEngine;

public delegate void Action(Chunk chunk, Index blockIndex, byte byteData);
public delegate void SideAction(Chunk chunk, Index blockIndex);

public struct Process {

    public Index blockIndex;
    public Action action;
    public byte byteData;

    public Process(Index blockIndex, Action action, byte byteData = 0)
    {
        this.blockIndex = blockIndex;
        this.action = action;
        this.byteData = byteData;
    }
}

public struct SideProcess
{
    public Index blockIndex;
    public SideAction action;

    public SideProcess(Index blockIndex, SideAction action)
    {
        this.blockIndex = blockIndex;
        this.action = action;
    }
}

public interface IShape
{
    void Draw(Chunk centerChunk, Index centerOfShape);
    void Optimize();
    void Optimize(SideAction action);
}

public class Shape : IShape
{
    private List<Process> mainProcesses;
    private List<SideProcess> sideProcesses;
    private static List<Chunk> chunksToBeUpdated = new List<Chunk>();

    public Shape()
    {
        mainProcesses = new List<Process>();
        sideProcesses = new List<SideProcess>();
    }
    // Singular
    public Shape(Action action, byte block = 0)
    {
        mainProcesses = new List<Process>();
        sideProcesses = new List<SideProcess>();

        mainProcesses.Add(new Process(new Index(0, 0, 0), action, block));
    }
    public Shape(List<Process> mainProcesses)
    {
        this.mainProcesses = mainProcesses;
        this.sideProcesses = new List<SideProcess>();
    }
    public Shape(List<Process> mainProcesses, List<SideProcess> sideProcesses)
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
                AddIfNotInList(targetChunk, chunksToBeUpdated);
                main.action(targetChunk, blockIndex, main.byteData);
            }
        }
        foreach (SideProcess side in sideProcesses)
        {
            Index relativeIndex = side.blockIndex;
            Chunk targetChunk;
            Index blockIndex = new Index(
                centerBlockIndex.i + relativeIndex.i, centerBlockIndex.j + relativeIndex.j, centerBlockIndex.k + relativeIndex.k
            );
            NormalizeLocation(out targetChunk, ref blockIndex, centerChunk);
            if (targetChunk != null)
            {
                AddIfNotInList(targetChunk, chunksToBeUpdated);
                side.action(targetChunk, blockIndex);
            }
        }
        foreach(Chunk chunkToBeUpdated in chunksToBeUpdated)
        {
            ChunkMeshGenerator.SpitMesh(chunkToBeUpdated);
        }
        chunksToBeUpdated.Clear();
    }
    // Optimize the side processes with default action of updates
    public void Optimize()
    {
        Optimize(ChunkMeshGenerator.UpdateBlock);
    }
    // Optimize the side processes for the least steps possible
    public void Optimize(SideAction action)
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
            sideProcesses.Add(new SideProcess(index, action));
        }
    }

    private static void NormalizeLocation(out Chunk chunk, ref Index blockIndex, Chunk centerChunk)
    {
        chunk = centerChunk;
        if (blockIndex.i < 0)
        {
            chunk = centerChunk.LftChunk;
            blockIndex.i = (blockIndex.i % Chunk.Width) + Chunk.Width;
        }
        else if (blockIndex.i >= Chunk.Width)
        {
            chunk = centerChunk.RgtChunk;
            blockIndex.i %= Chunk.Width;
        }
        if (blockIndex.j < 0)
        {
            chunk = centerChunk.BtmChunk;
            blockIndex.j = (blockIndex.j % Chunk.Height) + Chunk.Height;

        }
        else if (blockIndex.j >= Chunk.Height)
        {
            chunk = centerChunk.TopChunk;
            blockIndex.j %= Chunk.Height;
        }
        if (blockIndex.k < 0)
        {
            chunk = centerChunk.BckChunk;
            blockIndex.k = (blockIndex.k % Chunk.Width) + Chunk.Width;
        }
        else if (blockIndex.k >= Chunk.Width)
        {
            chunk = centerChunk.FwdChunk;
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


