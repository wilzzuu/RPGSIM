using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections;
using System;

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
    public Button searchBtn;
    public TMP_InputField priceInput;
    public Button searchPriceBtn;
    public TMP_Dropdown sortDropdown;
    public Toggle ascendingToggle;
    public Toggle affordableItemsToggle;
    public TextMeshProUGUI inventoryValueText;

    private List<ItemData> allItems = new List<ItemData>();
    private List<ItemData> inventoryItems = new List<ItemData>();
    private bool isBuyingTabActive = false;

    private int itemsLoaded = 0;
    private bool isLoadingMoreItems = false;

    private float fluctuationIntensity = 0.1f;
    private int itemsToFluctuate = 20;
    private float marketEventInterval = 120f;
    private DateTime lastUpdateTimestamp;
    private const string LastUpdateKey = "LastMarketUpdate";

    private static readonly Dictionary<string, int> rarityOrder = new Dictionary<string, int>
    {
        {"Common", 1},
        {"Uncommon", 2},
        {"Rare", 3},
        {"Epic", 4},
        {"Legendary", 5}
    };

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

        ShowSellTab();

        buyTabButton.onClick.AddListener(ShowBuyTab);
        sellTabButton.onClick.AddListener(ShowSellTab);

        ScrollRect scrollRect = (isBuyingTabActive ? buyScrollView : sellScrollView).GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnScroll);

        searchBtn.onClick.AddListener(SearchItems);
        searchInput.onValueChanged.AddListener(delegate { ClearSearch(); });
        searchPriceBtn.onClick.AddListener(SearchPrices);
        priceInput.onValueChanged.AddListener(delegate { ClearPriceSearch(); });

        sortDropdown.onValueChanged.AddListener(delegate { SortItems(); });
        ascendingToggle.onValueChanged.AddListener(delegate { SortItems(); });
        affordableItemsToggle.onValueChanged.AddListener(delegate { DisplayItems(); });
        InventoryManager.Instance.onInventoryValueChanged += RefreshInventoryValue;
        PlayerManager.Instance.onBalanceChanged += RefreshBalanceFilter;

        LoadLastUpdateTimestamp();
        ApplyRealTimeFluctuations();
        StartCoroutine(MarketFluctuationCoroutine());
    }

    private void UpdateCurrentTab()
    {
        DisplayItems();
    }

    void LoadLastUpdateTimestamp()
    {
        string lastUpdateString = PlayerPrefs.GetString(LastUpdateKey, DateTime.UtcNow.ToString());
        lastUpdateTimestamp = DateTime.Parse(lastUpdateString);
    }

    void ApplyRealTimeFluctuations()
    {
        DateTime currentTime = DateTime.UtcNow;
        TimeSpan elapsedTime = currentTime - lastUpdateTimestamp;

        int intervals = (int)(elapsedTime.TotalSeconds / marketEventInterval);

        if (intervals > 0)
        {
            for (int i = 0; i < intervals; i++)
            {
                List<ItemData> itemsForFluctuation = allItems.OrderBy(x => UnityEngine.Random.value).Take(itemsToFluctuate).ToList();

                foreach (var item in itemsForFluctuation)
                {
                    float previousPrice = item.Price;
                    ApplyFluctuation(item);
                    Debug.Log($"[Real-Time Fluctuation] Item: {item.Name}, Previous Price: {previousPrice:F2}, New Price: {item.Price:F2}");
                }
            }

            lastUpdateTimestamp = currentTime;
            PlayerPrefs.SetString(LastUpdateKey, lastUpdateTimestamp.ToString());
            PlayerPrefs.Save();
        }
    }

    void OnScroll(Vector2 scrollPosition)
    {
        if (scrollPosition.y <= 0.1f && !isLoadingMoreItems && itemsLoaded < (isBuyingTabActive ? allItems.Count : inventoryItems.Count))
        {
            isLoadingMoreItems = true;
            DisplayItems();
        }
    }

    void LoadAllGameItems()
    {
        allItems = Resources.LoadAll<ItemData>("Items").ToList();
    }

    void LoadInventoryItems()
    {
        inventoryItems = InventoryManager.Instance.GetAllItems();
    }

    public void ShowBuyTab()
    {
        isBuyingTabActive = true;
        buyScrollView.SetActive(true);
        sellScrollView.SetActive(false);
        itemsLoaded = 0;
        RefreshInventoryValue();
        DisplayItems();
    }

    public void ShowSellTab()
    {
        isBuyingTabActive = false;
        buyScrollView.SetActive(false);
        sellScrollView.SetActive(true);
        itemsLoaded = 0;
        RefreshInventoryValue();
        DisplayItems();
    }

    void RefreshInventoryValue()
    {
        float inventoryValue = InventoryManager.Instance.CalculateInventoryValue();
        inventoryValueText.text = $"Inventory Value:        {inventoryValue:F2}";
    }

    void RefreshBalanceFilter()
    {
        if (isBuyingTabActive && affordableItemsToggle.isOn)
        {
            DisplayItems();
        }
    }

    void DisplayItems()
    {
        Transform currentCatalogGrid = isBuyingTabActive ? buyCatalogGrid : sellCatalogGrid;
        List<ItemData> displayedItems = isBuyingTabActive ? allItems : inventoryItems;

        if (currentCatalogGrid == null) return;
        
        if (isBuyingTabActive && affordableItemsToggle.isOn)
        {
            float playerBalance = PlayerManager.Instance.GetPlayerBalance();
            displayedItems = displayedItems.Where(item => item.Price * 1.15f <= playerBalance).ToList();
        }

        foreach (Transform child in currentCatalogGrid)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < displayedItems.Count; i++)
        {
            GameObject itemObj = Instantiate(marketItemPrefab, currentCatalogGrid);
            MarketplaceItem marketplaceItem = itemObj.GetComponent<MarketplaceItem>();
            marketplaceItem.Setup(displayedItems[i], isBuyingTabActive);
        }
    }

    public void ClearSearch()
    {
        if (string.IsNullOrWhiteSpace(searchInput.text))
        {
            LoadAllGameItems();
            LoadInventoryItems();
        }
    }

    public void ClearPriceSearch()
    {
        if (string.IsNullOrWhiteSpace(priceInput.text))
        {
            LoadAllGameItems();
            LoadInventoryItems();
        }
    }

    public void SortItems()
    {
        allItems = new List<ItemData>(allItems);

        string sortCriteria = sortDropdown.options[sortDropdown.value].text;
        bool ascending = ascendingToggle.isOn;

        allItems = SortItemsByCriteria(sortCriteria, ascending);
        Debug.Log($"Sorted {allItems.Count} items by {sortCriteria}.");

        UpdateCurrentTab();
    }
    public void SearchItems()
    {
        string query = searchInput.text.ToLower();

        // Check if the search query is empty; if so, display all items
        if (string.IsNullOrWhiteSpace(query))
        {
            LoadAllGameItems();
            SortItems();
        }
        else
        {
            allItems = allItems.Where(item =>
                item.Name.ToLower().Contains(query) ||
                item.ID.ToString().Contains(query)).ToList();
        }
        UpdateCurrentTab();
    }

    public void SearchPrices()
    {
        string query = priceInput.text.ToString();
        if (string.IsNullOrWhiteSpace(query))
        {
            LoadAllGameItems();
            SortItems();
        }

        if (float.TryParse(query, out float itemPriceQuery))
        {
            allItems = allItems.Where(item => item.Price * 1.15f <= itemPriceQuery).ToList();
        }
        UpdateCurrentTab();
    }

    private List<ItemData> SortItemsByCriteria(string criteria, bool ascending)
    {
        return criteria switch
        {
            "Item" => ascending ? allItems.OrderBy(i => i.Item).ToList() : allItems.OrderByDescending(i => i.Item).ToList(),
            "Rarity" => ascending
                ? allItems.OrderBy(i => rarityOrder.ContainsKey(i.Rarity) ? rarityOrder[i.Rarity] : int.MaxValue).ToList()
                : allItems.OrderByDescending(i => rarityOrder.ContainsKey(i.Rarity) ? rarityOrder[i.Rarity] : int.MinValue).ToList(),
            "Color" => ascending ? allItems.OrderBy(i => i.Color).ToList() : allItems.OrderByDescending(i => i.Color).ToList(),
            "Style" => ascending ? allItems.OrderBy(i => i.Style).ToList() : allItems.OrderByDescending(i => i.Style).ToList(),
            "Price" => ascending ? allItems.OrderBy(i => i.Price).ToList() : allItems.OrderByDescending(i => i.Price).ToList(),
            _ => allItems
        };
    }

    public void BuyItem(ItemData item)
    {
        float purchasePrice = item.Price * 1.15f;
        if (PlayerManager.Instance.GetPlayerBalance() >= purchasePrice)
        {
            PlayerManager.Instance.DeductCurrency(purchasePrice);
            InventoryManager.Instance.AddItemToInventory(item);
            AdjustItemPrice(item, 1);
        }
        else
        {
            Debug.Log("Not enough balance to purchase this item.");
        }
    }

    public void SellItem(ItemData item, GameObject itemPrefab)
    {
        float sellingPrice = item.Price * 0.85f;
        if (InventoryManager.Instance.HasItem(item))
        {
            InventoryManager.Instance.RemoveItemFromInventory(item);
            PlayerManager.Instance.AddCurrency(sellingPrice);
            Destroy(itemPrefab);
            AdjustItemPrice(item, -1);
        }
        else
        {
            Debug.Log("Item not available in inventory for sale.");
        }
    }

    private void AdjustItemPrice(ItemData item, int change)
    {
        item.DemandScore += change;
        item.Price = Mathf.Clamp(item.BasePrice * (1 + item.DemandScore * fluctuationIntensity), item.BasePrice * 0.75f, item.BasePrice * 1.5f);
    }

    private IEnumerator MarketFluctuationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(marketEventInterval);

            // Randomly select items to fluctuate
            List<ItemData> itemsForFluctuation = allItems.OrderBy(x => UnityEngine.Random.value).Take(itemsToFluctuate).ToList();

            foreach (ItemData item in itemsForFluctuation)
            {
                float previousPrice = item.Price;
                float randomFluctuation = UnityEngine.Random.Range(-fluctuationIntensity, fluctuationIntensity);
                float newPrice = item.Price * (1 + randomFluctuation);

                // Clamp to a reasonable range around base price (50% - 200%)
                item.Price = Mathf.Clamp(newPrice, item.BasePrice * 0.5f, item.BasePrice * 2f);
                Debug.Log($"[Fluctuation Event] Item: {item.Name}, Previous Price: {previousPrice:F2}, New Price: {item.Price:F2}, Fluctuation: {randomFluctuation:P}");
            }

            Debug.Log("Market event: Selected items fluctuated.");
            UpdateCurrentTab();  // Refresh the displayed items to reflect updated prices
        }
    }

    private void ApplyFluctuation(ItemData item)
    {
        if (item.BasePrice <= 0)
        {
            Debug.LogWarning($"{item.Name} has a base price of zero or below. Skipping fluctuation.");
            return;
        }

        float randomFluctuation = UnityEngine.Random.Range(-fluctuationIntensity, fluctuationIntensity);
        float newPrice = item.Price * (1 + randomFluctuation);

        item.Price = Mathf.Clamp(newPrice, item.BasePrice * 0.5f, item.BasePrice * 2f);

        if (item.Price < 0.01f)
        {
            item.Price = item.BasePrice; // Reset to base price if fluctuation is too low
        }
    }
}
