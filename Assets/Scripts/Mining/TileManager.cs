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

        while (lastYInterval < height) {
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
        //0->199
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

        //200->399
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

        //400->599
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

        //600->799
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.SOFT_ROCK,
                TILE_NAME.COPPER,
                TILE_NAME.IRON,
                TILE_NAME.WATER
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.SOFT_ROCK, 1f },
            { TILE_NAME.COPPER, high },
            { TILE_NAME.IRON, veryLow },
            { TILE_NAME.WATER, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(600, tilesThatExistHere);
        }

        //800->999
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.SOFT_ROCK,
                TILE_NAME.COPPER,
                TILE_NAME.IRON,
                TILE_NAME.WATER
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.SOFT_ROCK, 1f },
            { TILE_NAME.COPPER, veryHigh },
            { TILE_NAME.IRON, low },
            { TILE_NAME.WATER, medium },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(800, tilesThatExistHere);
        }

        //1000->1199
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.SOFT_ROCK,
                TILE_NAME.COPPER,
                TILE_NAME.IRON,
                TILE_NAME.GOLD,
                TILE_NAME.WATER,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.MEDIUM_ROCK, 1f},
            { TILE_NAME.SOFT_ROCK, veryHigh },
            { TILE_NAME.COPPER, medium },
            { TILE_NAME.IRON, medium },
            { TILE_NAME.GOLD, veryLow },
            { TILE_NAME.WATER, medium },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(1000, tilesThatExistHere);
        }

        //1200->1399
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.SOFT_ROCK,
                TILE_NAME.COPPER,
                TILE_NAME.IRON,
                TILE_NAME.GOLD,
                TILE_NAME.WATER,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.MEDIUM_ROCK, 1f},
            { TILE_NAME.SOFT_ROCK, veryHigh },
            { TILE_NAME.COPPER, low },
            { TILE_NAME.IRON, medium },
            { TILE_NAME.GOLD, veryLow },
            { TILE_NAME.WATER, veryHigh },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(1200, tilesThatExistHere);
        }

        //1400->1599
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.COPPER,
                TILE_NAME.IRON,
                TILE_NAME.GOLD
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.MEDIUM_ROCK, 1f},
            { TILE_NAME.COPPER, veryLow },
            { TILE_NAME.IRON, medium },
            { TILE_NAME.GOLD, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(1400, tilesThatExistHere);
        }

        //1600->1799
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.IRON,
                TILE_NAME.GOLD
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.MEDIUM_ROCK, 1f},
            { TILE_NAME.IRON, high },
            { TILE_NAME.GOLD, low },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(1600, tilesThatExistHere);
        }

        //1800->1999
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.IRON,
                TILE_NAME.GOLD
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.MEDIUM_ROCK, 1f},
            { TILE_NAME.IRON, veryHigh },
            { TILE_NAME.GOLD, low },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(1800, tilesThatExistHere);
        }

        //2000->2199
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.HARD_ROCK,
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.IRON,
                TILE_NAME.GOLD
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.HARD_ROCK, 1f},
            { TILE_NAME.MEDIUM_ROCK, veryHigh},
            { TILE_NAME.IRON, low },
            { TILE_NAME.GOLD, medium },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(2000, tilesThatExistHere);
        }

        //2200->2399
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.HARD_ROCK,
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.IRON,
                TILE_NAME.GOLD
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.HARD_ROCK, 1f},
            { TILE_NAME.MEDIUM_ROCK, high},
            { TILE_NAME.IRON, low },
            { TILE_NAME.GOLD, medium },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(2200, tilesThatExistHere);
        }

        //2400->2599
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.HARD_ROCK,
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.IRON,
                TILE_NAME.GOLD
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.HARD_ROCK, 1f},
            { TILE_NAME.MEDIUM_ROCK, high},
            { TILE_NAME.IRON, low },
            { TILE_NAME.GOLD, medium },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(2400, tilesThatExistHere);
        }

        //2600->2799
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.HARD_ROCK,
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.IRON,
                TILE_NAME.GOLD
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.HARD_ROCK, 1f},
            { TILE_NAME.MEDIUM_ROCK, high},
            { TILE_NAME.IRON, low },
            { TILE_NAME.GOLD, medium },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(2600, tilesThatExistHere);
        }

        //2800->2999
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.HARD_ROCK,
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.HARD_ROCK,
                TILE_NAME.IRON,
                TILE_NAME.GOLD,
                TILE_NAME.COLBOLT,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.HARD_ROCK, 1f},
            { TILE_NAME.MEDIUM_ROCK, veryLow},
            { TILE_NAME.EXTREMELY_HARD_ROCK, medium},
            { TILE_NAME.IRON, veryLow },
            { TILE_NAME.GOLD, medium },
            { TILE_NAME.COLBOLT, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(2800, tilesThatExistHere);
        }

        //3000->3199
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.HARD_ROCK,
                TILE_NAME.MEDIUM_ROCK,
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.IRON,
                TILE_NAME.GOLD,
                TILE_NAME.COLBOLT,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.HARD_ROCK, 1f},
            { TILE_NAME.MEDIUM_ROCK, veryLow},
            { TILE_NAME.EXTREMELY_HARD_ROCK, medium},
            { TILE_NAME.IRON, veryLow },
            { TILE_NAME.GOLD, medium },
            { TILE_NAME.COLBOLT, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(3000, tilesThatExistHere);
        }

        //3200->3399
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.HARD_ROCK,
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.IRON,
                TILE_NAME.GOLD,
                TILE_NAME.COLBOLT,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.HARD_ROCK, 1f},
            { TILE_NAME.EXTREMELY_HARD_ROCK, medium},
            { TILE_NAME.IRON, veryLow },
            { TILE_NAME.GOLD, medium },
            { TILE_NAME.COLBOLT, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(3200, tilesThatExistHere);
        }

        //3400->3599
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.HARD_ROCK,
                TILE_NAME.GOLD,
                TILE_NAME.COLBOLT,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.EXTREMELY_HARD_ROCK, 1f},
            { TILE_NAME.HARD_ROCK, veryHigh},
            { TILE_NAME.GOLD, high },
            { TILE_NAME.COLBOLT, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(3400, tilesThatExistHere);
        }

        //3600->3799
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.HARD_ROCK,
                TILE_NAME.GOLD,
                TILE_NAME.COLBOLT,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.EXTREMELY_HARD_ROCK, 1f},
            { TILE_NAME.HARD_ROCK, medium},
            { TILE_NAME.GOLD, high },
            { TILE_NAME.COLBOLT, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(3600, tilesThatExistHere);
        }

        //3800->3999
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.HARD_ROCK,
                TILE_NAME.GOLD,
                TILE_NAME.COLBOLT,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.EXTREMELY_HARD_ROCK, 1f},
            { TILE_NAME.HARD_ROCK, low},
            { TILE_NAME.GOLD, high },
            { TILE_NAME.COLBOLT, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(3800, tilesThatExistHere);
        }

        //4000->4199
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.HARD_ROCK,
                TILE_NAME.GOLD,
                TILE_NAME.COLBOLT,
                TILE_NAME.LAVA,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.EXTREMELY_HARD_ROCK, 1f},
            { TILE_NAME.HARD_ROCK, low},
            { TILE_NAME.GOLD, high },
            { TILE_NAME.COLBOLT, veryLow },
            { TILE_NAME.LAVA, veryLow },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(4000, tilesThatExistHere);
        }

        //4200->4399
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.HARD_ROCK,
                TILE_NAME.GOLD,
                TILE_NAME.COLBOLT,
                TILE_NAME.LAVA,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.EXTREMELY_HARD_ROCK, 1f},
            { TILE_NAME.HARD_ROCK, low},
            { TILE_NAME.GOLD, high },
            { TILE_NAME.COLBOLT, low },
            { TILE_NAME.LAVA, low },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(4200, tilesThatExistHere);
        }

        //4400->4599
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.GOLD,
                TILE_NAME.COLBOLT,
                TILE_NAME.LAVA,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.EXTREMELY_HARD_ROCK, 1f},
            { TILE_NAME.GOLD, high },
            { TILE_NAME.COLBOLT, low },
            { TILE_NAME.LAVA, low },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(4400, tilesThatExistHere);
        }

        //4600->4799
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.COLBOLT,
                TILE_NAME.LAVA,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.EXTREMELY_HARD_ROCK, 1f},
            { TILE_NAME.COLBOLT, medium },
            { TILE_NAME.LAVA, medium },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(4600, tilesThatExistHere);
        }

        //4800->4999
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.COLBOLT,
                TILE_NAME.LAVA,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.EXTREMELY_HARD_ROCK, 1f},
            { TILE_NAME.COLBOLT, medium },
            { TILE_NAME.LAVA, medium },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(4800, tilesThatExistHere);
        }

        //5000->5199
        {
            List<TILE_NAME> tilesOrder = new List<TILE_NAME>
            {
                TILE_NAME.EXTREMELY_HARD_ROCK,
                TILE_NAME.COLBOLT,
                TILE_NAME.LAVA,
            };

            Dictionary<TILE_NAME, float> tileOffsets = new Dictionary<TILE_NAME, float>
            {
            { TILE_NAME.EXTREMELY_HARD_ROCK, 1f},
            { TILE_NAME.COLBOLT, high },
            { TILE_NAME.LAVA, high },
            };

            TilesThatExistHere tilesThatExistHere = new TilesThatExistHere(tilesOrder, tileOffsets);
            TilesThatExistOnThisHeight.Add(5000, tilesThatExistHere);
        }
    }

}
