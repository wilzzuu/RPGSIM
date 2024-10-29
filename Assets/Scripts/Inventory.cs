using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScreen : MonoBehaviour
{
    public GameObject inventorySlotPrefab;
    public Transform inventoryGrid;
    public Text totalValueText;
    public int itemsPerPage = 112;
    private int currentPage = 0;

    private List<ItemData> items = new List<ItemData>();

    void Start()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager instance is not set.");
            return;
        }

        items = InventoryManager.Instance.GetInventoryItems();
        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("No items found in inventory.");
            return;
        }

        DisplayCurrentPage();
        DisplayTotalValue();
    }

    void DisplayCurrentPage()
    {
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }

        int itemIndexOffset = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(itemIndexOffset + itemsPerPage, items.Count);

        for (int i = itemIndexOffset; i < endIndex; i++)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);
            ItemData item = items[i];

            Image itemImage = slot.transform.Find("ItemImage")?.GetComponent<Image>();
            Image rarityImage = slot.transform.Find("RarityImage")?.GetComponent<Image>();

            if (itemImage != null)
            {
                string imagePath = "ItemImages/" + item.ID;
                Sprite itemSprite = Resources.Load<Sprite>(imagePath);
                if (itemSprite != null)
                {
                    itemImage.sprite = itemSprite;
                }
                else
                {
                    Debug.LogWarning($"Failed to load item image for path: {imagePath}");
                }
            }

            if (rarityImage != null)
            {
                string rarityPath = "RarityImages/" + item.Rarity;
                Sprite raritySprite = Resources.Load<Sprite>(rarityPath);
                if (raritySprite != null)
                {
                    rarityImage.sprite = raritySprite;
                }
                else
                {
                    Debug.LogWarning($"Failed to load rarity image for path: {rarityPath}");
                }
            }
        }
    }

    public void NextPage()
    {
        if ((currentPage + 1) * itemsPerPage < items.Count)
        {
            currentPage++;
            DisplayCurrentPage();
        }
        else
        {
            Debug.Log("Reached the last page.");
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            DisplayCurrentPage();
        }
        else
        {
            Debug.Log("Already at the first page.");
        }
    }

    public void DisplayTotalValue()
    {
        float totalValue = InventoryManager.Instance.CalculateInventoryValue();
        Debug.Log("Total Inventory Value: " + totalValue);

        if (totalValueText != null)
        {
            totalValueText.text = $"Total Value: {totalValue:F2}";
        }
    }
}
