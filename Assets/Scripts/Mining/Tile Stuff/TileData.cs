using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Custom Tile", menuName = "Mining/Custom Tile")]
public class TileData : ScriptableObject
{
    public TILE_NAME tileName;
    public TileBase tileBase;
    public float maxBreakTime;
    public int hardness;
}

public enum TILE_NAME
{
    SOFT_ROCK,
    MEDIUM_ROCK,
    HARD_ROCK,
    EXTREMELY_HARD_ROCK,
    IRON,
    GOLD,
    COPPER,
    COLBOLT,
    LAVA,
    WATER,
}
