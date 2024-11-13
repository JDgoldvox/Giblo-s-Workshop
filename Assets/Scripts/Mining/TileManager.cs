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

    private int width = 20;
    private int height = 1000;

    private float randomXOffset = 0;
    private float randomYOffset = 0;

    private float veryLow = 0.1f;
    private float low = 0.15f;
    private float medium = 0.2f;

    class TilesThatExistHere
    {
        public TilesThatExistHere(List<TILE_NAME> listIn, Dictionary<TILE_NAME, float> DicIn)
        {
            tileNamesInOrder = listIn;
            nameToChance = DicIn;
        }
        public List<TILE_NAME> tileNamesInOrder;
        public Dictionary<TILE_NAME, float> nameToChance;
    }

    //height to dictionary of chances
    private Dictionary<int, TilesThatExistHere> TilesThatExistOnThisHeight = new Dictionary<int, TilesThatExistHere>();

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

        while (lastYInterval < 600) {
            List<TILE_NAME> sectionListOfTilesInOrder = TilesThatExistOnThisHeight[lastYInterval].tileNamesInOrder;
                LoopAndPlaceTilesInSection(lastYInterval, lastYInterval + sectionInterval, sectionListOfTilesInOrder);
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

    private void LoopAndPlaceTilesInSection(int currentHeight, int maxHeight, List<TILE_NAME> tilesThatExistHere)
    {
        int numberOfTilesThatExistHere = tilesThatExistHere.Count();
        float chance = 1;

        TILE_NAME currentTileName;
        for (int typeCount = 0; typeCount < numberOfTilesThatExistHere; typeCount++)
        {
            currentTileName = tilesThatExistHere[typeCount];

            //SET ALL STONE FIRST (ALWAYS 0)
            if(typeCount == 0)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = currentHeight; y < maxHeight; y++)
                    {
                        tilemap.SetTile(new Vector3Int(x, -y, 0), tileNameToTileBase[currentTileName]);
                    }
                }

                continue;
            }

            chance = TilesThatExistOnThisHeight[currentHeight].nameToChance[currentTileName];

            //after base placed down
            for (int x = 0; x < width; x++)
            {
                for (int y = currentHeight; y < maxHeight; y++)
                {
                    float rng = Mathf.PerlinNoise((x + randomXOffset + tileNameToOffset[currentTileName]) * 0.4f, (y + randomYOffset + tileNameToOffset[currentTileName]) * 0.4f);
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

    private void SetTileHeightData()
    {
        //0
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.SOFT_ROCK,
                TILE_NAME.COPPER
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.SOFT_ROCK, 1f },
            { TILE_NAME.COPPER, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(0, tilesThatExistHere);
        }

        //200
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.SOFT_ROCK,
                TILE_NAME.COPPER
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.SOFT_ROCK, 1f },
            { TILE_NAME.COPPER, low },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(200, tilesThatExistHere);
        }

        //400
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.SOFT_ROCK,
                TILE_NAME.COPPER,
                TILE_NAME.IRON
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.SOFT_ROCK, 1f },
            { TILE_NAME.COPPER, medium },
            { TILE_NAME.IRON, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(400, tilesThatExistHere);
        }
    }

    public void SetBreakSprite(Vector3Int position, float percentage)
    {
        if (!liveTileData.ContainsKey(position))
        {
            Debug.Log("Does not contain " + position); 
            return;
        }

        if(percentage > 0 && percentage < 25)
        {
            breakTileMap.SetTile(position, breakTiles[0]);
        }
        else if(percentage >= 25 && percentage < 50)
        {
            breakTileMap.SetTile(position, breakTiles[1]);
        }
        else if (percentage > 50 && percentage < 75)
        {
            breakTileMap.SetTile(position, breakTiles[2]);
        }
        else if (percentage > 75 && percentage < 100)
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

}
