using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }
    private const string SaveFileName = "InventoryData.dat";
    private List<SerializableItemData> inventoryItems = new List<SerializableItemData>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInventory(); // Load inventory on start
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Add item to inventory
    public void AddItemToInventory(ItemData item)
    {
        SerializableItemData serializableItem = new SerializableItemData(item);
        inventoryItems.Add(serializableItem);
        SaveInventory();
    }

    // Calculate total value of inventory
    public float CalculateInventoryValue()
    {
        float totalValue = 0;
        foreach (SerializableItemData item in inventoryItems)
        {
            totalValue += item.Price;
        }
        return totalValue;
    }

    public void ClearInventory()
    {
        inventoryItems.Clear();
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        Debug.Log("InventoryData path: " + path);
        
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Inventory file deleted at " + path);
        }
        
        SaveInventory();
        Debug.Log("Inventory cleared.");
    }

    public void SaveInventory()
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        using (FileStream file = File.Create(path))
        {
            bf.Serialize(file, inventoryItems);
        }
        Debug.Log("Inventory saved successfully at " + path);
    }

    public void LoadInventory()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(path, FileMode.Open))
            {
                inventoryItems = (List<SerializableItemData>)bf.Deserialize(file);
            }
            Debug.Log("Inventory loaded successfully from " + path);
        }
        else
        {
            Debug.LogWarning("No inventory file found. Starting with an empty inventory.");
        }
    }

    public List<SerializableItemData> GetInventoryItems()
    {
        return inventoryItems;
    }

}

[System.Serializable]
public class SerializableItemData
{
    public string ID;
    public string Name;
    public string Rarity;
    public float Price;
    public int Weight;

    public SerializableItemData(ItemData itemData)
    {
        ID = itemData.ID;
        Name = itemData.Name;
        Price = itemData.Price;
        Rarity = itemData.Rarity;
        Weight = itemData.Weight;
    }
}