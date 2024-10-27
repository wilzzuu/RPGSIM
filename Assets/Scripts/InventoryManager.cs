using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance { get; private set; }
    private List<ItemData> inventoryItems = new List<ItemData>();
    private const string SaveFileName = "GameData.dat";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Load inventory data
            List<SerializableItemData> loadedItems = DataSerializationManager.Instance.LoadGameData();
            inventoryItems = ConvertSerializableItemsToItemData(loadedItems);
            Debug.Log($"Loaded {inventoryItems.Count} items into inventory.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Converts SerializableItemData back to ItemData for runtime use
    private List<ItemData> ConvertSerializableItemsToItemData(List<SerializableItemData> serializableItems)
    {
        List<ItemData> items = new List<ItemData>();
        foreach (SerializableItemData sItem in serializableItems)
        {
            string itemPath = $"Items/{sItem.ID}";
            ItemData item = Resources.Load<ItemData>(itemPath);
            if (item != null)
            {
                items.Add(item);
                Debug.Log($"Successfully loaded {item.Name} with ID {sItem.ID} from {itemPath}");
            }
            else
            {
                Debug.LogWarning($"Failed to load ItemData at {itemPath}. Asset with ID {sItem.ID} may be missing or incorrectly named.");
            }
        }
        return items;
    }

    public bool HasItem(ItemData item)
    {
        return inventoryItems.Contains(item);
    }

    public void RemoveItemFromInventory(ItemData item)
    {
        if (HasItem(item))
        {
            inventoryItems.Remove(item);
            Debug.Log($"{item.Name} removed from inventory.");
            SaveInventory();
        }
        else
        {
            Debug.LogWarning($"{item.Name} not found in inventory.");
        }
    }

    public void AddItemToInventory(ItemData item)
    {
        inventoryItems.Add(item);
        SaveInventory();
    }

    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(inventoryItems); // Returns a copy of the list to prevent external modifications
    }

    public float CalculateInventoryValue()
    {
        float totalValue = 0;
        foreach (ItemData item in inventoryItems)
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

    // Save the inventory by converting items to serializable data
    private void SaveInventory()
    {
        List<SerializableItemData> serializableItems = new List<SerializableItemData>();
        foreach (var item in inventoryItems)
        {
            serializableItems.Add(new SerializableItemData(item));
        }
        DataSerializationManager.Instance.SaveGameData(serializableItems);
        Debug.Log("Inventory saved.");
    }

    public List<ItemData> GetInventoryItems()
    {
        return inventoryItems;
    }
}
