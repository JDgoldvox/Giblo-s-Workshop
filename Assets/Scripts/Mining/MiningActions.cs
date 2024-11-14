using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class MiningActions : MonoBehaviour
{
    public static MiningActions Instance;

    [SerializeField] private TileManager S_TileManager;
    private MiningControls S_MiningControls;
    private MiningInventory S_MiningInventory;

    [SerializeField] private float walkMultiplier = 69;
    [SerializeField] private float jumpForce;
    private float miningDistance = 3f;

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
        Vector3Int tileTargettedPos = S_MiningControls.mouseCellPosition;
        Tilemap map = S_TileManager.tilemap;
        LiveTileData liveTileData = null;

        //whilst left mouse button still held down
        while (S_MiningControls.isMiningButtonDown)
        {
            //check if valid tile position
            if (liveTileData == null)
            {
                //find current pos of mouse to see if we get a new tile
                tileTargettedPos = S_MiningControls.mouseCellPosition;

                if (CheckIfValidTile(tileTargettedPos))
                {
                    liveTileData = S_TileManager.liveTileData[tileTargettedPos];
                }
                else
                {
                    yield return null;
                    continue;
                }
            }

            //check if our mouse moved
            if (tileTargettedPos != S_MiningControls.mouseCellPosition)
            {
                //mouse position changed

                //Set fade break sprite
                StartCoroutine(UntouchedTileTimer(tileTargettedPos));

                //set details
                tileTargettedPos = S_MiningControls.mouseCellPosition;
                liveTileData = null;
                continue;
            }

            //BELOW THIS IS UPDATING STUFF, EVERYTHING IS IN ORDER

            //timer
            liveTileData.currentBreakTime += Time.deltaTime;

            //break sprite
            float percentageMined = (liveTileData.currentBreakTime / liveTileData.maxBreakTime);
            S_TileManager.SetBreakSprite(tileTargettedPos, percentageMined);
            liveTileData.TouchTile();

            //mine
            if (liveTileData.currentBreakTime >= liveTileData.maxBreakTime)
            {
                //break tile
                TileBase tileMined = map.GetTile(tileTargettedPos);
                AddMaterialToInventory(tileMined);
                map.SetTile(tileTargettedPos, null);

                //remove previous break sprite
                S_TileManager.DeleteBreakSprite(tileTargettedPos);

                //reset
                liveTileData.isAlive = false;
                liveTileData.currentBreakTime = 0;
                liveTileData = null;
                continue;
            }

            yield return null;
        }

        if (CheckIfValidTile(tileTargettedPos))
        {
            StartCoroutine(UntouchedTileTimer(tileTargettedPos));
        }
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
    }

    private bool CheckIfValidTile(Vector3Int tileTargettedPos)
    {
        //if does not exit in live data
        if(!S_TileManager.liveTileData.ContainsKey(tileTargettedPos))
        {
            return false;
        }
        else if (!S_TileManager.liveTileData[tileTargettedPos].isAlive)
        {
            return false;
        }

        Vector3Int playerPosition = S_MiningControls.tileMap.WorldToCell(transform.position);
        float targettedTileToPlayerDistance = Vector3Int.Distance(playerPosition, tileTargettedPos);

        if (targettedTileToPlayerDistance > miningDistance)//outside reach distance
        {
            return false;
        }

        return true;
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
