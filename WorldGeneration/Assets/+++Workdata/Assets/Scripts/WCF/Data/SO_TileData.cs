using UnityEngine;

[CreateAssetMenu(fileName = "SO_TileData", menuName = "Scriptable Objects/SO_TileData")]
public class SO_TileData : ScriptableObject
{
    public enum TileType
    {
        Water,
        Land,
        Coast,
        WaterShallow,
        LandInner,
        LandSpecial,
        Snow,
        Ice,
        SnowSpecial
    }

    public TileType type;
    public Sprite tileSprite;
    public bool walkable;
    
    //TODO: Varianz with spritearray.
}
