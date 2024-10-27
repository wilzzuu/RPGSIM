using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class UpgraderManager : MonoBehaviour
{
    public static UpgraderManager instance { get; private set; }
    
    public Transform inventoryCatalogGrid;
    public Transform upgradeCatalogGrid;
    public GameObject upgraderItemPrefab;
    public GameObject selectedUpgraderItemPrefab;
    public TMP_InputField searchInput;
    public TMP_Dropdown sortDropdown;
    public Toggle ascendingToggle;
    public Button inventoryTabButton;
    public Button upgradeTabButton;
    public GameObject inventoryScrollView;
    public GameObject upgradeItemsScrollView;
    public Button[] multiplierButtons;

    private GameObject selectedInventoryItemObj;
    private GameObject selectedUpgradeItemObj;
    public Transform selectedInventoryItemContainer;
    public Transform selectedUpgradeItemContainer;
    public TextMeshProUGUI probabilityText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI isSuccessText;
    public GameObject selectItemText1;
    public GameObject selectItemText2;
    public Button upgradeButton;

    private int itemsLoaded = 0;
    private bool isLoadingMoreItems = false;

    private List<ItemData> allItems = new List<ItemData>();
    private List<ItemData> inventoryItems = new List<ItemData>();
    private bool isInventoryTabActive = true;
    private ItemData selectedInventoryItem;
    private ItemData selectedUpgradeItem;
    private float successProbability;

    private static readonly Dictionary<string, int> rarityOrder = new Dictionary<string, int>
    {
        {"Common", 1},
        {"Uncommon", 2},
        {"Rare", 3},
        {"Epic", 4},
        {"Legendary", 5}
    };

    private float[] multipliers = { 1.5f, 2f, 5f, 10f, 20f };

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadAllGameItems();
        LoadInventoryItems();


        upgradeTabButton.interactable = false;
        ShowInventoryTab();

        SetMultiplierButtons(false);

        for (int i = 0; i < multiplierButtons.Length; i++)
        {
            float multiplier = multipliers[i];
            multiplierButtons[i].onClick.AddListener(() => SelectRandomUpgradeItem(multiplier));
        }

        inventoryTabButton.onClick.AddListener(ShowInventoryTab);
        upgradeTabButton.onClick.AddListener(ShowUpgradeTab);

        ScrollRect scrollRect = (isInventoryTabActive ? inventoryScrollView : upgradeItemsScrollView).GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnScroll);

        searchInput.onValueChanged.AddListener(delegate { SearchItems(); });
        sortDropdown.onValueChanged.AddListener(delegate { SortItems(); });
        ascendingToggle.onValueChanged.AddListener(delegate { SortItems(); });
        
        upgradeButton.onClick.AddListener(AttemptUpgrade);
    }

    private void SetMultiplierButtons(bool enable)
    {
        foreach (Button button in multiplierButtons)
        {
            button.interactable = enable;
        }
    }

    private void UpdateCurrentTab()
    {
        DisplayItems();
    }

    void DisplayItems()
    {
        Transform currentCatalogGrid = isInventoryTabActive ? inventoryCatalogGrid : upgradeCatalogGrid;
        List<ItemData> displayedItems = isInventoryTabActive ? inventoryItems : allItems;

        if (!isInventoryTabActive && selectedInventoryItem != null)
        {
            displayedItems = allItems.FindAll(upgradeItem => upgradeItem.Price > selectedInventoryItem.Price);
        }

        foreach (Transform child in currentCatalogGrid)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < displayedItems.Count; i++)
        {
            GameObject itemObj = Instantiate(upgraderItemPrefab, currentCatalogGrid);
            UpgraderItem upgraderItem = itemObj.GetComponent<UpgraderItem>();
            upgraderItem.Setup(displayedItems[i], isInventoryTabActive);
        }
    }

    void OnScroll(Vector2 scrollPosition)
    {
        if (scrollPosition.y <= 0.1f && !isLoadingMoreItems && itemsLoaded < (isInventoryTabActive ? allItems.Count : inventoryItems.Count))
        {
            isLoadingMoreItems = true;
            DisplayItems();
        }
    }

    // Load inventory items
    void LoadAllGameItems()
    {
        allItems = Resources.LoadAll<ItemData>("Items").ToList();
    }

    void LoadInventoryItems()
    {
        inventoryItems = InventoryManager.instance.GetAllItems();
    }

    public void ShowInventoryTab()
    {
        isInventoryTabActive = true;
        inventoryScrollView.SetActive(true);
        upgradeItemsScrollView.SetActive(false);
        itemsLoaded = 0;
        DisplayItems();
    }

    public void ShowUpgradeTab()
    {
        isInventoryTabActive = false;
        inventoryScrollView.SetActive(false);
        upgradeItemsScrollView.SetActive(true);
        itemsLoaded = 0;
        DisplayItems();
    }

    public void SelectInventoryItem(ItemData item)
    {
        selectedInventoryItem = item;
        upgradeTabButton.interactable = selectedInventoryItem != null;
        SetMultiplierButtons(true);

        foreach (Transform child in selectedInventoryItemContainer.transform)
        {
            Debug.Log("Destroying previous selectedInventoryItemObj");
            DestroyImmediate(child.gameObject);
        }

        selectedInventoryItemObj = Instantiate(selectedUpgraderItemPrefab, selectedInventoryItemContainer);
        Debug.Log("New selectedInventoryItemObj instantiated: " + selectedInventoryItemObj.name);

        isSuccessText.text = "";
        PopulateItemPrefab(selectedInventoryItemObj, item);
        selectItemText1.SetActive(false);
        UpdateUpgradeProbability();
    }

    public void SelectUpgradeItem(ItemData item)
    {
        selectedUpgradeItem = item;

        foreach (Transform child in selectedUpgradeItemContainer.transform)
        {
            Debug.Log("Destroying previous selectedUpgradeItemObj");
            DestroyImmediate(child.gameObject);
        }


        selectedUpgradeItemObj = Instantiate(selectedUpgraderItemPrefab, selectedUpgradeItemContainer);
        Debug.Log("New selectedUpgradeItemObj instantiated: " + selectedUpgradeItemObj.name);

        isSuccessText.text = "";
        PopulateItemPrefab(selectedUpgradeItemObj, item);
        selectItemText2.SetActive(false);
        UpdateUpgradeProbability();
    }

    private void SelectRandomUpgradeItem(float multiplier)
    {
        if (selectedInventoryItem == null) return;

        float targetPrice = selectedInventoryItem.Price * multiplier;
        float minPrice = targetPrice * 0.8f;
        float maxPrice = targetPrice * 1.2f;

        List<ItemData> validItems = allItems.FindAll(upgradeItem => upgradeItem.Price >= minPrice && upgradeItem.Price <= maxPrice);

        if (validItems.Count == 0)
        {
            validItems = allItems.FindAll(upgradeItem => upgradeItem.Price > selectedInventoryItem.Price);
        }

        if (validItems.Count > 0)
        {
            ItemData randomItem = validItems[UnityEngine.Random.Range(0, validItems.Count)];
            SelectUpgradeItem(randomItem); // Select the random upgrade item
            Debug.Log($"Randomly selected upgrade item: {randomItem.Name} with price: {randomItem.Price}");
        }
        else
        {
            Debug.Log("No valid items found within the specified range.");
        }
    }

    private void PopulateItemPrefab(GameObject itemObj, ItemData item)
    {
        Image itemImage = itemObj.transform.Find("ItemImage").GetComponent<Image>();
        Image rarityImage = itemObj.transform.Find("RarityImage").GetComponent<Image>();
        TextMeshProUGUI nameText = itemObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = itemObj.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();

        itemImage.sprite = Resources.Load<Sprite>($"ItemImages/{item.ID}");
        rarityImage.sprite = Resources.Load<Sprite>($"RarityImages/{item.Rarity}");
        nameText.text = item.Name;
        priceText.text = $"{item.Price:0.00}";
    }

    void UpdateUpgradeProbability()
    {
        if (selectedInventoryItem != null && selectedUpgradeItem != null)
        {
            successProbability = selectedInventoryItem.Price / selectedUpgradeItem.Price * 100;
            probabilityText.fontSize = 32;
            probabilityText.text = $"{successProbability:0.00}%";
            multiplierText.text = $"{selectedUpgradeItem.Price / selectedInventoryItem.Price:0.0}x";
        }
        else
        {
            probabilityText.fontSize = 20;
            probabilityText.text = "Select Both Items";
            multiplierText.text = "-";
        }
    }

    public void AttemptUpgrade()
    {
        if (selectedInventoryItem == null || selectedUpgradeItem == null) return;

        float chance = UnityEngine.Random.value;
        if (chance <= successProbability)
        {
            InventoryManager.instance.AddItemToInventory(selectedUpgradeItem);
            InventoryManager.instance.RemoveItemFromInventory(selectedInventoryItem);
            isSuccessText.text = "Upgrade Successful!";
            Debug.Log("Upgrade Successful!");
        }
        else
        {
            InventoryManager.instance.RemoveItemFromInventory(selectedInventoryItem);
            isSuccessText.text = "Upgrade Failed!";
            Debug.Log("Upgrade Failed!");
        }

        LoadInventoryItems();

        // Reset selected items
        selectedInventoryItem = null;
        selectedUpgradeItem = null;
        selectedInventoryItemObj = null;
        selectedUpgradeItemObj = null;
        selectItemText1.SetActive(true);
        selectItemText2.SetActive(true);
        probabilityText.text = "-";
        upgradeTabButton.interactable = false;
        ShowInventoryTab();
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
            allItems = new List<ItemData>(allItems);
        }
        else
        {
            // Filter items based on the search query
            allItems = allItems.Where(item =>
                item.Name.ToLower().Contains(query) ||
                item.ID.ToString().Contains(query)).ToList();
        }

        // Update the current tab to reflect the search results or reset if empty
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
}
