using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

/*
 *  How this works:
 *  The main thread will receive coordinates of which block to remove.
 *  The block removal process begins with changed the block to 0 (represents air);
 *  then the mesh construction of this new dataset is passed of to a separate thread, genThread.
 *  genThread will construct a new mesh (represented as type VUT) and pass it back
 *  to the main thread, so that the main thread may assign the new mesh variable.
 *  
 *  Work is added to Queues.
 *  Work for mesh construction is given as type, Deal;
 *  Work for mesh assignment is given as type, Chunk because it only needs to know which chunk to assign the mesh to.
 * 
 */
public static class ChunkMeshGen {

    //PreAllocated Set Deals
    private static Deal[] alloc;
    private static int allocCounter;

    //Processing Queues
    private static Queue<Deal> deals;
    private static Queue<Chunk> completedDeals;

    //Processing Thread
    private static Thread genThread;
    private static object lockScope;

    //This allows genThread to be active only while the main thread is active
    public static bool AppAlive = true;

    public static IEnumerator Initialize()
    {
        // Initialize variables
        lockScope = new object();
        alloc = new Deal[50];
        for(int i = 0; i < alloc.Length; i++)
        {
            alloc[i] = new Deal(new Index2(0, 0, 0, 0, 0, 0), Airify, Shape.ONE);
        }
        allocCounter = 0;
        deals = new Queue<Deal>();
        completedDeals = new Queue<Chunk>();

        // Start thread
        genThread = new Thread(Run);
        genThread.Name = "Chunk Mesh Generator";
        genThread.Start();

        // Wait for work to do
        while (true)
        {
            lock (lockScope)
            {
                // Error logging (make sure genThread is still running)
                // For some reason, genThread was sometimes failing.
                if (deals.Count > 0)
                {
                    Debug.Log("FROM MAIN THREAD: " + deals.Count);
                    Debug.Log("STATE OF PROCESSING THREAD: " + genThread.ThreadState);
                }
                // Do mesh assignment
                if (completedDeals.Count > 0)
                {
                    SpitMesh(completedDeals.Dequeue());
                }
            }
            yield return null;
        }
    }
    // For the initial draw
    public static void Draw(Index index)
    {
        Chunk chunk = World.chunks[index.i, index.j, index.k];
        byte[,,] blocks = chunk.blocks;
        PrimVUT[,,] vuts = chunk.vuts;

        for(int i = 0; i < Chunk.WIDTH; i++)
        {
            for(int j = 0; j < Chunk.HEIGHT; j++)
            {
                for(int k = 0; k < Chunk.WIDTH; k++)
                {
                    if (blocks[i, j, k] != 0)
                    {
                        PrimVUT block = BlockMeshGen.Draw(GetBlockSides(chunk.blocks, index, i, j, k), 1, i, j, k);
                        vuts[i, j, k] = block;
                    }
                }
            }
        }
        SpitMesh(chunk);
    }
    // Remove a block
    public static void RemoveOne(Index chunkIndex, Index blockIndex)
    {
        lock (lockScope)
        {
            deals.Enqueue(MakeDeal(chunkIndex, blockIndex, Shape.ONE));
        }

    }
    /*
    private static void Remove(Index chunkIndex, Index blockIndex)
    {
        Chunk chunk = World.chunks[chunkIndex.i, chunkIndex.j, chunkIndex.k];
        PrimVUT vut = chunk.vuts[blockIndex.i, blockIndex.j, blockIndex.k];
        byte[,,] blocks = chunk.blocks;

        //Dump VUT(GC UP, MEM DOWN)
        vut.vertices = null;
        vut.uvs = null;
        vut.triangles = null;

        //Update Byte Model
        blocks[blockIndex.i, blockIndex.j, blockIndex.k] = 0;
    }
    */
    // Deal will have coordinates and the shape to remove
    private static Deal MakeDeal(Index chunkIndex, Index blockIndex, IShape shape)
    {
        Deal deal = alloc[allocCounter];
        deal.chunkIndex = chunkIndex;
        deal.blockIndex = blockIndex;
        deal.shape = shape;

        allocCounter = (allocCounter + 1) % alloc.Length;

        return deal;
    }
    // AirifyOne*
    private static void Airify(Index chunkIndex, Index blockIndex)
    {

        Chunk chunk = World.chunks[chunkIndex.i, chunkIndex.j, chunkIndex.k];
        //PrimVUT vut = chunk.vuts[blockIndex.i, blockIndex.j, blockIndex.k];

        //Dump VUT(GC UP, MEM DOWN)
        //vut.vertices = null;
        //vut.uvs = null;
        //vut.triangles = null;


        //Update Surrounding Blocks' VUT
        //LFT
        blockIndex.i--;
        UpdateBlock(chunkIndex, blockIndex);
        //RGT
        blockIndex.i += 2;
        UpdateBlock(chunkIndex, blockIndex);
        //BTM
        blockIndex.i--;
        blockIndex.j--;
        UpdateBlock(chunkIndex, blockIndex);
        //TOP
        blockIndex.j += 2;
        UpdateBlock(chunkIndex, blockIndex);
        //BCK
        blockIndex.j--;
        blockIndex.k--;
        UpdateBlock(chunkIndex, blockIndex);
        blockIndex.k += 2;
        UpdateBlock(chunkIndex, blockIndex);


        //Finish
        completedDeals.Enqueue(chunk);
        //SpitMesh(chunk);
    }
    // Update vut data of block and surrounding blocks
    private static void UpdateBlock(Index chunkIndex, Index blockIndex)
    {
        Chunk[,,] chunks = World.chunks;
        int i = blockIndex.i;
        int j = blockIndex.j;
        int k = blockIndex.k;
        bool oob = false;
        //Out of bounds checking; if so, go to next chunk over
        if(i < 0)
        {
            chunkIndex.i--;
            i = Chunk.WIDTH - 1;
            oob = true;
        } else if(i >= Chunk.WIDTH)
        {
            chunkIndex.i++;
            i = 0;
            oob = true;
        }
        else if(j < 0)
        {
            chunkIndex.j--;
            j = Chunk.WIDTH - 1;
            oob = true;
        }
        else if(j >= Chunk.HEIGHT)
        {
            chunkIndex.j++;
            j = 0;
            oob = true;
        }
        else if (k < 0)
        {
            chunkIndex.k--;
            k = Chunk.WIDTH - 1;
            oob = true;
        }
        else if (k >= Chunk.WIDTH)
        {
            chunkIndex.k++;
            k = 0;
            oob = true;
        }

        try
        {
            Chunk chunk = chunks[chunkIndex.i, chunkIndex.j, chunkIndex.k];
            byte[,,] blocks = chunk.blocks;
            if (blocks[i, j, k] != 0)
            {
                PrimVUT block = BlockMeshGen.Draw(GetBlockSides(blocks, chunkIndex, i, j, k), 1, i, j, k);
                chunk.vuts[i, j, k] = block;
            }
            if (oob)
            {
                lock (lockScope)
                {
                    completedDeals.Enqueue(chunk);
                }
            }
        }
        catch (System.IndexOutOfRangeException e)
        {
            Debug.Log("Tried editing block out of range. Is user not in center of world? " + e);
        }


    }
    // ! THIS IS THE HEAVY WORK THAT THIS DESIGN HAS TO WORK WITH !
    private static void SpitMesh(Chunk chunk)
    {
        //System.DateTime time = System.DateTime.Now;
        PrimVUT[,,] vuts = chunk.vuts;
        byte[,,] blocks = chunk.blocks;
        VUT totalVUT = new VUT(new List<Vector3>(), new List<Vector2>(), new List<int>());
        for (int i = 0; i < Chunk.WIDTH; i++)
        {
            for (int j = 0; j < Chunk.HEIGHT; j++)
            {
                for (int k = 0; k < Chunk.WIDTH; k++)
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
        totalVUT.ToMesh(chunk.mesh);
        chunk.mCollider.sharedMesh = chunk.mesh;
        //Debug.Log(System.DateTime.Now - time);
    }
    // Block sides is constructed in form a byte
    private static byte GetBlockSides(byte[,,] myBlocks, Index index, int i, int j, int k)
    {
        byte data = 0;
        #region LEFT
        if(i - 1 < 0)
        {
            if(index.i - 1 > -1)
            {
                if(World.chunks[index.i - 1, index.j, index.k].blocks[15, j, k] == 0)
                {
                    data |= 1;
                }
            }
        }
        else
        {
            if(myBlocks[i - 1, j, k] == 0)
            {
                data |= 1;
            }
        }
        #endregion
        #region RIGHT
        if(i + 1 >= Chunk.WIDTH)
        {
            if(index.i + 1 < World.WIDTH)
            {
                if (World.chunks[index.i + 1, index.j, index.k].blocks[0, j, k] == 0)
                {
                    data |= 2;
                }
            }
        }
        else
        {
            if (myBlocks[i + 1, j, k] == 0)
            {
                data |= 2;
            }
        }
        #endregion
        #region BOTTOM
        if (j - 1 < 0)
        {
            if (index.j - 1 > -1)
            {
                if (World.chunks[index.i, index.j - 1, index.k].blocks[i, 15, k] == 0)
                {
                    data |= 4;
                }
            }
        }
        else
        {
            if (myBlocks[i, j - 1, k] == 0)
            {
                data |= 4;
            }
        }
        #endregion
        #region TOP
        if (j + 1 >= Chunk.HEIGHT)
        {
            if (index.j + 1 < World.HEIGHT)
            {
                if (World.chunks[index.i, index.j + 1, index.k].blocks[i, 0, k] == 0)
                {
                    data |= 8;
                }
            }
        }
        else
        {
            if (myBlocks[i, j + 1, k] == 0)
            {
                data |= 8;
            }
        }
        #endregion
        #region BACK
        if (k - 1 < 0)
        {
            if (index.k - 1 > -1)
            {
                if (World.chunks[index.i, index.j, index.k - 1].blocks[i, j, 15] == 0)
                {
                    data |= 16;
                }
            }
        }
        else
        {
            if (myBlocks[i, j, k - 1] == 0)
            {
                data |= 16;
            }
        }
        #endregion
        #region FORWARD
        if (k + 1 >= Chunk.WIDTH)
        {
            if (index.k + 1 < World.WIDTH)
            {
                if (World.chunks[index.i, index.j, index.k + 1].blocks[i, j, 0] == 0)
                {
                    data |= 32;
                }
            }
        }
        else
        {
            if (myBlocks[i, j, k + 1] == 0)
            {
                data |= 32;
            }
        }
        #endregion
        return data;

    }
    private static void Run()
    {
        // Wait for work to do, stop if main thread is stopped
        while (AppAlive)
        {
            // Mesh construction (heavy work)
            if(deals.Count > 0)
            {
                lock (lockScope)
                {
                    deals.Dequeue().Start();

                } 
            }
        }
    }
    
}
