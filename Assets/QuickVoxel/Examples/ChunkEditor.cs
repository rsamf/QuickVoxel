using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Note:
    * This example, not quite part of QuickVoxel, has a bug.
    * hit.transform may point to an unexpected Transform while the Index calculated in blockIndex in CreateBlock()
    * is corresponding to a different Transform. Nevertheless, this is useful for testing. For accurate block edits
    * Use the CreateRandom() which looks like its deteriorating the landmass
    */
[RequireComponent(typeof(Camera))]
public class ChunkEditor : MonoBehaviour {
    public World world;
    public Transform cube;
    // One block
    public Shape oneAir; // one block of air (data = 0)
    public Shape oneDirt; // one block of dirt (data = 1)
    // Multi-block
    public Shape plusAir; // plus shape (+) of air
    public Shape cubeAir; // full cube of air
    public Shape cubeDirt; // full cube of dirt

    private Vector3 offset = new Vector3(.5f, 0, .5f);
    private Camera cam;
    private Vector3 targetPosition = Vector3.zero;
    private Index blockIndex = new Index();

    void Start()
    {
        cam = GetComponent<Camera>();

        // Create Some shapes
        oneAir = new Shape();
        oneAir.Optimize();
        oneDirt = new Shape(1);
        oneDirt.Optimize();

        /* 
         * A Process contains a list of Indexes and bytes. Index provides information of where to place the block (in 
         * relation to the center block where the center block is at (0,0,0)) and the byte is the type of block to 
         * place where 0 is reserved for air and 1-255 is whatever you wish to be. These are used to create more complex
         * shapes.
         */

        // A plus shape of air
        List<Process> plusProcesses = new List<Process>();
        plusProcesses.Add(new Process(new Index(0, 0, 0), 0));
        plusProcesses.Add(new Process(new Index(-1, 0, 0), 0));
        plusProcesses.Add(new Process(new Index(1, 0, 0), 0));
        plusProcesses.Add(new Process(new Index(0, 0, -1), 0));
        plusProcesses.Add(new Process(new Index(0, 0, 1), 0));
        plusAir = new Shape(plusProcesses);
        plusAir.Optimize();

        // An 4x4x4 cube of air
        List<Process> cubeProcessesAllAir = new List<Process>();
        for(int i = -2; i < 2; i++)
        {
            for(int j = -2; j < 2; j++)
            {
                for(int k = -2; k < 2; k++)
                {
                    cubeProcessesAllAir.Add(new Process(new Index(i, j, k), 0));
                }
            }
        }
        cubeAir = new Shape(cubeProcessesAllAir);
        cubeAir.Optimize();

        // An 8x8x8 cube of dirt
        List<Process> cubeProcessesAllDirt = new List<Process>();
        for (int i = -4; i < 5; i++)
        {
            for (int j = -4; j < 5; j++)
            {
                for (int k = -4; k < 5; k++)
                {
                    cubeProcessesAllDirt.Add(new Process(new Index(i, j, k), 1));
                }
            }
        }
        cubeDirt = new Shape(cubeProcessesAllDirt);
        cubeDirt.Optimize();
    }

    

    void Update()
    {
        
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 1000f);
        Debug.DrawRay(ray.origin + transform.forward, hit.point - ray.origin, Color.green);
        hit.point += offset;
        targetPosition.x = (int)hit.point.x;
        targetPosition.y = (int)hit.point.y;
        targetPosition.z = (int)hit.point.z;
        cube.position = targetPosition;

        if(Physics.Raycast(ray, out hit, 1000f))
        {
            if (Input.GetMouseButton(0))
            {
                // Remove Block (Create Air)
                CreateBlock(hit, plusAir, false);
            }
            else if (Input.GetMouseButton(1))
            {
                // Add Block (Create Stone/Dirt)
                CreateBlock(hit, cubeAir, true);
               
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            CreateRandom(cubeAir, false);
        }
    }

    private void CreateBlock(RaycastHit hit, Shape shape, bool append = false)
    {
        Chunk chunk = hit.transform.GetComponent<Chunk>();

        hit.point += offset;
        blockIndex.i = (int)(append ? hit.normal.x + hit.point.x % Chunk.Width : hit.point.x % Chunk.Width);
        blockIndex.j = (int)(append ? hit.normal.y + hit.point.y % Chunk.Width : hit.point.y % Chunk.Width);
        blockIndex.k = (int)(append ? hit.normal.z + hit.point.z % Chunk.Width : hit.point.z % Chunk.Width);
        ChunkMeshGenerator.EnqueueWork(new Work(chunk, blockIndex, shape));
    }

    private void CreateRandom(Shape shape, bool append = false)
    {
        //World world = GameObject.Find("World Spawner").GetComponentInChildren<World>();
        Index blockIndex = new Index((int)(Random.value * Chunk.Width), (int)(Random.value * Chunk.Height), (int)(Random.value * Chunk.Width));
        Index chunkIndex = new Index((int)(Random.value * world.Width), 0, (int)(Random.value * world.Width));
        Work work = new Work(world.chunks[chunkIndex.i, chunkIndex.j, chunkIndex.k], blockIndex, shape);
        ChunkMeshGenerator.EnqueueWork(work);
    }
}