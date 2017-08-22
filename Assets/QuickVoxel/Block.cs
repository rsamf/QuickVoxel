using UnityEngine;
using System.Collections;

public class Block
{

    public static Rect[] UvMapping { get; set; }

    public string Name { get; set; }
    public int Resistance { get; set; }
    public Vector2[] UV { get; set; }

    Block(string name, int resistance = 100)
    {
        Name = name;
        Resistance = resistance;
        UV = Quad.UvDefault;
    }
    Block(string name, Vector2[] uv, int resistance = 100)
    {
        Name = name;
        Resistance = resistance;
        UV = uv;
    }

    public static void MapRectsToBlocks(Texture2D[] textures, Rect[] rects)
    {
        UvMapping = rects;
        for (int i = 0; i < textures.Length; i++)
        {
            Rect rect = rects[i];
            for (int j = 1; j < Blocks.Length; j++)
            {
                if (textures[i].name == Blocks[j].Name)
                {
                    Blocks[j].UV = new Vector2[] {
                        new Vector2(rect.xMin, rect.yMin),
                        new Vector2(rect.xMin, rect.yMax),
                        new Vector2(rect.xMax, rect.yMin),
                        new Vector2(rect.xMax, rect.yMax)
                    };
                    break;
                }
            }
        }
    }

    // PRE-ALLOC ARRAY OF BLOCKS (expecting to be long list)
    public static readonly Block[] Blocks = {
        null,
        new Block("dirt"),
        new Block("stone")
    };


}
