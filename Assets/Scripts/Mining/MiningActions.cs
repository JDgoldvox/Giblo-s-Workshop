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
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        rb2D.linearVelocity += walkMultiplier * MiningControls.Instance.moveDirection * Time.deltaTime;
    }

    public void MineTile(Vector3Int cellPosition)
    {
        //start coroutine
        StartCoroutine(WaitToMine(cellPosition));
    }

    private IEnumerator WaitToMine(Vector3Int cellPosition)
    {
        float timer = 0;

        //whilst left mouse button still held down
        //and mouse hasn't moved from original starting cell position
        while (Mouse.current.leftButton.IsPressed() && MiningControls.Instance.mouseCellPosition == cellPosition)
        {
            Debug.Log("mining...");
            timer += Time.deltaTime;
            if (timer > timeToMineABlock)
            {
                Tilemap map = TileManager.instance.tilemap;
                map.SetTile(cellPosition, null);
            }

            yield return null;
        }
        Debug.Log("mining stopped! , CELL POS CHANGED FROM: " + cellPosition + " TO: " + MiningControls.Instance.mouseCellPosition);

        //nothing happened
        yield return null;
    }
}
