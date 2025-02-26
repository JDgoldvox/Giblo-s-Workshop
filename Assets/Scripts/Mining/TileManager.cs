using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UIElements;
using System.Collections;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;
    [SerializeField]  public Tilemap tilemap;
    [SerializeField] public Tilemap breakTileMap;

    //all tiles
    public List<TileData> tileDatas = new List<TileData>();
    [SerializeField] private List<TileBase> breakTiles = new List<TileBase>();

    //tile base to tile data
    public Dictionary<TileBase, TileData> tileToData = new Dictionary<TileBase, TileData>();

    //name to tile base
    public Dictionary<TILE_NAME, TileBase> tileNameToTileBase = new Dictionary<TILE_NAME, TileBase>();

    //for generation offsets of perlin noise
    private Dictionary<TILE_NAME, float> tileNameToOffset = new Dictionary<TILE_NAME, float>();

    private int width = 60;
    private int height = 5000;

    private float randomXOffset = 0;
    private float randomYOffset = 0;

    float perlinNoiseScale = 0.2f;

    private float veryLow = 0.08f;
    private float low = 0.10f;
    private float medium = 0.12f;
    private float high = 0.14f;
    private float veryHigh = 0.16f;


    //SOs for height to chance and fill tile
    [SerializeField] TileChance[] tileChancesSO;
    private Dictionary<int, TileChance> tileChances = new Dictionary<int, TileChance>();

    //live tile data
    public Dictionary<Vector3Int, LiveTileData> liveTileData = new Dictionary<Vector3Int, LiveTileData>();

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

        //2. put data in chances for tile to spawn
        SetTileHeightData();

        randomXOffset = UnityEngine.Random.Range(0.01f, 100f);
        randomYOffset = UnityEngine.Random.Range(0.01f, 100f);

        //3. spawn tiles
        SpawnTiles();

        //4. Set Live Data
        SetLiveData();
    }
    private void SpawnTiles()
    {
        int lastYInterval = 0;
        int sectionInterval = 200;

        while (lastYInterval < height) {
            LoopAndPlaceTilesInSection(lastYInterval, lastYInterval + sectionInterval, tileChances[lastYInterval]);
            lastYInterval += sectionInterval;
        }
        
    }

    private void GenerateRandomOffSets()
    {
        foreach(TILE_NAME tileName in tileNameToTileBase.Keys)
        {
            float randomOffset = UnityEngine.Random.Range(0.01f, 100f);
            tileNameToOffset.Add(tileName, randomOffset);
        }
    }

    private void LoopAndPlaceTilesInSection(int currentHeight, int maxHeight, TileChance tileChanceForThisHeight)
    {
        float chance;
        TILE_NAME currentTileName;

        //set fill tile first
        currentTileName = tileChanceForThisHeight.fillTile;
        chance = 1;
        for (int x = 0; x < width; x++)
        {
            for (int y = currentHeight; y < maxHeight; y++)
            {
                tilemap.SetTile(new Vector3Int(x, -y, 0), tileNameToTileBase[currentTileName]);
            }
        }

        TileChancePair tileChancePair;
        int probNum = tileChanceForThisHeight.probability.Count();

        for (int i = 0; i < probNum; i++)
        {
            tileChancePair = tileChanceForThisHeight.probability[i];
            currentTileName = tileChancePair.tileName;
            chance = ChanceToFloat(tileChancePair.chance);

            //after base placed down
            for (int x = 0; x < width; x++)
            {
                for (int y = currentHeight; y < maxHeight; y++)
                {
                    float rng = Mathf.PerlinNoise((x + randomXOffset + tileNameToOffset[currentTileName]) * perlinNoiseScale, (y + randomYOffset + tileNameToOffset[currentTileName]) * perlinNoiseScale);
                    rng = Mathf.Clamp01(rng);

                    //check if succeeded rng
                    if(chance >= rng)
                    {
                        tilemap.SetTile(new Vector3Int(x, -y, 0), tileNameToTileBase[currentTileName]);
                    }
                }
            }
        }

    }

    public void SetBreakSprite(Vector3Int position, float percentage)
    {
        if(percentage > 0 && percentage < 0.25)
        {
            breakTileMap.SetTile(position, breakTiles[0]);
        }
        else if(percentage >= 0.25 && percentage < 0.5)
        {
            breakTileMap.SetTile(position, breakTiles[1]);
        }
        else if (percentage > 0.5 && percentage < 0.75)
        {
            breakTileMap.SetTile(position, breakTiles[2]);
        }
        else if (percentage > 0.75 && percentage < 1)
        {
            breakTileMap.SetTile(position, breakTiles[3]);
        }
    }

    public void DeleteBreakSprite(Vector3Int position)
    {
        breakTileMap.SetTile(position, null);
    }

    public void SetLiveData()
    {
        //loop though grid
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int currentPos = new Vector3Int(x, -y, 0);
                TileBase typeOfTile = tilemap.GetTile(currentPos);

                //get break time
                if(typeOfTile == null)
                {
                    continue;
                }
                TileData thisTileData = tileToData[typeOfTile];
                float maxBreakTime = thisTileData.maxBreakTime;

                liveTileData.Add(currentPos, new LiveTileData(maxBreakTime));
            }
        }
    }

    private void SetTileHeightData()
    {
        for(int i = 0; i < tileChancesSO.Length; i++)
        {
            tileChances.Add(tileChancesSO[i].Height, tileChancesSO[i]);
        }
    }
    public float ChanceToFloat(TILE_CHANCE chance)
    {
        switch(chance)
        {
            case TILE_CHANCE.VERY_LOW:
                return veryLow;
            case TILE_CHANCE.LOW:
                return low;
            case TILE_CHANCE.MEDIUM:
                return medium;
            case TILE_CHANCE.HIGH:
                return high;
            case TILE_CHANCE.VERY_HIGH:
                return veryHigh;
            default:
                return 1f;
        }
    }
}


