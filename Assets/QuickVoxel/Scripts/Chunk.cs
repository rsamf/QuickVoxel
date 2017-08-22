using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class Chunk : MonoBehaviour
{

    public static readonly byte Width = 16;
    public static readonly byte Height = 16;
    public byte[,,] Blocks { get; set; }
    public PrimVUT[,,] Vuts { get; set; }
    public VUT MainVUT { get; set; }

    private MeshFilter mFilter;
    private MeshRenderer mRenderer;
    private MeshCollider mCollider;
    public Mesh MyMesh { get; set; }

    public Chunk LftChunk { get; set; }
    public Chunk RgtChunk { get; set; }
    public Chunk BtmChunk { get; set; }
    public Chunk TopChunk { get; set; }
    public Chunk BckChunk { get; set; }
    public Chunk FwdChunk { get; set; }

    public string Name { get; set; }

    // Use this for initialization
    void Awake()
    {
        Blocks = new byte[Width, Height, Width];
        Vuts = new PrimVUT[Width, Height, Width];
        MainVUT = new VUT(new List<Vector3>(), new List<Vector2>(), new List<int>());
        mFilter = GetComponent<MeshFilter>();
        mRenderer = GetComponent<MeshRenderer>();
        mCollider = GetComponent<MeshCollider>();
        MyMesh = mFilter.mesh;
        //MyMesh.MarkDynamic();
        enabled = false;
    }

    public void SetMesh(VUT vut)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        vut.ToMesh(MyMesh);
        mCollider.sharedMesh = MyMesh;
        mRenderer.enabled = true;
        //UnityEngine.Debug.Log("C " + this.Name + " " + sw.ElapsedMilliseconds / 1000f);
    }


    public void ReadBlocks()
    {
        string str = "";
        for (byte j = 0; j < Height; j++)
        {
            for (byte i = 0; i < Width; i++)
            {
                for (byte k = 0; k < Width; k++)
                {
                    str += Blocks[i, j, k] + ", ";
                }
                UnityEngine.Debug.Log(str);
                str = "";
            }
        }
    }
    public void MatchWith(byte[,,] blocks)
    {
        for (byte j = 0; j < Height; j++)
        {
            for (byte i = 0; i < Width; i++)
            {
                for (byte k = 0; k < Width; k++)
                {
                    if (this.Blocks[i, j, k] != blocks[i, j, k])
                    {
                        UnityEngine.Debug.Log("DOES NOT MATCH");
                        return;
                    }
                }
            }
        }
        UnityEngine.Debug.Log("MATCHES");
    }
    public void ClearBlocks()
    {
        for (byte i = 0; i < Width; i++)
        {
            for (byte j = 0; j < Height; j++)
            {
                for (byte k = 0; k < Width; k++)
                {
                    Blocks[i, j, k] = 0;
                }
            }
        }
    }
    public override string ToString()
    {
        return this.Name;
    }
}