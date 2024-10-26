using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MarketManager : MonoBehaviour
{
    public static MarketManager instance { get; private set; }
    public Transform buyCatalogGrid;
    public Transform sellCatalogGrid;
    public GameObject marketItemPrefab;

    public Button buyTabButton;
    public Button sellTabButton;
    public GameObject buyScrollView;
    public GameObject sellScrollView;

    public TMP_InputField searchInput;
    public TMP_Dropdown sortDropdown;
    public Toggle ascendingToggle;

    private List<ItemData> allItems = new List<ItemData>();       // All items available for buying
    private List<ItemData> inventoryItems = new List<ItemData>(); // Items in the player's inventory
    private List<ItemData> displayedItems = new List<ItemData>(); // Filtered/Sorted items displayed in the catalog
    private bool isBuyingTabActive = true; // Tracks if the Buy tab is active
    
    private int itemsLoaded = 0;
    private int itemsToLoadPerBatch = 50; // Number of items to load per batch
    private bool isLoadingMoreItems = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadAllGameItems();
        LoadInventoryItems();
        
        ShowBuyTab();

        buyTabButton.onClick.AddListener(ShowBuyTab);
        sellTabButton.onClick.AddListener(ShowSellTab);

        // Add listener for scrolling event
        ScrollRect scrollRect = (isBuyingTabActive ? buyScrollView : sellScrollView).GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    void OnScroll(Vector2 scrollPosition)
    {
        // Check if near the bottom and if not already loading more items
        if (scrollPosition.y <= 0.1f && !isLoadingMoreItems && itemsLoaded < (isBuyingTabActive ? allItems.Count : inventoryItems.Count))
        {
            isLoadingMoreItems = true;
            DisplayItems(); // Load the next batch of items
        }
    }

    void LoadAllGameItems()
    {
        allItems = Resources.LoadAll<ItemData>("Items").ToList();
        Debug.Log($"Loaded {allItems.Count} items for the market.");
    }
    void LoadInventoryItems()
    {
        inventoryItems = InventoryManager.instance.GetAllItems();
        Debug.Log($"Loaded {inventoryItems.Count} items from inventory.");
    }

    // Show the Buy tab and load items
    public void ShowBuyTab()
    {
        isBuyingTabActive = true;
        buyScrollView.SetActive(true);
        sellScrollView.SetActive(false);
        itemsLoaded = 0; // Reset items loaded for fresh load
        DisplayItems();
    }

    public void ShowSellTab()
    {
        isBuyingTabActive = false;
        sellScrollView.SetActive(true);
        buyScrollView.SetActive(false);
        itemsLoaded = 0; // Reset items loaded for fresh load
        DisplayItems();
    }


    // Display items in the given catalog grid
    void DisplayItems()
    {
        Transform currentCatalogGrid = isBuyingTabActive ? buyCatalogGrid : sellCatalogGrid;

        // Clear existing items if starting fresh
        if (itemsLoaded == 0)
        {
            foreach (Transform child in currentCatalogGrid)
            {
                Destroy(child.gameObject);
            }
        }

        // Get the items to display
        List<ItemData> itemsToDisplay = isBuyingTabActive ? allItems : inventoryItems;

        // Limit loading to a specific batch size
        int endIndex = Mathf.Min(itemsLoaded + itemsToLoadPerBatch, itemsToDisplay.Count);

        for (int i = itemsLoaded; i < endIndex; i++)
        {
            GameObject itemObj = Instantiate(marketItemPrefab, currentCatalogGrid);
            MarketplaceItem marketplaceItem = itemObj.GetComponent<MarketplaceItem>();
            marketplaceItem.Setup(itemsToDisplay[i], isBuyingTabActive);
        }

        // Update the number of loaded items
        itemsLoaded = endIndex;
        isLoadingMoreItems = false; // Reset loading flag
    }


    public void SortItems()
    {
        string sortCriteria = sortDropdown.options[sortDropdown.value].text;
        bool ascending = ascendingToggle.isOn;

        displayedItems = SortItemsByCriteria(sortCriteria, ascending);
        Debug.Log($"Sorted {displayedItems.Count} items by {sortCriteria}.");
        UpdateCurrentTab();
    }

    // Filter items based on search query
    public void SearchItems()
    {
        string query = searchInput.text.ToLower();

        displayedItems = (isBuyingTabActive ? allItems : inventoryItems).Where(item =>
            item.Name.ToLower().Contains(query) ||
            item.ID.ToString().Contains(query)).ToList();

        UpdateCurrentTab();
    }

    // Sort items based on chosen criteria
    private List<ItemData> SortItemsByCriteria(string criteria, bool ascending)
    {
        return criteria switch
        {
            "Type" => ascending ? displayedItems.OrderBy(i => i.Item).ToList() : displayedItems.OrderByDescending(i => i.Item).ToList(),
            "Rarity" => ascending ? displayedItems.OrderBy(i => i.Rarity).ToList() : displayedItems.OrderByDescending(i => i.Rarity).ToList(),
            "Color" => ascending ? displayedItems.OrderBy(i => i.Color).ToList() : displayedItems.OrderByDescending(i => i.Color).ToList(),
            "Style" => ascending ? displayedItems.OrderBy(i => i.Style).ToList() : displayedItems.OrderByDescending(i => i.Style).ToList(),
            "Price" => ascending ? displayedItems.OrderBy(i => i.Price).ToList() : displayedItems.OrderByDescending(i => i.Price).ToList(),
            _ => displayedItems
        };
    }

    // Update the active tab with the sorted/filtered items
    private void UpdateCurrentTab()
    {
        if (isBuyingTabActive)
        {
            DisplayItems();
        }
        else
        {
            DisplayItems();
        }
    }

    // Buy an item
    public void BuyItem(ItemData item)
    {
        float purchasePrice = item.Price * 1.15f;
        if (PlayerManager.Instance.GetPlayerBalance() >= purchasePrice)
        {
            PlayerManager.Instance.DeductCurrency(purchasePrice);
            InventoryManager.instance.AddItemToInventory(item);
            Debug.Log($"Purchased {item.Name} for ${purchasePrice:F2}");
        }
        else
        {
            Debug.Log("Not enough balance to purchase this item.");
        }
    }

    // Sell an item
    public void SellItem(ItemData item)
    {
        float sellingPrice = item.Price * 0.85f;
        if (InventoryManager.instance.HasItem(item))
        {
            InventoryManager.instance.RemoveItemFromInventory(item);
            PlayerManager.Instance.AddCurrency(sellingPrice);
            Debug.Log($"Sold {item.Name} for ${sellingPrice:F2}");
        }
        else
        {
            Debug.Log("Item not available in inventory for sale.");
        }
    }
}
