using UnityEngine;
using System.Collections;

public static class DataGenerator
{
    public static readonly Config CONFIG = new Config
    (
        World.SEED, .01f, 0, 5000, 16
    //SEED      scale       elev        random          height
    );



    public struct Config
    {
        public int seed, elevation;
        public float perlinScale, randomness, heightScale;

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

    //GENERATES THE DATA
    public static void GenBlocksAt(Index forIndex)
    {
        byte[,,] blocks;
        float height;
        Vector3 forPosition = forIndex.Position;
        for (int i = 0; i < Chunk.WIDTH; i++)
        {
            for (int k = 0; k < Chunk.WIDTH; k++)
            {
                height = GetPerlin(i + forPosition.x, k + forPosition.z, CONFIG);
                for (int chunkIndexY = 0, offset = 0; chunkIndexY < World.HEIGHT; chunkIndexY++, offset += Chunk.HEIGHT)
                {
                    blocks = World.chunks[forIndex.i, chunkIndexY, forIndex.k].blocks;
                    for(int j = 0; j < Chunk.HEIGHT; j++)
                    {
                        if(j + offset < height)
                        {
                            //DIRT
                            blocks[i, j, k] = 1;
                        } else
                        {
                            //AIR
                            blocks[i, j, k] = 0;
                        }
                    }
                }
            }
        }
    }

}
