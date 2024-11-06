using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Custom Tile", menuName = "Mining/Custom Tile")]
public class TileData : ScriptableObject
{
    public TILE_NAME tileName;
    public TileBase tileBase;
    public bool isOre;
}

public enum TILE_NAME
{
    IRON, GOLD, ROCK, COPPER
}
