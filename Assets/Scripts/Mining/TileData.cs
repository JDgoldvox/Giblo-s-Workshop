using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Custom Tile", menuName = "Mining/Custom Tile")]
public class TileData : ScriptableObject
{
    public TileBase tileBase;
    public bool isOre;
}
