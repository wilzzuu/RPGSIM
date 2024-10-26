using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance; 

    public InventoryData inventory = new InventoryData();
    private string savePath;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            savePath = Application.persistentDataPath + "/playerInventory.dat";
            LoadInventory(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    // Save the inventory data to a binary file
    public void SaveInventory()
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(savePath))
        {
            bf.Serialize(file, inventory);
        }
        Debug.Log("Inventory saved successfully.");
    }

    // Load the inventory data from the binary file
    public void LoadInventory()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(savePath, FileMode.Open))
            {
                inventory = (InventoryData)bf.Deserialize(file);
            }
            Debug.Log("Inventory loaded successfully.");
        }
        else
        {
            Debug.LogWarning("No inventory file found, creating a new inventory.");
            inventory = new InventoryData();
        }
    }

    // Method to add an item to the inventory
    public void AddItemToInventory(ItemData item)
    {
        inventory.items.Add(item);
        SaveInventory();  // Save after adding
    }

    // Method to clear the inventory (useful for debugging)
    public void ClearInventory()
    {
        inventory.items.Clear();
        SaveInventory();
        Debug.Log("Inventory cleared.");
    }

}


[Serializable]
public class InventoryData
{
    public List<ItemData> items = new List<ItemData>();
}