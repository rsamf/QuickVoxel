using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UE = UnityEngine;
using System.Threading;
using System.Diagnostics;
using System;

/*
 *  How this works:
 *  The main thread will receive coordinates of which block to remove.
 *  The block removal process begins with changing the byte data of the specified index to 0, which represents air.
 *  Then, the mesh construction of this new dataset is passed of to a separate thread, genThread.
 *  genThread will construct a new mesh (represented as type VUT) and pass it back
 *  to the main thread, so that the main thread may assign the new mesh.
 *  
 *  Note about Work:
 *  Work for mesh construction is given as type of struct, Work
 *  Work is added to the processing queue, pendingWork.
 *  Finished Work is added to the processing queue, finishedWork, so that the main thread may assign the mesh given in the Chunk's VUT property.
 */
public static class ChunkMeshGenerator {

    //PreAllocated Set Deals
    //private static Work[] alloc;
    //private static int allocCounter;

    //Processing Queues
    private static Queue<Work> pendingWork = new Queue<Work>(); // For the side thread
    private static Queue<Chunk> finishedWork = new Queue<Chunk>(); // For the main, Unity thread
    private static bool slowButSmooth = false;
    public static bool SlowButSmooth {
        get { return slowButSmooth; } set { slowButSmooth = value; }
    }
    /*
    private static Queue<Work> pendingWork 
    {
        get
        {
            lock (lockScope)
            {
                return __pendingWork;
            }
        }
        set
        {
            lock (lockScope)
            {
                __pendingWork = value;
            }
        }
    }
    private static Queue<Chunk> finishedWork
    {
        get
        {
            lock (lockScope)
            {
                return __finishedWork;
            }
        }
        set
        {
            lock (lockScope)
            {
                __finishedWork = value;
            }
        }
    }*/

    //Processing Thread
    //private static Thread genThread;
    private static object lockScope = new object();
    private static bool isInitialized = false;
    public static bool IsInitialized { get { return isInitialized; } }


    public static IEnumerator Initialize()
    {
        isInitialized = true;
        Chunk chunkTemp;
        // Wait for work to do
        while (true)
        {
            // Removing this lock causes error
            lock (lockScope)
            {
                if (pendingWork.Count > 0 /*&& !threadIsRunning*/)
                {
                    //threadIsRunning = true;
                    ThreadPool.QueueUserWorkItem(Run);
                }
                while (finishedWork.Count > 0)
                {
                    chunkTemp = finishedWork.Dequeue();
                    chunkTemp.SetMesh(chunkTemp.MainVUT);
                    if(SlowButSmooth) yield return null;
                }
            }

            yield return null;
        }
    }
    // For the initial draw
    public static void DrawWorld(World world)
    {
        DrawChunks(world, 0, world.Width - 1, 0, world.Width - 1);
    }
    public static void DrawChunks(World world, int xMin, int xMax, int zMin, int zMax)
    {
        // Iterate through chunks
        for (int chunkX = xMin; chunkX <= xMax; chunkX++)
        {
            for (int chunkY = 0; chunkY < world.Height; chunkY++)
            {
                for (int chunkZ = zMin; chunkZ <= zMax; chunkZ++)
                {
                    //Debug.Log("enqueuing work");
                    EnqueueWork(new Work(world.chunks[chunkX, chunkY, chunkZ], Index.Zero, null));
                }
            }
        }
    }
    public static void DrawChunk(Chunk chunk)
    {
        byte[,,] blocks = chunk.Blocks;
        PrimVUT[,,] vuts = chunk.Vuts;
        byte targetBlock;
        // Iterate through blocks
        for (int blockX = 0; blockX < Chunk.Width; blockX++)
        {
            for (int blockY = 0; blockY < Chunk.Height; blockY++)
            {
                for (int blockZ = 0; blockZ < Chunk.Width; blockZ++)
                {
                    targetBlock = blocks[blockX, blockY, blockZ];
                    if (targetBlock != 0)
                    {
                        vuts[blockX, blockY, blockZ] = BlockMeshGenerator.Draw(chunk, blockX, blockY, blockZ, targetBlock);
                    }
                }
            }
        }
        // Chunk is done, spit out mesh
        SpitMesh(chunk);
    }

    public static void AddBlock(Chunk chunk, Index blockIndex, byte block = 1)
    {
        chunk.Blocks[blockIndex.i, blockIndex.j, blockIndex.k] = block;
        UpdateBlock(chunk, blockIndex);
    }
    // Alias for AddBlock
    public static void EditBlock(Chunk chunk, Index blockIndex, byte block = 1)
    {
        AddBlock(chunk, blockIndex, block);
    }
    public static void RemoveBlock(Chunk chunk, Index blockIndex, byte block = 0)
    {
        chunk.Blocks[blockIndex.i, blockIndex.j, blockIndex.k] = block;
        UpdateBlock(chunk, blockIndex);
    }
    // Update vut data of block and surrounding blocks
    public static void UpdateBlock(Chunk chunk, Index blockIndex)
    {
        byte block = chunk.Blocks[blockIndex.i, blockIndex.j, blockIndex.k];
        if(block != 0)
        {
            chunk.Vuts[blockIndex.i, blockIndex.j, blockIndex.k] = BlockMeshGenerator.Draw(chunk, blockIndex.i, blockIndex.j, blockIndex.k, block);
        }
    }

    // ! THIS IS THE HEAVY WORK THAT THIS DESIGN HAS TO WORK WITH !
    public static void SpitMesh(Chunk chunk)
    {
        PrimVUT[,,] vuts = chunk.Vuts;
        byte[,,] blocks = chunk.Blocks;
        VUT totalVUT = chunk.MainVUT;
        totalVUT.Clear();
        for (int i = 0; i < Chunk.Width; i++)
        {
            for (int j = 0; j < Chunk.Height; j++)
            {
                for (int k = 0; k < Chunk.Width; k++)
                {
                    if (blocks[i, j, k] != 0)
                    {
                        totalVUT.vertices.AddRange(vuts[i, j, k].vertices);
                        totalVUT.uvs.AddRange(vuts[i, j, k].uvs);
                        foreach (int tri in vuts[i, j, k].triangles)
                        {
                            totalVUT.triangles.Add(tri + (totalVUT.vertices.Count - vuts[i, j, k].vertices.Length));
                        }
                    }
                }
            }
        }
        finishedWork.Enqueue(chunk);
    }

    // Block sides is constructed in form a byte
    private static byte GetBlockSides(Chunk chunk, int blockX, int blockY, int blockZ)
    {
        int lastBlockIndexXZ = Chunk.Width - 1;
        int lastBlockIndexY = Chunk.Height - 1;
        byte[,,] blocks = chunk.Blocks;
        byte data = 0;
        #region LEFT
        if (blockX - 1 < 0)
        {
            if (chunk.LftChunk != null && chunk.LftChunk.Blocks[lastBlockIndexXZ, blockY, blockZ] == 0)
            {
                data |= 1;
            }
        } else if (blocks[blockX - 1, blockY, blockZ] == 0)
        {
            data |= 1;
        }
        #endregion
        #region RIGHT
        if(blockX + 1 > lastBlockIndexXZ)
        {
            if(chunk.RgtChunk != null && chunk.RgtChunk.Blocks[0, blockY, blockZ] == 0)
            {
                data |= 2;
            }
        } else if(blocks[blockX + 1, blockY, blockZ] == 0)
        {
            data |= 2;
        }
        #endregion
        #region BOTTOM
        if (blockY - 1 < 0)
        {
            if (chunk.BtmChunk != null && chunk.BtmChunk.Blocks[blockX, lastBlockIndexY, blockZ] == 0)
            {
                data |= 4;
            }
        }
        else if (blocks[blockX, blockY - 1, blockZ] == 0)
        {
            data |= 4;
        }
        #endregion
        #region TOP
        if (blockY + 1 > lastBlockIndexY)
        {
            if (chunk.TopChunk != null && chunk.TopChunk.Blocks[blockX, 0, blockZ] == 0)
            {
                data |= 8;
            }
        }
        else if (blocks[blockX, blockY + 1, blockZ] == 0)
        {
            data |= 8;
        }
        #endregion
        #region BACK
        if (blockZ - 1 < 0)
        {
            if (chunk.BckChunk != null && chunk.BckChunk.Blocks[blockX, blockY, lastBlockIndexXZ] == 0)
            {
                data |= 16;
            }
        }
        else if (blocks[blockX, blockY, blockZ - 1] == 0)
        {
            data |= 16;
        }
        #endregion
        #region FORWARD
        if (blockZ + 1 > lastBlockIndexXZ)
        {
            if (chunk.FwdChunk != null && chunk.FwdChunk.Blocks[blockX, blockY, 0] == 0)
            {
                data |= 32;
            }
        }
        else if (blocks[blockX, blockY, blockZ + 1] == 0)
        {
            data |= 32;
        }
        #endregion
        return data;

    }


    /*
     * Threading Jobs 
     */
    public static void EnqueueWork(Work work)
    {
        // Access to thread
        pendingWork.Enqueue(work);
    }

    private static void Run(object threadContext)
    {
        lock (lockScope)
        {
            while(pendingWork.Count > 0)
            {
                Work w = pendingWork.Dequeue();
                if(w.Shape == null)
                {
                    DrawChunk(w.CenterChunk);
                } else
                {
                    w.Start();
                }
            }
        }
    }
}