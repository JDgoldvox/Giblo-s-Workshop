using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections;


public class MiningControls : MonoBehaviour
{
    public static MiningControls Instance;

    [SerializeField] public Tilemap tileMap;

    private MiningInputActions miningInputActions;
    private InputAction mineAction;
    private InputAction moveAction;
    private InputAction jumpAction;

    public Vector2 moveDirection { get; private set; } 
    public Vector3Int mouseCellPosition { get; private set; } 
    public bool isMiningButtonDown { get; private set; } = false;
    public bool isJumpHeld { get; private set; } = false;
    public bool isTouchingFloor { get; private set; } = false;
    public bool isFacingRight { get; private set; } = true;
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

        jumpAction = miningInputActions.Player.Jump;
        jumpAction.Enable();
        jumpAction.performed += Jump;
        jumpAction.canceled += Jump;

    }

    private void OnDisable()
    {
        mineAction.Disable();
        mineAction.performed -= Mine;
        mineAction.canceled -= Mine;

        moveAction.Disable();
        moveAction.performed -= Move;
        moveAction.canceled -= Move;

        jumpAction.Disable();
        jumpAction.performed -= Jump;
        jumpAction.canceled -= Jump;
    }

    void Update()
    {
        UpdateMouseCellPosition();
    }

    private void FixedUpdate()
    {
        IsTouchingFloor();
    }

    private void IsTouchingFloor()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(0,-1), 1f, LayerMask.GetMask("TileMap"));
        //Debug.DrawRay(transform.position, new Vector2(0, -1) * 1f, Color.white);
        isTouchingFloor = (hit) ? true : false;
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

            if(moveDirection == Vector2.right)
            {
                isFacingRight = true;
                Debug.Log("Facing right");
            }
            else
            {
                isFacingRight = false;
                Debug.Log("Facing left");
            }
        }
        else if (context.canceled)
        {
            moveDirection = Vector2.zero;
        }
    }

    private void UpdateMouseCellPosition()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseCellPosition = new Vector3Int(tileMap.WorldToCell(mousePos).x, tileMap.WorldToCell(mousePos).y, 0);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isJumpHeld = true;
            MiningActions.Instance.StartJump();
        }
        else if (context.canceled)
        {
            isJumpHeld = false;
        }
    }

}
