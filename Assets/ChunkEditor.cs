using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class ChunkEditor : MonoBehaviour {

    public Transform cube;
    public Camera cam;

    // Use this for initialization
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray rayTest = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitTest;
        Physics.Raycast(rayTest, out hitTest, 1000f);
        Debug.DrawRay(rayTest.origin + transform.forward, hitTest.point - rayTest.origin, Color.green);
        cube.position = new Vector3((int)hitTest.point.x, (int)hitTest.point.y, (int)hitTest.point.z);


        if (Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Vector3 relativePoint = hit.point - World.position;
                //Debug.Log(hit.point + " >> " + relativePoint);
                Vector3 chunkPosition = new Vector3(relativePoint.x / Chunk.WIDTH, relativePoint.y / Chunk.HEIGHT, relativePoint.z / Chunk.WIDTH);
                Vector3 blockPosition = new Vector3(relativePoint.x % Chunk.WIDTH, relativePoint.y % Chunk.HEIGHT, relativePoint.z % Chunk.WIDTH);
                Index chunkIndex = new Index((int)chunkPosition.x, (int)chunkPosition.y, (int)chunkPosition.z);
                Index blockIndex = new Index((int)blockPosition.x, (int)blockPosition.y, (int)blockPosition.z);
                //Debug.Log("AT CHUNK: " + chunkIndex + " AND AT BLOCK: " + blockIndex);
                byte[,,] blocks = World.chunks[chunkIndex.i, chunkIndex.j, chunkIndex.k].blocks;
                if (blocks[blockIndex.i, blockIndex.j, blockIndex.k] != 0)
                {
                    blocks[blockIndex.i, blockIndex.j, blockIndex.k] = 0;
                    ChunkMeshGen.RemoveOne(chunkIndex, blockIndex);
                }



            
            }
        }
    }
}
