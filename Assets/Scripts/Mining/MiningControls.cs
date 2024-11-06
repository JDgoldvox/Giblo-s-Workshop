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
    public bool isMiningButtonDown { get; private set; } = false;

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
        mineAction.canceled += Mine;

        moveAction = miningInputActions.Player.Move;
        moveAction.Enable();
        moveAction.performed += Move;
        moveAction.canceled += Move;
    }

    private void OnDisable()
    {
        mineAction.Disable();
        mineAction.performed -= Mine;
        mineAction.canceled -= Mine;

        moveAction.Disable();
        moveAction.performed -= Move;
        moveAction.canceled -= Move;
    }

    void Update()
    {
        UpdateMouseCellPosition();
    }

    private void Mine(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isMiningButtonDown = context.ReadValueAsButton();
            MiningActions.Instance.MineTile();

        }
        else if (context.canceled)
        {
            isMiningButtonDown = false;
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
