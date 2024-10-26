using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScreen : MonoBehaviour
{
    public GameObject inventorySlotPrefab;
    public Transform inventoryGrid;
    public int itemsPerPage = 112;
    public int currentPage = 0;

    private List<ItemData> inventoryItems;

    void Start()
    {
        if (InventoryManager.instance == null)
        {
            Debug.LogError("InventoryManager instance is not set. Make sure InventoryManager is added to the scene.");
            return;
        }
        
        LoadInventory(); 
        DisplayCurrentPage();
        CalculateInventoryValue();
    }

    void LoadInventory()
    {
        foreach (ItemData item in inventoryItems)
        {
            Debug.Log(item);
        }
    }

    void DisplayCurrentPage()
    {
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }

        int itemIndexOffset = currentPage * itemsPerPage;
        for (int i = itemIndexOffset; i < Mathf.Min(itemIndexOffset + itemsPerPage, inventoryItems.Count); i++)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);
            ItemData item = inventoryItems[i];

            Image itemImage = slot.transform.Find("ItemImage").GetComponent<Image>();
            Image rarityImage = slot.transform.Find("RarityImage").GetComponent<Image>();

            string imagePath = "ItemImages/" + item.ID;
            string rarityPath = "RarityImages/" + item.Rarity;

            itemImage.sprite = Resources.Load<Sprite>(imagePath);
            rarityImage.sprite = Resources.Load<Sprite>(rarityPath);

        }
    }

    public void NextPage()
    {
        if ((currentPage + 1) * itemsPerPage < inventoryItems.Count)
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

    void CalculateInventoryValue()
    {
        float totalValue = 0;
        foreach (ItemData item in inventoryItems)
        {
            totalValue += item.Price;
        }
    }
}