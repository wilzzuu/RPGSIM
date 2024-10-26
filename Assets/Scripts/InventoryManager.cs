using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance; 

    private List<ItemData> playerInventory = new List<ItemData>();
    private string savePath;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            savePath = Application.persistentDataPath + "/inventory.json";
            LoadInventory(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    public void AddItemToInventory(ItemData item)
    {
        playerInventory.Add(item);
        Debug.Log(item.Name + " added to inventory.");
        SaveInventory(); 
    }

    
    public void RemoveItemFromInventory(ItemData item)
    {
        if (playerInventory.Contains(item))
        {
            playerInventory.Remove(item);
            SaveInventory(); 
            Debug.Log(item.Name + " removed from inventory.");
        }
        else
        {
            Debug.LogError("Item not found in inventory.");
        }
    }

    
    private void SaveInventory()
    {
        string json = JsonUtility.ToJson(new ItemListWrapper { items = playerInventory }, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Inventory saved.");
    }

    
    private void LoadInventory()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ItemListWrapper loadedData = JsonUtility.FromJson<ItemListWrapper>(json);
            playerInventory = loadedData.items ?? new List<ItemData>();
            Debug.Log("Inventory loaded.");
        }
        else
        {
            Debug.Log("No inventory save file found.");
        }
    }

    public List<ItemData> GetPlayerInventory()
    {
        return playerInventory;
    }
}


[System.Serializable]
public class ItemListWrapper
{
    public List<ItemData> items;
}
