using System.Collections.Generic;
using UnityEngine;

public class MiningInventory : MonoBehaviour
{
    public Dictionary<TILE_NAME, int> inventory = new Dictionary<TILE_NAME, int>();

    public void AddItem(TILE_NAME item, int amount)
    {
        if(inventory.TryGetValue(item, out int inv))
        {
            inv += amount;
        }
        else
        {
            inventory.Add(item, amount);
        }
    }
}
