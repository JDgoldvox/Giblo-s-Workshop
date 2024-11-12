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

    private int jumpsRemaining = 0;
    private int maxNumberOfJumps = 2;
    private float maxJumpTime = 0.1f;
    [HideInInspector] public bool canJump = false;
    private float maxIgnoreFloorResetTime = 0.1f;
    private bool ignoreFloorReset = false;

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
        UpdateJump();
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

            S_TileManager.SetBreakSprite(blockTargetted, (timer / timeToMineABlock) * 100);

            //block continuing until valid tile is hovered over
            if (!map.GetTile(blockTargetted))
            {
                //remove previous break sprite
                S_TileManager.SetBreakSprite(blockTargetted, 0);

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

                //remove previous break sprite
                S_TileManager.SetBreakSprite(blockTargetted, 0);
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

    public void StartJump()
    {
        canJump = true;
        ignoreFloorReset = true;
        StartCoroutine(IgnoreFloorResetTimer());
    }

    private void UpdateJump()
    {
        //reset
        if (S_MiningControls.isTouchingFloor && !ignoreFloorReset)
        {
            Debug.Log("Resetting jump");
            jumpsRemaining = maxNumberOfJumps;
        }

        //jump
        if (S_MiningControls.isJumpHeld && canJump && jumpsRemaining > 0)
        {
            //do the jump
            StartCoroutine(JumpTimer());
            jumpsRemaining--;
        }
    }

    private IEnumerator JumpTimer()
    {
        float jumpTimer = maxJumpTime;

        //whilst we still allowed to jump or stopped holding jump
        while (jumpTimer > 0 && S_MiningControls.isJumpHeld)
        {
            rb2D.AddForceY(jumpForce * Time.deltaTime, ForceMode2D.Impulse);
            jumpTimer -= Time.deltaTime;
            canJump = false;
            yield return null;
        }

        yield return null;
    }

    private IEnumerator IgnoreFloorResetTimer()
    {
        float ignoreFloorResetTimer = maxIgnoreFloorResetTime;

        //whilst we still allowed to jump or stopped holding jump
        while (ignoreFloorResetTimer > 0)
        {
            ignoreFloorResetTimer -= Time.deltaTime;
            yield return null;
        }
        ignoreFloorReset = false;

        yield return null;
    }
}
