using UnityEngine;

public class GibloMiningAnimations : MonoBehaviour
{
    private MiningControls S_MiningControls;
    private float initialScaleX = 1123f;

    private void Awake()
    {
        S_MiningControls = GetComponent<MiningControls>();
    }

    private void Start()
    {
        initialScaleX = transform.localScale.x;
        Debug.Log("scale: " + initialScaleX);
    }


    public void Update()
    {
        if (S_MiningControls.isFacingRight && transform.localScale.x < 0)
        {
            Debug.Log("facing right, changing scale");
            transform.localScale = new Vector3(initialScaleX, transform.localScale.y, transform.localScale.z);
        }
        else if(!S_MiningControls.isFacingRight && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-initialScaleX, transform.localScale.y, transform.localScale.z);
        }
    }
}
