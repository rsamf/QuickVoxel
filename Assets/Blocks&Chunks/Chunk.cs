using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class Chunk : MonoBehaviour
{

    public static readonly byte WIDTH = 16;
    public static readonly byte HEIGHT = 16;
    public byte[,,] blocks;
    public PrimVUT[,,] vuts;

    public MeshFilter mFilter;
    public MeshRenderer mRenderer;
    public MeshCollider mCollider;
    public Mesh mesh;



    void Start()
    {
        enabled = false;
    }

    // Use this for initialization
    void Awake()
    {
        blocks = new byte[WIDTH, HEIGHT, WIDTH];
        vuts = new PrimVUT[WIDTH, HEIGHT, WIDTH];
        mFilter = GetComponent<MeshFilter>();
        mRenderer = GetComponent<MeshRenderer>();
        mCollider = GetComponent<MeshCollider>();
        mesh = mFilter.mesh;
        mesh.MarkDynamic();
    }


    public void ReadBlocks()
    {
        string str = "";
        for (byte j = 0; j < HEIGHT; j++)
        {
            for (byte i = 0; i < WIDTH; i++)
            {
                for (byte k = 0; k < WIDTH; k++)
                {
                    str += blocks[i, j, k] + ", ";
                }
                Debug.Log(str);
                str = "";
            }
        }
    }
    public void MatchWith(byte[,,] blocks)
    {
        for (byte j = 0; j < HEIGHT; j++)
        {
            for (byte i = 0; i < WIDTH; i++)
            {
                for (byte k = 0; k < WIDTH; k++)
                {
                    if (this.blocks[i, j, k] != blocks[i, j, k])
                    {
                        Debug.Log("DOES NOT MATCH");
                        return;
                    }
                }
            }
        }
        Debug.Log("MATCHES");
    }
    public void ClearBlocks()
    {
        for (byte i = 0; i < WIDTH; i++)
        {
            for (byte j = 0; j < HEIGHT; j++)
            {
                for (byte k = 0; k < WIDTH; k++)
                {
                    blocks[i, j, k] = 0;
                }
            }
        }
    }
}
