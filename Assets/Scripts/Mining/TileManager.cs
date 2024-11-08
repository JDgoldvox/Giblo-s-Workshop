using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;
    public Tilemap tilemap;
    [SerializeField] private List<TileData> tileDatas = new List<TileData>();
    public Dictionary<TileBase, TileData> tileToData = new Dictionary<TileBase, TileData>();
    public Dictionary<TILE_NAME, TileBase> tileNameToTileBase = new Dictionary<TILE_NAME, TileBase>();

    private int width = 20;
    private int height = 1000;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        //match tile base to tile data
        foreach (var tileData in tileDatas)
        {
            tileToData.Add(tileData.tileBase, tileData);
            tileNameToTileBase.Add(tileData.tileName, tileData.tileBase);
        }
    }

    void Start()
    {
        SpawnTiles();
    }
    private void SpawnTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height ; y++)
            {
                //roll random number
                int rng = UnityEngine.Random.Range(0, 100);
                
                //grab tile according to random number and y height
                TileBase generatedTile = GetTileToMatchHeight(rng, y);

                //set tile
                tilemap.SetTile(new Vector3Int(x, -y, 0), generatedTile);
            }

        }
    }

    private TileBase GetTileToMatchHeight(int rng, int height)
    {
        //IF Y LESS THAN 50
        if(height < 50)
        {
            return rng switch
            {
                <= 20 => tileNameToTileBase.TryGetValue(TILE_NAME.COPPER, out TileBase tile) ? tile : null,
                < 100 => tileNameToTileBase.TryGetValue(TILE_NAME.SOFT_ROCK, out TileBase tile) ? tile : null,
                _ => null // Default case
            };
        }

        return null;
        
    }
}
