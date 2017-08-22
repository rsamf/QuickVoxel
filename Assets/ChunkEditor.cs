using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class ChunkEditor : MonoBehaviour {

    public Transform cube;
    public Camera cam;
    public Shape oneAir;
    public Shape oneBlock;
    public Shape editOneBlock;
    public Shape plusAir;
    public Shape cubeAir;
    public Shape cubeBlock;
    public Vector3 offset = new Vector3(.5f, 0, .5f);

    private Work[] works;
    private int workIndex;
    private Vector3 targetPosition;
    private Index blockIndex;

    public World world;


    // Use this for initialization
    void Start()
    {
        works = new Work[25];
        workIndex = 0;
        for(int i = 0; i < works.Length; i++)
        {
            works[i] = new Work();
        }
        targetPosition = Vector3.zero;
        blockIndex = new Index();

        List<Process> p = new List<Process>();
        p.Add(new Process(new Index(0, 0, 0), ChunkMeshGenerator.RemoveBlock));
        p.Add(new Process(new Index(-1, 0, 0), ChunkMeshGenerator.RemoveBlock));
        p.Add(new Process(new Index(1, 0, 0), ChunkMeshGenerator.RemoveBlock));
        p.Add(new Process(new Index(0, 0, -1), ChunkMeshGenerator.RemoveBlock));
        p.Add(new Process(new Index(0, 0, 1), ChunkMeshGenerator.RemoveBlock));

        List<Process> p1 = new List<Process>();
        for(int i = -4; i < 5; i++)
        {
            for(int j = -4; j < 5; j++)
            {
                for(int k = -4; k < 5; k++)
                {
                    p1.Add(new Process(new Index(i, j, k), ChunkMeshGenerator.RemoveBlock));
                }
            }
        }
        List<Process> p2 = new List<Process>();
        for (int i = -4; i < 5; i++)
        {
            for (int j = -4; j < 5; j++)
            {
                for (int k = -4; k < 5; k++)
                {
                    p2.Add(new Process(new Index(i, j, k), ChunkMeshGenerator.AddBlock, 1));
                }
            }
        }

        cubeBlock = new Shape(p2);
        cubeBlock.Optimize();
        cubeAir = new Shape(p1);
        cubeAir.Optimize();
        plusAir = new Shape(p);
        plusAir.Optimize();
        cam = GetComponent<Camera>();
        oneAir = new Shape(ChunkMeshGenerator.RemoveBlock);
        oneBlock = new Shape(ChunkMeshGenerator.AddBlock, 1);
        //editOneBlock = new Shape(ChunkMeshGenerator.EditBlock, 1);
        oneAir.Optimize();
        oneBlock.Optimize();
        //editOneBlock.Optimize();
    }

    // Update is called once per frame
    void Update()
    {
        
        Ray rayTest = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitTest;
        Physics.Raycast(rayTest, out hitTest, 1000f);
        Debug.DrawRay(rayTest.origin + transform.forward, hitTest.point - rayTest.origin, Color.green);
        hitTest.point += offset;
        targetPosition.x = (int)hitTest.point.x;
        targetPosition.y = (int)hitTest.point.y;
        targetPosition.z = (int)hitTest.point.z;
        cube.position = targetPosition;

        if(Physics.Raycast(rayTest, out hitTest, 1000f))
        {
            if (Input.GetMouseButton(0))
            {
                // Remove Block (Create Air)
                CreateBlock(hitTest, cubeAir, false);
            }
            else if (Input.GetMouseButton(1))
            {
                // Add Block (Create Stone/Dirt)
                CreateBlock(hitTest, cubeBlock, true);
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            CreateRandom(cubeAir, false);
        }
    }
    private void CreateRandom(Shape shape, bool append = false)
    {
        //World world = GameObject.Find("World Spawner").GetComponentInChildren<World>();
        Index blockIndex = new Index((int)(Random.value * Chunk.Width), (int)(Random.value * Chunk.Height), (int)(Random.value * Chunk.Width));
        Index chunkIndex = new Index((int)(Random.value * world.Width), 0, (int)(Random.value * world.Width));
        Work work = new Work(world.chunks[chunkIndex.i, chunkIndex.j, chunkIndex.k], blockIndex, shape);
        ChunkMeshGenerator.EnqueueWork(work);
    }

    private void CreateBlock(RaycastHit hit, Shape shape, bool append=false)
    {
        Chunk chunk = hit.transform.GetComponent<Chunk>();
        hit.point += offset;
        blockIndex.i = (int)(append ? hit.normal.x + hit.point.x % Chunk.Width : hit.point.x % Chunk.Width);
        blockIndex.j = (int)(append ? hit.normal.y + hit.point.y % Chunk.Width : hit.point.y % Chunk.Width);
        blockIndex.k = (int)(append ? hit.normal.z + hit.point.z % Chunk.Width : hit.point.z % Chunk.Width);
        Work work = works[workIndex];
        work.CenterChunk = chunk;
        work.CenterBlockIndex = blockIndex;
        work.Shape = shape;
        ChunkMeshGenerator.EnqueueWork(work);
        workIndex = (workIndex + 1) % 25;
    }
}