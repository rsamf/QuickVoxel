using UnityEngine;
using System.Collections;

// Voxel data generation
// 0 = Air
// 1 = Stone/ground
// 2-255 = ... your choice (see Block class)
public static class DataGenerator
{
    public static readonly Config Default = new Config(1, .01f, 0, 5000, 16);

    public struct Config
    {
        public int seed, elevation;
        public float perlinScale, randomness, heightScale;

        public static readonly Config Default = new Config(1, .01f, 0, 5000, 16);

        public Config(int seed, float perlinScale, int elevation, float randomness, float heightScale)
        {
            this.seed = seed;
            this.perlinScale = perlinScale;
            this.elevation = elevation;
            this.randomness = randomness;
            this.heightScale = heightScale;
        }
        public string toString()
        {
            return "Scale: " + this.perlinScale + ", Randomness: " + this.randomness + ", HeightScale: " + this.heightScale;
        }
    }


    public static int GetPerlin(float i, float k, Config config)
    {
        Random.InitState(config.seed);
        float xRandom = i + Random.Range(-config.randomness, config.randomness);
        float zRandom = k + Random.Range(-config.randomness, config.randomness);
        int solution =
            Mathf.FloorToInt(Mathf.PerlinNoise(
                //Perlin operations
                xRandom * config.perlinScale, zRandom * config.perlinScale
            //Scaled to height
            ) * config.heightScale)
        ;
        if (solution > config.heightScale)
        {
            //Debug.Log("ERROR at (" + i + ", " + k + ") is " + solution + " REDUCING IT TO 0");
            solution = 0;
        }
        return solution;
    }

    public static void GenWorld(World world)
    {
        GenChunks(world, 0, world.Width - 1, 0, world.Width - 1);
    }
    public static void GenChunks(World world, int xMin, int xMax, int zMin, int zMax)
    {
        float h;
        byte[,,] blocks;
        for(int chunkX = xMin; chunkX <= xMax; chunkX++)
        {
            for(int chunkZ = zMin; chunkZ <= zMax; chunkZ++)
            {
                // Iterate blocks from top perspective
                for (int blockX = 0; blockX < Chunk.Width; blockX++)
                {
                    for (int blockZ = 0; blockZ < Chunk.Width; blockZ++)
                    {
                        // Get height that blocks should go up to
                        h = GetPerlin(world.Position.x + blockX + chunkX * Chunk.Width, world.Position.z + blockZ + chunkZ * Chunk.Width, world.Config);
                        // Iterate along y axis of chunks
                        for (int chunkY = 0, offset = 0; chunkY < world.Height; chunkY++, offset += Chunk.Height)
                        {
                            // Now we finally know which chunk we're dealing with, get blocks of this chunk
                            blocks = world.chunks[chunkX, chunkY, chunkZ].Blocks;
                            // Iterate along y axis of blocks, and set the blocks of this chunk
                            for (int blockY = 0; blockY < Chunk.Height; blockY++)
                            {
                                if (blockY + offset < h)
                                {
                                    //STONE
                                    blocks[blockX, blockY, blockZ] = 2;
                                }
                                else if (blockY + offset == h)
                                {
                                    //DIRT
                                    blocks[blockX, blockY, blockZ] = 1;
                                }
                                else
                                {
                                    //AIR
                                    blocks[blockX, blockY, blockZ] = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
