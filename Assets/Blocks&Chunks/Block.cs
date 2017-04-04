using UnityEngine;
using System.Collections;

public class Block
{

    // Special blocks divided into 3 parts:
    // _ _ _ _ _ _ _ _
    // ^ ^ ^ ^ ^ ^ ^ ^
    //  DETAIL ^ ^ ^ ^  240
    //     SUBTYPE ^ ^  12
    //            TYPE  3
    public static readonly byte TREE = 1;
    public static readonly byte MINERAL = 2;
    // something else can be 3 like structure or house
    // With structures there can be 3 different subtypes of structures and 
    // 15 different details although 2 bits may need to be reserved for its rotation
    // making it have 3 different details instead
    // MASKS
    public static readonly byte TYPE = 3;     //0 = none, 1 = tree, 2 = mineral, 3 = (structure?)
    public static readonly byte SUBTYPE = 12; //4, 8
    public static readonly byte DETAIL = 240; //16, 32, 64, 128
    // SHIFTS
    public static readonly byte SHIFT_TO_SUBTYPE = 2;
    public static readonly byte SHIFT_TO_DETAIL = 4;

    // ARRAY OF BLOCKS
    public static readonly byte TREE_START = 2;
    public static Block[] Blocks = {
        null,
        new Block("dirt", 100, Quad.uvDefault),
        new Block("brick", 100, Quad.uvDefault)
    };


    public string name;
    public int resistance;
    public Vector2[] uv;

    Block(string name, int resistance)
    {
        this.name = name;
        this.resistance = resistance;
    }
    Block(string name, int resistance, Vector2[] uv)
    {
        this.name = name;
        this.resistance = resistance;
        this.uv = uv;
    }

    //Parsing a tree:
    //TYPE = always 1
    //SUBTYPE = type of tree, oak is 0, pine is 1
    //DETAIL = height, min of 0, max of 15

}
