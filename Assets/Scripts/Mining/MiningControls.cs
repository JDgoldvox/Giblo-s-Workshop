using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections;


public class MiningControls : MonoBehaviour
{
    public static MiningControls Instance;

    private MiningInputActions miningInputActions;
    private InputAction mineAction;
    private InputAction moveAction;
    private InputAction jumpAction;

    public Vector2 moveDirection { get; private set; } 
    public Vector3Int mouseCellPosition { get; private set; } 
    public bool isMiningButtonDown { get; private set; } = false;
    public bool isJumping { get; private set; } = false;
    [SerializeField] CircleCollider2D floorCollider;
    public bool isTouchingFloor { get; private set; } = false;


    private float maxJumpTime = 0.2f;

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
    }

    void Update()
    {
        UpdateMouseCellPosition();
    }

    //CANT DO THIS, NEED TO BE ON ITS OWN SRIPT
    private void OnTriggerStay2D(Collider2D collision)
    {
        //if(collision.CompareTag("floor")
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

    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isJumping = true;
            StartCoroutine(JumpTimer());
        }
        else if (context.canceled)
        {
            isJumping = false;
        }
    }

    private IEnumerator JumpTimer()
    {
        float jumpTimer = maxJumpTime;

        while(jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
            yield return null;
        }

        isJumping = false;
        yield return null;
    }
}
