using UnityEngine;
using System.Collections.Generic;

public class CrateManager : MonoBehaviour
{
    public List<CrateData> allCrates; 

    
    public CrateData GetCrateByID(string crateID)
    {
        foreach (CrateData crateData in allCrates)
        {
            if (crateData.ID == crateID)
            {
                return crateData;
            }
        }
        Debug.LogError("Crate not found: " + crateID);
        return null;
    }

    
    public ItemData GetItemByID(string itemID)
    {
        foreach (CrateData crateData in allCrates)
        {
            foreach (ItemData item in crateData.Items)
            {
                if (item.ID == itemID)
                {
                    return item;
                }
            }
        }
        Debug.LogError("Item not found: " + itemID);
        return null;
    }
}
