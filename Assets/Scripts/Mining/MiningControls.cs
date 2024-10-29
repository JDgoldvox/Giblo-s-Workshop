using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class MiningControls : MonoBehaviour
{

    MiningInputActions miningInputActions;
    InputAction fireAction;

    private void Awake()
    {
        miningInputActions = new MiningInputActions();

    }

    private void OnEnable()
    {
        fireAction = miningInputActions.Player.Fire;
        fireAction.Enable();
        fireAction.performed += Fire;
    }

    private void OnDisable()
    {
        fireAction.Disable();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void Fire(InputAction.CallbackContext context)
    {
        Tilemap map = TileManager.instance.tilemap;

        Debug.Log("fired");
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int worldToCell = map.WorldToCell(mousePos);

        TileBase clickedTile = map.GetTile(worldToCell);

        if (clickedTile != null)
        {
            Debug.Log(worldToCell + " CLICKED ON: " + clickedTile.name);

            if (TileManager.instance.tileToData.TryGetValue(clickedTile, out TileData data))
            {
                Debug.Log("IS ORE: " + data.isOre);
            }
            else
            {
                Debug.Log("Cell not recognised");
            }
        }
        else
        {
            Debug.Log(worldToCell + " CLICKED ON: " + "E-M-P-T-Y");
        }
    }
}
