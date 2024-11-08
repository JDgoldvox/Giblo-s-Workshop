using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class MiningActions : MonoBehaviour
{
    public static MiningActions Instance;

    [SerializeField] private TileManager S_TileManager;
    private MiningControls S_MiningControls;
    private MiningInventory S_MiningInventory;

    [SerializeField] private float walkMultiplier = 69;
    [SerializeField] private float jumpForce;
    
    Rigidbody2D rb2D = null;
    float timeToMineABlock = 2;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        rb2D = GetComponent<Rigidbody2D>();
        S_MiningControls = GetComponent<MiningControls>();
        S_MiningInventory = GetComponent<MiningInventory>();
    }

    void FixedUpdate()
    {
        Movement();
        Jumping();
    }

    private void Movement()
    {
        rb2D.linearVelocity += walkMultiplier * S_MiningControls.moveDirection * Time.deltaTime;
    }

    public void MineTile()
    {
        StartCoroutine(WaitToMine());
    }

    private IEnumerator WaitToMine()
    {
        float timer = 0;
        Vector3Int blockTargetted = S_MiningControls.mouseCellPosition;
        Tilemap map = S_TileManager.tilemap;

        //whilst left mouse button still held down
        //and mouse hasn't moved from original starting cell position
        while (S_MiningControls.isMiningButtonDown)
        {
            timer += Time.deltaTime;

            //block continuing until valid tile is hovered over
            if (!map.GetTile(blockTargetted))
            {
                blockTargetted = S_MiningControls.mouseCellPosition;
                timer = 0;
                yield return null;
                continue;
            }

            //check if we moved our mouse
            if (S_MiningControls.mouseCellPosition != blockTargetted)
            {
                timer = 0;
                blockTargetted = S_MiningControls.mouseCellPosition;
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
        TileData data = S_TileManager.tileToData[tileBase];
        Debug.Log("found " +  data.tileName.ToString());

        S_MiningInventory.AddItem(data.tileName, 1);

    }

    private void Jumping()
    {
        if (S_MiningControls.isJumping && Floor)
        {
            rb2D.AddForceY(jumpForce, ForceMode2D.Impulse);
        }
    }
}
