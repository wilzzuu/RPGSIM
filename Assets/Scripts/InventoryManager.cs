using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    private List<ItemData> inventoryItems = new List<ItemData>();
    private string SaveFileName
    {
        get
        {
            #if UNITY_EDITOR
                return "PlayTest_GameData.dat";
            #else
                return "GameData.dat";
            #endif
        }
    }

    public delegate void InventoryValueChangedHandler();
    public event InventoryValueChangedHandler onInventoryValueChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            List<SerializableItemData> loadedItems = DataSerializationManager.Instance.LoadGameData();
            inventoryItems = ConvertSerializableItemsToItemData(loadedItems);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
            SaveInventory();
            onInventoryValueChanged?.Invoke();
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
        onInventoryValueChanged?.Invoke();
    }

    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(inventoryItems);
    }

    public float CalculateInventoryValue()
    {
        float totalValue = 0;
        foreach (ItemData item in inventoryItems)
        {
            totalValue += item.Price;
        }
        return totalValue * 0.85f;
    }

    public void ClearInventory()
    {
        inventoryItems.Clear();
        string path = Path.Combine(Application.persistentDataPath, "SaveData/" + SaveFileName);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
        
        SaveInventory();
    }

    private void SaveInventory()
    {
        List<SerializableItemData> serializableItems = new List<SerializableItemData>();
        foreach (var item in inventoryItems)
        {
            serializableItems.Add(new SerializableItemData(item));
        }
        DataSerializationManager.Instance.SaveGameData(serializableItems);
    }

    public List<ItemData> GetInventoryItems()
    {
        return inventoryItems;
    }
}
