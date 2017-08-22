using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureAtlasGenerator
{

    private static readonly int padding = 2;
    private static readonly float cropPercentage = .01f; //5% of each texture is going to be cut off 
    public static Texture2D RecentlyGeneratedAtlas { get; set; }

    public static Texture2D Generate(Texture2D[] textures, int size, out Rect[] rects)
    {
        // Create Atlas
        Texture2D atlas = new Texture2D(size, size);
        rects = atlas.PackTextures(textures, padding, size);
        atlas.filterMode = FilterMode.Point;
        RecentlyGeneratedAtlas = atlas;

        // Crop UVs
        float width;
        float height;
        for(int i = 0; i < rects.Length; i++)
        {
            width = rects[i].width;
            height = rects[i].height;
            rects[i].xMin += cropPercentage * width;
            rects[i].xMax -= cropPercentage * width;
            rects[i].yMin += cropPercentage * height;
            rects[i].yMax -= cropPercentage * height;
            Debug.Log(textures[i].name);
            Debug.Log(rects[i]);
        }
        return atlas;
    }


    public static Texture2D Generate(string texturesPath, int size, out Rect[] rects, out Texture2D[] textures)
    {
        Object[] texturesAsObjects = Resources.LoadAll(texturesPath);
        textures = new Texture2D[texturesAsObjects.Length];
        for(int i = 0; i < textures.Length; i++)
        {
            textures[i] = (Texture2D) texturesAsObjects[i];
        }
        return Generate(textures, size, out rects);
    }


    public static void MapRectsToBlocks(Texture2D[] textures, Rect[] rects)
    {
        Block.MapRectsToBlocks(textures, rects);
    }
}
