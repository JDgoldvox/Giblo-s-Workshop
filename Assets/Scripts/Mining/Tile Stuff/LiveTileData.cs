using UnityEngine;

public class LiveTileData
{
    public float maxBreakTime = 0;
    public float currentBreakTime = 0;
    public bool isTouched { get; private set; } = false;
    private float maxUntouchedTime = 3;
    private float touchTimer = 0;
    public LiveTileData(float breakTimeIn)
    {
        maxBreakTime = breakTimeIn;
    }

    private void qweqwe()
    {
        //reset touched
        if (isTouched)
        {
            Debug.Log("time... to get untouched");
            touchTimer += Time.deltaTime;
            if(touchTimer > maxUntouchedTime)
            {
                Debug.Log("touch reset");
                isTouched = false;
                ResetLiveData();
            }
        }
    }

    public void ResetLiveData()
    {
        currentBreakTime = 0;
    }

    public void TouchTile()
    {
        isTouched = true;
        touchTimer = 0; //reset touch timer
    }

    public void UntouchTile()
    {
        isTouched = false;
    }


}
