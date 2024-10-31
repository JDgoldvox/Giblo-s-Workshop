using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;
    public Tilemap tilemap;
    [SerializeField] private List<TileData> tileDatas = new List<TileData>();
    public Dictionary<TileBase, TileData> tileToData = new Dictionary<TileBase, TileData>();

    public TileBase tile;
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
        }
    }

    void Start()
    {
        SpawnTiles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnTiles()
    {
        tilemap.SetTile(new Vector3Int(0,0,0), tile);
        tilemap.SetTile(new Vector3Int(1, 0, 0), tile);
        tilemap.SetTile(new Vector3Int(0, 1, 0), tile);
    }
}
