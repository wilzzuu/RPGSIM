using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScreen : MonoBehaviour
{
    public GameObject inventorySlotPrefab;
    public Transform inventoryGrid;
    public int itemsPerPage = 112;
    private int currentPage = 0;

    private List<SerializableItemData> items;

    void Start()
    {
        if (InventoryManager.instance == null)
        {
            Debug.LogError("InventoryManager instance is not set.");
            return;
        }

        // Load items from InventoryManager
        items = InventoryManager.instance.GetInventoryItems();
        DisplayCurrentPage();
    }

    void DisplayCurrentPage()
    {
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }

        int itemIndexOffset = currentPage * itemsPerPage;
        for (int i = itemIndexOffset; i < Mathf.Min(itemIndexOffset + itemsPerPage, items.Count); i++)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);
            SerializableItemData item = items[i];

            // Find Image components in prefab
            Image itemImage = slot.transform.Find("ItemImage").GetComponent<Image>();
            Image rarityImage = slot.transform.Find("RarityImage").GetComponent<Image>();

            // Load images based on paths
            string imagePath = "ItemImages/" + item.ID;
            string rarityPath = "RarityImages/" + item.Rarity;

            itemImage.sprite = Resources.Load<Sprite>(imagePath);
            rarityImage.sprite = Resources.Load<Sprite>(rarityPath);
        }
    }

    public void NextPage()
    {
        if ((currentPage + 1) * itemsPerPage < items.Count)
        {
            currentPage++;
            DisplayCurrentPage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            DisplayCurrentPage();
        }
    }

    public void DisplayTotalValue()
    {
        float totalValue = InventoryManager.instance.CalculateInventoryValue();
        Debug.Log("Total Inventory Value: " + totalValue);
    }
}