using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Hello World for QuickVoxel
public class WorldSpawner : MonoBehaviour {

    public GameObject worldToSpawn;
    public GameObject chunkToSpawn;

    public int width = 3;
    public int height = 2;


	void Start () {
        Rect[] rects;
        Texture2D[] textures;
        World world;

        /*
         * Texture Generation (optional)
         */

        // Set the texture of chunk to the Atlas
        chunkToSpawn.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = TextureAtlasGenerator.Generate("Textures", 8192, out rects, out textures);
        // Map rects/uvs
        TextureAtlasGenerator.MapRectsToBlocks(textures, rects);

        /*
         * Actual Voxel world generation 
         */

        // Create GameObject for world
        world = Instantiate(worldToSpawn, Vector3.zero, Quaternion.identity, transform).GetComponent<World>();
        // Call Initalize to create chunks
        world.Initialize(width, height, chunkToSpawn, DataGenerator.Config.Default);
    }
	
}
