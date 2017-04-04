using UnityEngine;
using System;
using System.Collections.Generic;

public class World : MonoBehaviour
{

    [Range(0, 10000)]
    public int seed = 5000;
    public static int SEED = 5000;
    //Game
    public static GameObject player;
    public static Vector3 position;

    //Chunk Generation
    public static readonly int WIDTH = 11, HEIGHT = 3;
    static Transform activeRoot, fakeRoot;
    public static Chunk[,,] chunks;

    void Start()
    {
        // If seed is 0, then choose a random seed
        if(seed == 0)
        {
            System.Random rGen = new System.Random();
            SEED = rGen.Next(10000);
        } else
        {
            SEED = seed;
        }
        Debug.Log("Starting with seed " + SEED);


        //Create Chunks
        chunks = new Chunk[WIDTH, HEIGHT, WIDTH];
        GameObject temp;
        for(int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int k = 0; k < WIDTH; k++)
                {
                    temp = Instantiate(Resources.Load("Chunk"), this.transform) as GameObject;
                    chunks[i, j, k] = temp.GetComponent<Chunk>();
                    temp.transform.position = new Vector3(i * Chunk.WIDTH, j * Chunk.HEIGHT, k * Chunk.WIDTH);
                }
            }
        }
        //Generate Block Data
        for (int i = 0; i < WIDTH; i++)
        {
            for(int k = 0; k < WIDTH; k++)
            {
                DataGenerator.GenBlocksAt(new Index(i, k));
            }
        }
        //Generate Mesh Data
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int k = 0; k < WIDTH; k++)
                {
                    ChunkMeshGen.Draw(new Index(i, j, k));
                }
            }
        }
        StartCoroutine(ChunkMeshGen.Initialize());

        //Dump
        /*
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                for (int k = 0; k < WIDTH; k++)
                {
                    chunks[i, j, k].vuts = null;
                    chunks[i, j, k].blocks = null;
                }
            }
        }*/
    }
    // Mainly used to tell thread in ChunkMeshGen to stop running
    private void OnApplicationQuit()
    {
        
        ChunkMeshGen.AppAlive = false;
        Debug.Log("Application ending now");
    }

}