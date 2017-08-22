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
    //private static Work[] alloc = new Work[128];
    //private static int allocCounter = 0;

    //Processing Queues
    private static Queue<Work> pendingWork = new Queue<Work>(); // For the side thread
    private static Queue<Chunk> finishedWork = new Queue<Chunk>(); // For the main, Unity thread

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
                if (pendingWork.Count > 0)
                {
                    //threadIsRunning = true;
                    ThreadPool.QueueUserWorkItem(Run);
                }
                while (finishedWork.Count > 0)
                {
                    chunkTemp = finishedWork.Dequeue();
                    chunkTemp.SetMesh(chunkTemp.MainVUT);
                }
            }

            yield return null;
        }
    }
    // For the initial draw
    public static void DrawWorld(World world)
    {
        //Work work = alloc[allocCounter];

        // Iterate through chunks
        for (int chunkX = 0; chunkX < world.Width; chunkX++)
        {
            for (int chunkY = 0; chunkY < world.Height; chunkY++)
            {
                for (int chunkZ = 0; chunkZ < world.Width; chunkZ++)
                {
                    pendingWork.Enqueue(new Work(world.chunks[chunkX, chunkY, chunkZ], Index.Zero, null));
                }
            }
        }
    }
    public static IEnumerator DrawChunks(World world, int xMin, int xMax, int zMin, int zMax)
    {
        //Work work = alloc[allocCounter];
        // Iterate through chunks
        for (int chunkX = xMin; chunkX <= xMax; chunkX++)
        {
            for (int chunkY = 0; chunkY < world.Height; chunkY++)
            {
                for (int chunkZ = zMin; chunkZ <= zMax; chunkZ++)
                {
                    pendingWork.Enqueue(new Work(world.chunks[chunkX, chunkY, chunkZ], Index.Zero, null));
                    yield return null;
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

    /*
     * Threading Jobs 
     */


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