using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Random = UnityEngine.Random;

public class CrateOpening : MonoBehaviour
{
    public GameObject crateItemPrefab;
    public Transform crateItemGrid;
    public GameObject openedItemPrefab;
    public Transform crateGridParent;
    public float initialScrollSpeed = 4000f;
    public float finalScrollSpeed = 100f;
    public int numberOfReelItems = 40;
    public float easingDuration = 7f;
    
    private bool _isScrolling;
    private Vector3 _initialReelPosition;
    private GridLayoutGroup _gridLayout;
    private RectTransform _reelTransform;
    private int _openedItemIndex;
    private float _randomOffset;

    public GameObject crateButtonPrefab;
    public Transform crateSelectorPanel;
    public Button selectCrateButton;
    public Button openCrateButton;
    private bool _isSelectorOpen;
    private bool _isFirstSelection = true;
    private List<CrateData> _availableCrates;
    private CrateData _selectedCrateData;

    public UIManager uiManager;

    private static readonly Dictionary<string, int> RarityOrder = new Dictionary<string, int>
    {
        {"Common", 1},
        {"Uncommon", 2},
        {"Rare", 3},
        {"Epic", 4},
        {"Legendary", 5}
    };

    void Start()
    {   
        openCrateButton.interactable = false;
        _availableCrates = new List<CrateData>(Resources.LoadAll<CrateData>("Crates"));

        DisplayCrateSelector(_availableCrates);
        selectCrateButton.onClick.AddListener(ToggleCrateSelector);

        _gridLayout = crateGridParent.GetComponent<GridLayoutGroup>();
        _reelTransform = crateGridParent.GetComponent<RectTransform>();

        SetInitialReelPosition();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void SelectCrate(CrateData chosenCrate)
    {
        _selectedCrateData = chosenCrate;

        if (_selectedCrateData) openCrateButton.interactable = true;
        else openCrateButton.interactable = false;

        if (_selectedCrateData.Price <= PlayerManager.Instance.GetPlayerBalance()) openCrateButton.interactable = true;
        else openCrateButton.interactable = false;
        
        DisplayCrateItems(_selectedCrateData);

        if (_isFirstSelection)
        {
            _isFirstSelection = false;
            _isSelectorOpen = false;
            crateSelectorPanel.gameObject.SetActive(false);
        }
        else
        {
            ToggleCrateSelector();
        }
    }

    private void SetInitialReelPosition()
    {
        float itemWidth = _gridLayout.cellSize.x + _gridLayout.spacing.x;
        float totalReelWidth = itemWidth * numberOfReelItems;
        
        _initialReelPosition = new Vector3(totalReelWidth / 2 - 800, crateGridParent.localPosition.y, crateGridParent.localPosition.z);
        crateGridParent.localPosition = _initialReelPosition;
    }

    public void OpenCrate()
    {
        if (_isScrolling) return;

        if (PlayerManager.Instance == null || _selectedCrateData == null)
        {
            Debug.LogError("PlayerManager or selectedCrateData is null");
            return;
        }

        if (_selectedCrateData == null)
        {
            Debug.LogError("No crate selected. Plerate select a crate first.");
            return;
        }

        foreach (Transform child in crateGridParent) 
        {
            Destroy(child.gameObject);
        }

        crateGridParent.localPosition = _initialReelPosition;

        if (PlayerManager.Instance.GetPlayerBalance() >= _selectedCrateData.Price)
        {
            PlayerManager.Instance.DeductCurrency(_selectedCrateData.Price);
            ItemData openedItem = GetRandomItemByPercentage();
            StartCoroutine(AnimateScrollingReel(openedItem));
        }
        else
        {
            Debug.LogError("Not enough money to open this crate.");
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private ItemData GetRandomItemByPercentage()
    {
        if (_selectedCrateData.Items == null || _selectedCrateData.Items.Count == 0)
        {
            Debug.LogError("No items in the selected crate. Ensure the selected crate has items.");
            return null; 
        }

        float totalWeight = 0f;
        foreach (var item in _selectedCrateData.Items)
        {
            totalWeight += item.Weight;  
        }

        
        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var item in _selectedCrateData.Items)
        {
            cumulativeWeight += item.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return item;
            }
        }

        Debug.LogWarning("No item was selected; returning default item.");
        return _selectedCrateData.Items[_selectedCrateData.Items.Count - 1];
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator AnimateScrollingReel(ItemData openedItem)
    {
        _isScrolling = true;
        openCrateButton.interactable = false;
        selectCrateButton.interactable = false;
        uiManager.LockUI();

        List<GameObject> reelItems = new List<GameObject>();
        if (reelItems == null) throw new ArgumentNullException(nameof(reelItems));
        float itemWidth = _gridLayout.cellSize.x + _gridLayout.spacing.x;
        float totalReelWidth = itemWidth * numberOfReelItems;
        _reelTransform.sizeDelta = new Vector2(totalReelWidth, _reelTransform.sizeDelta.y);

        _openedItemIndex = Random.Range(24, numberOfReelItems - 4);
        for (int i = 0; i < numberOfReelItems; i++)
        {
            ItemData itemData = (i == _openedItemIndex) ? openedItem : GetRandomItemByPercentage();
            GameObject reelItem = Instantiate(openedItemPrefab, crateGridParent);
            SetUpReelItem(reelItem, itemData);
            reelItems.Add(reelItem);
        }

        _randomOffset = Random.Range(0f, 256f);
        float targetPosition = _initialReelPosition.x - (itemWidth * _openedItemIndex) + 800 - _randomOffset;

        float elapsed = 0f;

        while (elapsed < easingDuration)
        {
            float t = elapsed / easingDuration;
            float erateFactor = ErateOutQuint(t);

            float currentPosition = Mathf.Lerp(_initialReelPosition.x, targetPosition, erateFactor);
            crateGridParent.localPosition = new Vector3(currentPosition, crateGridParent.localPosition.y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        crateGridParent.localPosition = new Vector3(targetPosition, crateGridParent.localPosition.y, 0);

        StopReel(openedItem);
        _isScrolling = false;
        selectCrateButton.interactable = true;
        uiManager.UnlockUI();
        
    }

    private void StopReel(ItemData openedItem)
    {
        InventoryManager.Instance.AddItemToInventory(openedItem);
        CollectionManager.Instance.AddItemToCollection(openedItem);
        if (PlayerManager.Instance.GetPlayerBalance() < _selectedCrateData.Price)
        {
            DisplayCrateSelector(_availableCrates);
            ToggleCrateSelector();
            openCrateButton.interactable = false;
        }
        else
        {
            openCrateButton.interactable = true;
        }
    }

    private void SetUpReelItem(GameObject reelItem, ItemData itemData)
    {
        Image itemImage = reelItem.transform.Find("ItemImage").GetComponent<Image>();
        Image rarityImage = reelItem.transform.Find("RarityImage").GetComponent<Image>();

        string imagePath = "ItemImages/" + itemData.ID;
        string rarityPath = "RarityImages/" + itemData.Rarity;

        itemImage.sprite = Resources.Load<Sprite>(imagePath);
        rarityImage.sprite = Resources.Load<Sprite>(rarityPath);
    }

    
    private float ErateOutQuint(float t)
    {
        return 1 - Mathf.Pow(1 - t, 5);
    }

    public void DisplayCrateItems(CrateData selectedCrate)
    {
        if (crateItemGrid == null)
        {
            Debug.LogError("crateItemGrid is not assigned in the Inspector.");
            return;
        }

        selectedCrate.Items = selectedCrate.Items.OrderBy(item => RarityOrder[item.Rarity]).ToList();

        foreach (Transform child in crateItemGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var itemData in selectedCrate.Items)
        {
            if (itemData == null)
            {
                continue;
            }

            GameObject item = Instantiate(crateItemPrefab, crateItemGrid);
            Image itemImage = item.transform.Find("ItemImage").GetComponent<Image>();
            Image rarityImage = item.transform.Find("RarityImage").GetComponent<Image>();
            TextMeshProUGUI nameText = item.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = item.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();

            itemImage.sprite = Resources.Load<Sprite>($"ItemImages/{itemData.ID}");
            rarityImage.sprite = Resources.Load<Sprite>($"RarityImages/{itemData.Rarity}");
            nameText.text = itemData.Name;
            priceText.text = $"{itemData.Price:F2}";
        }
    }

    public void ToggleCrateSelector()
    {
        _isSelectorOpen = !_isSelectorOpen;
        crateSelectorPanel.gameObject.SetActive(_isSelectorOpen);

        if (_isFirstSelection && !_isSelectorOpen)
        {
            _isSelectorOpen = true;
            crateSelectorPanel.gameObject.SetActive(true);
        }
    }

    public void DisplayCrateSelector(List<CrateData> availableCrates)
    {
        foreach (Transform child in crateSelectorPanel)
        {
            Destroy(child.gameObject);
        }

        List<CrateData> orderedAvailableCrates = availableCrates.OrderBy(i => i.Price).ToList();

        foreach (var crateData in orderedAvailableCrates)
        {
            GameObject crateButton = Instantiate(crateButtonPrefab, crateSelectorPanel);
            Image crateImage = crateButton.transform.Find("CrateImage").GetComponent<Image>();
            TextMeshProUGUI nameText = crateButton.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = crateButton.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
            
            crateImage.sprite = Resources.Load<Sprite>($"CrateImages/{crateData.ID}");
            nameText.text = crateData.Name;
            priceText.text = $"{crateData.Price:F2}";

            Button button = crateButton.GetComponent<Button>();
            button.onClick.AddListener(() => SelectCrate(crateData));
        }
    }
}
