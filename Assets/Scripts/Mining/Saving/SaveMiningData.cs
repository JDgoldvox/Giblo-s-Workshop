using System.IO;
using UnityEngine;

public class SaveMiningData : MonoBehaviour
{
    MiningInventory inventory;

    private void Awake()
    {
        inventory = GetComponent<MiningInventory>();
    }

    public void SaveInventory()
    {
        string savePath = Application.persistentDataPath + "/miningInventory.save";

        File.WriteAllText(savePath, JsonUtility.ToJson(inventory, true));
    }
}
