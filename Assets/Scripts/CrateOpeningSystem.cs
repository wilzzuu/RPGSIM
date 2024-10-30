using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

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
    
    private bool isScrolling = false;
    private Vector3 initialReelPosition;
    private GridLayoutGroup gridLayout;
    private RectTransform reelTransform;
    private int openedItemIndex;
    private float randomOffset;

    public GameObject crateButtonPrefab;
    public Transform crateSelectorPanel;
    public Button selectCrateButton;
    public Button openCrateButton;
    private bool isSelectorOpen = false;
    private bool isFirstSelection = true;
    private List<CrateData> availableCrates;
    private CrateData selectedCrateData;

    public UIManager uiManager;

    private static readonly Dictionary<string, int> rarityOrder = new Dictionary<string, int>
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
        availableCrates = new List<CrateData>(Resources.LoadAll<CrateData>("Crates"));

        DisplayCrateSelector(availableCrates);
        selectCrateButton.onClick.AddListener(ToggleCrateSelector);

        gridLayout = crateGridParent.GetComponent<GridLayoutGroup>();
        reelTransform = crateGridParent.GetComponent<RectTransform>();

        SetInitialReelPosition();
    }

    private void SelectCrate(CrateData chosenCrate)
    {
        selectedCrateData = chosenCrate;

        if (selectedCrateData != null) openCrateButton.interactable = true;
        else openCrateButton.interactable = false;

        if (selectedCrateData.Price <= PlayerManager.Instance.GetPlayerBalance()) openCrateButton.interactable = true;
        else openCrateButton.interactable = false;
        
        DisplayCrateItems(selectedCrateData);

        if (isFirstSelection)
        {
            isFirstSelection = false;
            isSelectorOpen = false;
            crateSelectorPanel.gameObject.SetActive(false);
        }
        else
        {
            ToggleCrateSelector();
        }
    }

    private void SetInitialReelPosition()
    {
        float itemWidth = gridLayout.cellSize.x + gridLayout.spacing.x;
        float totalReelWidth = itemWidth * numberOfReelItems;
        
        initialReelPosition = new Vector3(totalReelWidth / 2 - 800, crateGridParent.localPosition.y, crateGridParent.localPosition.z);
        crateGridParent.localPosition = initialReelPosition;
    }

    public void OpenCrate()
    {
        if (isScrolling) return;

        if (PlayerManager.Instance == null || selectedCrateData == null)
        {
            Debug.LogError("PlayerManager or selectedCrateData is null");
            return;
        }

        if (selectedCrateData == null)
        {
            Debug.LogError("No crate selected. Plerate select a crate first.");
            return;
        }

        foreach (Transform child in crateGridParent) 
        {
            Destroy(child.gameObject);
        }

        crateGridParent.localPosition = initialReelPosition;

        if (PlayerManager.Instance.GetPlayerBalance() >= selectedCrateData.Price)
        {
            PlayerManager.Instance.DeductCurrency(selectedCrateData.Price);
            ItemData openedItem = GetRandomItemByPercentage();
            StartCoroutine(AnimateScrollingReel(openedItem));
        }
        else
        {
            Debug.LogError("Not enough money to open this crate.");
            return;
        }
    }

    private ItemData GetRandomItemByPercentage()
    {
        if (selectedCrateData.Items == null || selectedCrateData.Items.Count == 0)
        {
            Debug.LogError("No items in the selected crate. Ensure the selected crate has items.");
            return null; 
        }

        float totalWeight = 0f;
        foreach (var item in selectedCrateData.Items)
        {
            totalWeight += item.Weight;  
        }

        
        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var Item in selectedCrateData.Items)
        {
            cumulativeWeight += Item.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return Item;
            }
        }

        Debug.LogWarning("No item was selected; returning default item.");
        return selectedCrateData.Items[selectedCrateData.Items.Count - 1];
    }

    private IEnumerator AnimateScrollingReel(ItemData openedItem)
    {
        isScrolling = true;
        openCrateButton.interactable = false;
        selectCrateButton.interactable = false;
        uiManager.LockUI();

        List<GameObject> reelItems = new List<GameObject>();
        float itemWidth = gridLayout.cellSize.x + gridLayout.spacing.x;
        float totalReelWidth = itemWidth * numberOfReelItems;
        reelTransform.sizeDelta = new Vector2(totalReelWidth, reelTransform.sizeDelta.y);

        openedItemIndex = Random.Range(24, numberOfReelItems - 4);
        for (int i = 0; i < numberOfReelItems; i++)
        {
            ItemData itemData = (i == openedItemIndex) ? openedItem : GetRandomItemByPercentage();
            GameObject reelItem = Instantiate(openedItemPrefab, crateGridParent);
            SetUpReelItem(reelItem, itemData);
            reelItems.Add(reelItem);
        }

        randomOffset = Random.Range(0f, 256f);
        float targetPosition = initialReelPosition.x - (itemWidth * openedItemIndex) + 800 - randomOffset;

        float elapsed = 0f;

        while (elapsed < easingDuration)
        {
            float t = elapsed / easingDuration;
            float erateFactor = ErateOutQuint(t);

            float currentPosition = Mathf.Lerp(initialReelPosition.x, targetPosition, erateFactor);
            crateGridParent.localPosition = new Vector3(currentPosition, crateGridParent.localPosition.y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        crateGridParent.localPosition = new Vector3(targetPosition, crateGridParent.localPosition.y, 0);

        StopReel(openedItem);
        isScrolling = false;
        selectCrateButton.interactable = true;
        uiManager.UnlockUI();
    }

    private void StopReel(ItemData openedItem)
    {
        InventoryManager.Instance.AddItemToInventory(openedItem);
        if (PlayerManager.Instance.GetPlayerBalance() < selectedCrateData.Price)
        {
            DisplayCrateSelector(availableCrates);
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

        selectedCrate.Items = selectedCrate.Items.OrderBy(item => rarityOrder[item.Rarity]).ToList();

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
        isSelectorOpen = !isSelectorOpen;
        crateSelectorPanel.gameObject.SetActive(isSelectorOpen);

        if (isFirstSelection && !isSelectorOpen)
        {
            isSelectorOpen = true;
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

        foreach (var CrateData in orderedAvailableCrates)
        {
            GameObject crateButton = Instantiate(crateButtonPrefab, crateSelectorPanel);
            Image crateImage = crateButton.transform.Find("CrateImage").GetComponent<Image>();
            TextMeshProUGUI nameText = crateButton.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = crateButton.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
            
            crateImage.sprite = Resources.Load<Sprite>($"CrateImages/{CrateData.ID}");
            nameText.text = CrateData.Name;
            priceText.text = $"{CrateData.Price:F2}";

            Button button = crateButton.GetComponent<Button>();
            button.onClick.AddListener(() => SelectCrate(CrateData));
        }
    }
}
