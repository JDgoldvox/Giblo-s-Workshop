using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using Unity.Mathematics;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;
    public Tilemap tilemap;

    //all tiles
    [SerializeField] private List<TileData> tileDatas = new List<TileData>();

    //tile base to tile data
    public Dictionary<TileBase, TileData> tileToData = new Dictionary<TileBase, TileData>();

    //name to tile base
    public Dictionary<TILE_NAME, TileBase> tileNameToTileBase = new Dictionary<TILE_NAME, TileBase>();

    //for generation offsets of perlin noise
    private Dictionary<TILE_NAME, float> tileNameToOffset = new Dictionary<TILE_NAME, float>();

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
        //1. generate seed
        GenerateRandomOffSets();

        //2. spawn
        SpawnTiles();
    }
    private void SpawnTiles()
    {
        //different each time we generate it
        float randomXOffset = UnityEngine.Random.Range(0.01f, 100f);
        float randomYOffset = UnityEngine.Random.Range(0.01f, 100f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float rng = Mathf.PerlinNoise((x + randomXOffset) * 0.2f, (y + randomYOffset) * 0.2f);
                rng = Mathf.Clamp(rng, 0, 1);

                //grab tile according to random number and y height
                TileBase generatedTile = GetTileToMatchHeight(rng, y);

                //set tile
                tilemap.SetTile(new Vector3Int(x, -y, 0), generatedTile);
            }
        }
    }

    private TileBase GetTileToMatchHeight(float rng, int height)
    {
        //IF Y LESS THAN 50
        if (height < 50)
        {
            return rng switch
            {
                <= 0.1f => tileNameToTileBase.TryGetValue(TILE_NAME.COPPER, out TileBase tile) ? tile : null,
                <= 1f => tileNameToTileBase.TryGetValue(TILE_NAME.SOFT_ROCK, out TileBase tile) ? tile : null,
                _ => null
            };
        }
        else if (height < 100)
        {
            return rng switch
            {
                <= 0.12f => tileNameToTileBase.TryGetValue(TILE_NAME.COPPER, out TileBase tile) ? tile : null,
                <= 1f => tileNameToTileBase.TryGetValue(TILE_NAME.SOFT_ROCK, out TileBase tile) ? tile : null,
                _ => null
            }; ;
        }
        else if (height < 200)
        {
            return rng switch
            {
                <= 0.16f => tileNameToTileBase.TryGetValue(TILE_NAME.COPPER, out TileBase tile) ? tile : null,
                <= 1f => tileNameToTileBase.TryGetValue(TILE_NAME.SOFT_ROCK, out TileBase tile) ? tile : null,
                _ => null
            }; ;
        }
        else if (height < 500)
        {
            return rng switch
            {
                <= 0.20f => tileNameToTileBase.TryGetValue(TILE_NAME.COPPER, out TileBase tile) ? tile : null,
                <= 0.30f => tileNameToTileBase.TryGetValue(TILE_NAME.IRON, out TileBase tile) ? tile : null,
                <= 1f => tileNameToTileBase.TryGetValue(TILE_NAME.SOFT_ROCK, out TileBase tile) ? tile : null,
                _ => null
            }; ;
        }

        return null;

    }

    private void GenerateRandomOffSets()
    {
        foreach(TILE_NAME tileName in tileNameToTileBase.Keys)
        {
            float randomOffset = UnityEngine.Random.Range(0.01f, 100f);
            tileNameToOffset.Add(tileName, randomOffset);
        }
    }
}
