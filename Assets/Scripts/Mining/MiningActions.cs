using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class MiningActions : MonoBehaviour
{
    public static MiningActions Instance;

    [SerializeField] private float walkMultiplier = 69;

    Rigidbody2D rb2D = null;
    float timeToMineABlock = 2;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        rb2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        rb2D.linearVelocity += walkMultiplier * MiningControls.Instance.moveDirection * Time.deltaTime;
    }

    public void MineTile()
    {
        StartCoroutine(WaitToMine());
    }

    private IEnumerator WaitToMine()
    {
        float timer = 0;
        Vector3Int blockTargetted = MiningControls.Instance.mouseCellPosition;
        Tilemap map = TileManager.instance.tilemap;

        //whilst left mouse button still held down
        //and mouse hasn't moved from original starting cell position
        while (MiningControls.Instance.isMiningButtonDown)
        {
            timer += Time.deltaTime;

            //block continuing until valid tile is hovered over
            if (!map.GetTile(blockTargetted))
            {
                blockTargetted = MiningControls.Instance.mouseCellPosition;
                timer = 0;
                yield return null;
                continue;
            }

            //check if we moved our mouse
            if (MiningControls.Instance.mouseCellPosition != blockTargetted)
            {
                timer = 0;
                blockTargetted = MiningControls.Instance.mouseCellPosition;
            }

            if (timer > timeToMineABlock)
            {
                AddMaterialToInventory(map.GetTile(blockTargetted));
                map.SetTile(blockTargetted, null);
            }

            yield return null;
        }

        //nothing happened
        yield return null;
    }

    private void AddMaterialToInventory(TileBase tileBase)
    {
        TileData data = TileManager.instance.tileToData[tileBase];
        Debug.Log("found " +  data.tileName.ToString());
    }
}
