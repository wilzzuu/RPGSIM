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
    public TMP_Dropdown sortDropdown;
    public Toggle ascendingToggle;

    private List<ItemData> allItems = new List<ItemData>();
    private List<ItemData> inventoryItems = new List<ItemData>();
    private bool isBuyingTabActive = true;

    private int itemsLoaded = 0;
    private int itemsToLoadPerBatch = 50;
    private bool isLoadingMoreItems = false;

    private float fluctuationIntensity = 0.1f; // 1% price fluctuation range
    private int itemsToFluctuate = 10;          // Number of items to fluctuate each event
    private float marketEventInterval = 30f;
    private DateTime lastUpdateTimestamp;
    private const string LastUpdateKey = "LastMarketUpdate";

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

        ScrollRect scrollRect = (isBuyingTabActive ? buyScrollView : sellScrollView).GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnScroll);

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
                    ApplyFluctuation(item);
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
        inventoryItems = InventoryManager.instance.GetAllItems();
    }

    public void ShowBuyTab()
    {
        isBuyingTabActive = true;
        buyScrollView.SetActive(true);
        sellScrollView.SetActive(false);
        itemsLoaded = 0;
        DisplayItems();
    }

    public void ShowSellTab()
    {
        isBuyingTabActive = false;
        sellScrollView.SetActive(true);
        buyScrollView.SetActive(false);
        itemsLoaded = 0;
        DisplayItems();
    }

    void DisplayItems()
    {
        Transform currentCatalogGrid = isBuyingTabActive ? buyCatalogGrid : sellCatalogGrid;

        if (itemsLoaded == 0)
        {
            foreach (Transform child in currentCatalogGrid)
            {
                Destroy(child.gameObject);
            }
        }

        List<ItemData> itemsToDisplay = isBuyingTabActive ? allItems : inventoryItems;
        int endIndex = Mathf.Min(itemsLoaded + itemsToLoadPerBatch, itemsToDisplay.Count);

        for (int i = itemsLoaded; i < endIndex; i++)
        {
            GameObject itemObj = Instantiate(marketItemPrefab, currentCatalogGrid);
            MarketplaceItem marketplaceItem = itemObj.GetComponent<MarketplaceItem>();
            marketplaceItem.Setup(itemsToDisplay[i], isBuyingTabActive);
        }

        itemsLoaded = endIndex;
        isLoadingMoreItems = false;
    }

    public void BuyItem(ItemData item)
    {
        float purchasePrice = item.Price * 1.15f;
        if (PlayerManager.Instance.GetPlayerBalance() >= purchasePrice)
        {
            PlayerManager.Instance.DeductCurrency(purchasePrice);
            InventoryManager.instance.AddItemToInventory(item);
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
        if (InventoryManager.instance.HasItem(item))
        {
            InventoryManager.instance.RemoveItemFromInventory(item);
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
                float randomFluctuation = UnityEngine.Random.Range(-fluctuationIntensity, fluctuationIntensity);
                float newPrice = item.Price * (1 + randomFluctuation);

                // Clamp to a reasonable range around base price (50% - 200%)
                item.Price = Mathf.Clamp(newPrice, item.BasePrice * 0.5f, item.BasePrice * 2f);
                Debug.Log($"Market fluctuation: {item.Name} new price is ${item.Price:F2}");
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
