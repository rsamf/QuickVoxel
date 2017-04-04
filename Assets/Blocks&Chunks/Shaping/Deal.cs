/*
 * Deal is in the form of coordinates, and the shape that I'm dealing/handling with
 */
public class Deal
{
    public Index chunkIndex, blockIndex;
    public Process custom;
    public IShape shape;

    public Deal(Index2 index, Process custom, IShape shape)
    {
        this.chunkIndex = index.chunkIndex;
        this.blockIndex = index.blockIndex;
        this.custom = custom;
        this.shape = shape; 
    }
    public Deal(Index chunkIndex, Index blockIndex, Process custom, IShape shape)
    {
        this.chunkIndex = chunkIndex;
        this.blockIndex = blockIndex;
        this.custom = custom;
        this.shape = shape;
    }

    public void Start()
    {
        this.shape.Draw(chunkIndex, blockIndex, custom);
    }
}
