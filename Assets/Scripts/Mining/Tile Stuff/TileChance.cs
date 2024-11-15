using System.Collections.Generic;
using UnityEngine;
using static TileChance;

[System.Serializable]
public class TileChancePair
{
    public TILE_NAME tileName;
    public TILE_CHANCE chance;
}
public enum TILE_CHANCE
{
    VERY_LOW,
    LOW,
    MEDIUM,
    HIGH,
    VERY_HIGH
}

[CreateAssetMenu(fileName = "TileChance", menuName = "Mining/TileChance")]
public class TileChance : ScriptableObject
{
    

    public int Height;
    public TILE_NAME fillTile;
    public List<TileChancePair> probability = new List<TileChancePair>();
}


