using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class MiningControls : MonoBehaviour
{

    public static MiningControls Instance;

    private MiningInputActions miningInputActions;
    private InputAction mineAction;
    private InputAction moveAction;

    public Vector2 moveDirection { get; private set; } 
    public Vector3Int mouseCellPosition { get; private set; } 
    public bool isMouseDown { get; private set; } = false;

    private void Awake()
    {
        miningInputActions = new MiningInputActions();

        if(Instance == null)
        {
            Instance = this;
        }

    }

    private void OnEnable()
    {
        mineAction = miningInputActions.Player.Mine;
        mineAction.Enable();
        mineAction.performed += Mine;

        moveAction = miningInputActions.Player.Move;
        moveAction.Enable();
        moveAction.performed += Move;
        moveAction.canceled += Move;
    }

    private void OnDisable()
    {
        mineAction.Disable();
        mineAction.performed -= Mine;

        moveAction.Disable();
        moveAction.performed -= Move;
        moveAction.canceled -= Move;
    }

    void Update()
    {
        UpdateMouseCellPosition();
        Debug.Log("is mouse down: " + isMouseDown);
    }

    private void Mine(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Tilemap map = TileManager.instance.tilemap;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int worldToCell = map.WorldToCell(mousePos);
            TileBase clickedTile = map.GetTile(worldToCell);

            //return if empty clicked on tile
            if (clickedTile == null)
            {
                //Debug.Log(worldToCell + " CLICKED ON: " + "E-M-P-T-Y");
                return;
            }

            //Debug.Log(worldToCell + " CLICKED ON: " + clickedTile.name);

            //activate mining function
            MiningActions.Instance.MineTile(worldToCell);
        }
    }

    private void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveDirection = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            moveDirection = Vector2.zero;
        }
    }

    private void UpdateMouseCellPosition()
    {
        Tilemap map = TileManager.instance.tilemap;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseCellPosition = map.WorldToCell(mousePos);
    }
}
