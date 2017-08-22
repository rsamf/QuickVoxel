using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public class World : MonoBehaviour
{
    private bool isInitialized = false;
    public bool IsInitialized { get { return isInitialized; } }

    private bool manualTraversal = true;
    public bool ManualTraversal {  get { return manualTraversal; } }

    private int width;
    public int Width { get { return width; } }

    private int height;
    public int Height { get { return height; } }

    private DataGenerator.Config config;
    public DataGenerator.Config Config { get { return config; } }
    //Game
    public GameObject player;
    private Vector2 playersLastChunkPosition;
    public Vector3 Position {
        get {
            return transform.position;
        }
        set
        {
            transform.position = value;
        }
    }

    //Chunk Generation
    private GameObject chunkToSpawn;
    public Chunk[,,] chunks;


    void Update()
    {
        if (IsInitialized)
        {
            if(player == null)
            {
                if (!manualTraversal)
                {
                    manualTraversal = true;
                    UnityEngine.Debug.Log("Switching to manual traversal");
                }
                else
                {
                    if (Input.GetKeyUp(KeyCode.LeftArrow) && width > 0)
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        TraverseLeft();
                        UnityEngine.Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!! " + sw.ElapsedMilliseconds / 1000f);
                    }
                    else if (Input.GetKeyUp(KeyCode.RightArrow) && width > 0)
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        TraverseRight();
                        UnityEngine.Debug.Log(sw.ElapsedMilliseconds / 1000f);

                    }
                    else if (Input.GetKeyUp(KeyCode.DownArrow) && width > 0)
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        TraverseBack();
                        UnityEngine.Debug.Log(sw.ElapsedMilliseconds / 1000f);

                    }
                    else if (Input.GetKeyUp(KeyCode.UpArrow) && width > 0)
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        TraverseForward();
                        UnityEngine.Debug.Log(sw.ElapsedMilliseconds / 1000f);

                    }
                }
            } else
            {
                int x = (int)(player.transform.position.x / Chunk.Width);
                int z = (int)(player.transform.position.z / Chunk.Width);
                if (manualTraversal)
                {
                    manualTraversal = false;
                    playersLastChunkPosition.x = x;
                    playersLastChunkPosition.y = z;
                    UnityEngine.Debug.Log("Switching to automatic traversal");
                    UnityEngine.Debug.Log(x);
                    UnityEngine.Debug.Log(z);
                } else
                {
                    if (x < playersLastChunkPosition.x)
                    {
                        playersLastChunkPosition.x = x;
                        TraverseLeft();
                    }
                    else if (x > playersLastChunkPosition.x)
                    {
                        playersLastChunkPosition.x = x;
                        TraverseRight();
                    }
                    else if (z < playersLastChunkPosition.y)
                    {
                        playersLastChunkPosition.y = z;
                        TraverseBack();
                    }
                    else if (z > playersLastChunkPosition.y)
                    {
                        playersLastChunkPosition.y = z;
                        TraverseForward();
                    }
                }
            }

        }

    }

    public void Initialize(int width, int height, GameObject chunkObj, DataGenerator.Config config)
    {
        this.width = width;
        this.height = height;
        this.chunkToSpawn = chunkObj;
        this.config = config;
        this.playersLastChunkPosition = Vector3.zero;
        SetUp();
        isInitialized = true;
    }

    private void SetUp()
    {

        //Create Chunks
        UnityEngine.Debug.Log("Creating chunks");
        chunks = new Chunk[width, height, width];
        GameObject chunkObj;
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Width; k++)
                {
                    chunkObj = Instantiate(chunkToSpawn, transform) as GameObject;
                    chunks[i, j, k] = chunkObj.GetComponent<Chunk>();
                    chunkObj.transform.position = new Vector3(i * Chunk.Width, j * Chunk.Height, k * Chunk.Width);
                }
            }
        }
        // Set Names
        SetChunkNames();
        //Set Chunk Locality
        SetChunkLocality(0, width - 1, 0, width - 1);
        //Generate Block Data
        DataGenerator.GenWorld(this);
        //Generate Mesh Data
        StartCoroutine(ChunkMeshGenerator.Initialize());
        ChunkMeshGenerator.DrawWorld(this);
    }

    /*
     * Chunk Traversal 
     */
    public void TraverseLeft()
    {
        UnityEngine.Debug.Log("Started traversal");
        transform.position += Vector3.left * Chunk.Width;

        int lastIndex = width - 1;
        Chunk[,,] traversalChunks = GetChunks(lastIndex, lastIndex, 0, lastIndex);
        for(int i = lastIndex - 1; i >= 0; i--)
        {
            for(int j = 0; j < height; j++)
            {
                for(int k = 0; k < width; k++)
                {
                    chunks[i, j, k].transform.localPosition += Vector3.right * Chunk.Width;
                    chunks[i + 1, j, k] = chunks[i, j, k];
                }
            }
        }
        SetChunks(0, 0, 0, lastIndex, traversalChunks);
        for (int j = 0; j < height; j++)
        {
            for (int k = 0; k < width; k++)
            {
                chunks[0, j, k].transform.localPosition = new Vector3(0, j * Chunk.Height, k * Chunk.Width);
                chunks[0, j, k].GetComponent<MeshRenderer>().enabled = false;
            }
        }
        SetChunkNames();
        DataGenerator.GenChunks(this, 0, 0, 0, lastIndex);

        // Only once all chunks are set can I update the locality of each chunk
        // Need to do first 2 along x-axis
        SetChunkLocality(0, 1, 0, lastIndex);
        ChunkMeshGenerator.DrawChunks(this, 0, 1, 0, lastIndex);
        UnityEngine.Debug.Log("Finished");
    }
    public void TraverseRight()
    {
        transform.position += Vector3.right * Chunk.Width;

        int lastIndex = width - 1;
        Chunk[,,] traversalChunks = GetChunks(0, 0, 0, lastIndex);
        for (int i = 1; i <= lastIndex; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    chunks[i, j, k].transform.localPosition += Vector3.left * Chunk.Width;
                    chunks[i - 1, j, k] = chunks[i, j, k];
                }
            }
        }
        SetChunks(lastIndex, lastIndex, 0, lastIndex, traversalChunks);
        for (int j = 0; j < height; j++)
        {
            for (int k = 0; k < width; k++)
            {
                chunks[lastIndex, j, k].transform.localPosition = new Vector3(lastIndex * Chunk.Width, j * Chunk.Height, k * Chunk.Width);
                chunks[lastIndex, j, k].GetComponent<MeshRenderer>().enabled = false;
            }
        }
        SetChunkNames();
        DataGenerator.GenChunks(this, lastIndex, lastIndex, 0, lastIndex);

        SetChunkLocality(lastIndex - 1, lastIndex, 0, lastIndex);
        ChunkMeshGenerator.DrawChunks(this, lastIndex - 1, lastIndex, 0, lastIndex);
    }
    public void TraverseBack()
    {
        transform.position += Vector3.back * Chunk.Width;

        int lastIndex = width - 1;
        Chunk[,,] traversalChunks = GetChunks(0, lastIndex, lastIndex, lastIndex);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = lastIndex - 1; k >= 0; k--)
                {
                    chunks[i, j, k].transform.localPosition += Vector3.forward * Chunk.Width;
                    chunks[i, j, k + 1] = chunks[i, j, k];
                }
            }
        }
        SetChunks(0, lastIndex, 0, 0, traversalChunks);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                chunks[i, j, 0].transform.localPosition = new Vector3(i * Chunk.Width, j * Chunk.Height, 0);
                chunks[i, j, 0].GetComponent<MeshRenderer>().enabled = false;
            }
        }
        SetChunkNames();
        DataGenerator.GenChunks(this, 0, lastIndex, 0, 0);

        SetChunkLocality(0, lastIndex, 0, 1);
        ChunkMeshGenerator.DrawChunks(this, 0, lastIndex, 0, 1);
    }
    public void TraverseForward()
    {
        transform.position += Vector3.forward * Chunk.Width;

        int lastIndex = width - 1;
        Chunk[,,] traversalChunks = GetChunks(0, lastIndex, 0, 0);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 1; k <= lastIndex; k++)
                {
                    chunks[i, j, k].transform.localPosition += Vector3.back * Chunk.Width;
                    chunks[i, j, k - 1] = chunks[i, j, k];
                }
            }
        }
        SetChunks(0, lastIndex, lastIndex, lastIndex, traversalChunks);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                chunks[i, j, lastIndex].transform.localPosition = new Vector3(i * Chunk.Width, j * Chunk.Height, lastIndex * Chunk.Width);
                chunks[i, j, lastIndex].GetComponent<MeshRenderer>().enabled = false;
            }
        }
        SetChunkNames();
        DataGenerator.GenChunks(this, 0, lastIndex, lastIndex, lastIndex);

        SetChunkLocality(0, lastIndex, lastIndex - 1, lastIndex);
        ChunkMeshGenerator.DrawChunks(this, 0, lastIndex, lastIndex - 1, lastIndex);
    }
    private Chunk[,,] GetChunks(int xMin, int xMax, int zMin, int zMax)
    {
        Chunk[,,] temp = new Chunk[xMax - xMin + 1, Height, zMax - zMin + 1];
        for(int i = xMin; i <= xMax; i++)
        {
            for(int j = 0; j < Height; j++)
            {
                for(int k = zMin; k <= zMax; k++)
                {
                    //Debug.Log(chunks[i, j, k].Name);
                    temp[i - xMin, j, k - zMin] = chunks[i, j, k];
                }
            }
        }
        return temp;
    }
    private void SetChunks(int xMin, int xMax, int zMin, int zMax, Chunk[,,] withChunks)
    {
        for (int i = xMin; i <= xMax; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = zMin; k <= zMax; k++)
                {
                    //Debug.Log(chunks[i, j, k].Name + " << " + withChunks[i - xMin, j, k - zMin].Name);
                    chunks[i, j, k] = withChunks[i - xMin, j, k - zMin];
                }
            }
        }

    }
    private void SetChunkLocality (int xMin, int xMax, int zMin, int zMax)
    {
        // Set Chunk Locality
        for (int i = xMin; i <= xMax; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = zMin; k <= zMax; k++)
                {
                    // LEFT
                    try
                    {
                        chunks[i, j, k].LftChunk = chunks[i - 1, j, k];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        chunks[i, j, k].LftChunk = null;
                    }
                    // RIGHT
                    try
                    {
                        chunks[i, j, k].RgtChunk = chunks[i + 1, j, k];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        chunks[i, j, k].RgtChunk = null;
                    }
                    // BOTTOM
                    try
                    {
                        chunks[i, j, k].BtmChunk = chunks[i, j - 1, k];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        chunks[i, j, k].BtmChunk = null;
                    }
                    // TOP
                    try
                    {
                        chunks[i, j, k].TopChunk = chunks[i, j + 1, k];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        chunks[i, j, k].TopChunk = null;
                    }
                    // BACK
                    try
                    {
                        chunks[i, j, k].BckChunk = chunks[i, j, k - 1];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        chunks[i, j, k].BckChunk = null;
                    }
                    // FORWARD
                    try
                    {
                        chunks[i, j, k].FwdChunk = chunks[i, j, k + 1];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        chunks[i, j, k].FwdChunk = null;
                    }
                }
            }
        }
    }
    private void SetChunkNames()
    {
        string chunkName;
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                for(int k = 0; k < width; k++)
                {
                    chunkName = string.Format("Chunk ({0}, {1}, {2})", i, j, k);
                    chunks[i, j, k].Name = chunkName;
                }
            }
        }
    }

}