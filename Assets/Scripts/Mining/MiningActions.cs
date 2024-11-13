using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class MiningActions : MonoBehaviour
{
    public static MiningActions Instance;

    [SerializeField] private TileManager S_TileManager;
    private MiningControls S_MiningControls;
    private MiningInventory S_MiningInventory;

    [SerializeField] private float walkMultiplier = 69;
    [SerializeField] private float jumpForce;
    
    Rigidbody2D rb2D = null;

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
        Vector3Int blockTargettedPos = S_MiningControls.mouseCellPosition;
        Tilemap map = S_TileManager.tilemap;

        //get max break time
        float timeToMineABlock = 100;
        if (S_TileManager.liveTileData.TryGetValue(blockTargettedPos, out LiveTileData liveTileData))
        {
            timeToMineABlock = liveTileData.maxBreakTime;
            timer = liveTileData.currentBreakTime;
        }

        //whilst left mouse button still held down
        while (S_MiningControls.isMiningButtonDown)
        {
            //block continuing until valid tile is hovered over
            if (!map.GetTile(blockTargettedPos))
            {
                timer = liveTileData.currentBreakTime;
                blockTargettedPos = S_MiningControls.mouseCellPosition;
                timeToMineABlock = S_TileManager.liveTileData[blockTargettedPos].maxBreakTime; //change new block mining time
                yield return null;
            }

            //update current break time
            timer += Time.deltaTime;
            liveTileData.currentBreakTime = timer;

            //check if we moved our mouse
            if (S_MiningControls.mouseCellPosition != blockTargettedPos)
            {
                timer = liveTileData.currentBreakTime;
                Debug.Log("remoing break sprite");
                //Start a timer to see whether the tile was touched again, if not 3 seconds will pass and live data will reset
                StartCoroutine(UntouchedTileTimer(blockTargettedPos));

                blockTargettedPos = S_MiningControls.mouseCellPosition;
            }

            // update break sprite
            S_TileManager.SetBreakSprite(blockTargettedPos, (timer / timeToMineABlock) * 100);
            S_TileManager.liveTileData[blockTargettedPos].TouchTile();

            //if we break block
            if (timer > timeToMineABlock)
            {
                AddMaterialToInventory(map.GetTile(blockTargettedPos));
                map.SetTile(blockTargettedPos, null);

                //remove previous break sprite
                S_TileManager.DeleteBreakSprite(blockTargettedPos);

            }

            yield return null;
        }

        StartCoroutine(UntouchedTileTimer(blockTargettedPos));

        //nothing happened
        yield return null;
    }

    IEnumerator UntouchedTileTimer(Vector3Int tileTargettedPos)
    {
        float threeSecondTimer = 0;
        LiveTileData ltd = S_TileManager.liveTileData[tileTargettedPos];

        //if not touched again, break loop
        ltd.UntouchTile();

        while (!ltd.isTouched)
        {
            threeSecondTimer += Time.deltaTime;

            //three second threshold passed, remove all break sprites and reset ltd
            if (threeSecondTimer > 3)
            {
                ltd.ResetLiveData();
                S_TileManager.DeleteBreakSprite(tileTargettedPos);
                yield break;
            }

            yield return null;
        }

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
