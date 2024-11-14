using System.Diagnostics.Contracts;
using UnityEngine;

public class LiveTileData
{
    public float maxBreakTime = 0;
    public float currentBreakTime = 0;
    public bool isAlive = true;

    public bool isTouched { get; private set; } = false;
    private float maxUntouchedTime = 3;
    public LiveTileData(float breakTimeIn)
    {
        maxBreakTime = breakTimeIn;
    }

    public void ResetLiveData()
    {
        currentBreakTime = 0;
    }

    public void TouchTile()
    {
        isTouched = true;
    }

    public void UntouchTile()
    {
        isTouched = false;
    }


}
