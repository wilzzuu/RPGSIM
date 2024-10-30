using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Unity.Mathematics;

public class RouletteManager : MonoBehaviour
{
    public static RouletteManager Instance { get; private set; }
    public GameObject rouletteItemPrefab;
    public GameObject inventoryItemPrefab;
    public GameObject itemSelectorView, gameView;
    public Transform reelContainer;
    public Transform inventoryCatalogGrid;
    public Transform selectedItemsGrid;
    public TextMeshProUGUI selectedPlayerItemsTotalValue, chanceOfWinningText, totalValueText, outcomeText;
    public Button startGameButton, confirmSelectionButton, replayButton, backButton;

    private List<ItemData> allItems = new List<ItemData>();
    private List<ItemData> selectedPlayerItems = new List<ItemData>();
    private List<RouletteItem> reelItems = new List<RouletteItem>();
    private Dictionary<ItemData, bool> itemOwnership = new Dictionary<ItemData, bool>();

    public float initialScrollSpeed = 4000f;
    public float finalScrollSpeed = 100f;
    public int numberOfReelItems = 60;
    public float easingDuration = 7f;
    private GridLayoutGroup gridLayout;
    private RectTransform reelTransform;
    private int winningItemIndex;
    private float randomOffset;
    private bool isScrolling = false;
    private Vector3 initialReelPosition;

    private float accumulatedBotValue = 0f;
    private float totalPlayerValue = 0f;
    private float totalGameValue = 0f;
    private float winChance = 0f;
    public UIManager uiManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        confirmSelectionButton.onClick.AddListener(ConfirmSelection);
        startGameButton.onClick.AddListener(StartGame);
        replayButton.onClick.AddListener(RestartGame);
        backButton.onClick.AddListener(BackToSelection);

        gridLayout = reelContainer.GetComponent<GridLayoutGroup>();
        reelTransform = reelContainer.GetComponent<RectTransform>();

        itemSelectorView.SetActive(true);
        gameView.SetActive(false);

        PopulateInventoryCatalog();
    }

    private void PopulateInventoryCatalog()
    {
        foreach (Transform child in inventoryCatalogGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in InventoryManager.Instance.GetAllItems())
        {
            GameObject itemObj = Instantiate(inventoryItemPrefab, inventoryCatalogGrid);
            itemObj.GetComponent<RouletteInventoryItem>().Setup(item, selectedPlayerItems.Contains(item));
        }
    }

    public void AddItemToSelection(ItemData item)
    {
        if (selectedPlayerItems.Contains(item) || selectedPlayerItems.Count >= 12)
        {
            Debug.LogWarning("Cannot select more than 10 items or duplicate items.");
            return;
        }

        selectedPlayerItems.Add(item);
        totalPlayerValue += item.Price;
        selectedPlayerItemsTotalValue.text = $"Total Value: {totalPlayerValue:F2}";

        UpdateSelectedItemsGrid();
    }

    public void RemoveItemFromSelection(ItemData item)
    {
        if (selectedPlayerItems.Remove(item))
        {
            totalPlayerValue -= item.Price;
            selectedPlayerItemsTotalValue.text = $"Total Value: {totalPlayerValue:F2}";
        }

        UpdateSelectedItemsGrid();
    }

    private void UpdateSelectedItemsGrid()
    {
        foreach (Transform child in selectedItemsGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in selectedPlayerItems)
        {
            GameObject itemObj = Instantiate(inventoryItemPrefab, selectedItemsGrid);
            itemObj.GetComponent<RouletteInventoryItem>().Setup(item, true);
        }
    }

    void ConfirmSelection()
    {
        if (selectedPlayerItems.Count < 1)
        {
            Debug.LogWarning("Select at least one item to start.");
            return;
        }

        GenerateBotsAndPopulateReel();
        SetInitialReelPosition();
        UpdateUI();
        itemSelectorView.SetActive(false);
        gameView.SetActive(true);
    }

    private void SetInitialReelPosition()
    {
        float itemWidth = gridLayout.cellSize.x + gridLayout.spacing.x;
        float totalReelWidth = itemWidth * reelItems.Count;

        initialReelPosition = new Vector3(totalReelWidth / 2 - 800, reelContainer.localPosition.y, reelContainer.localPosition.z);
        reelContainer.localPosition = initialReelPosition;
    }

    void StartGame()
    {
        if (isScrolling || selectedPlayerItems.Count == 0 || reelItems.Count == 0) return;

        foreach (Transform child in reelContainer)
        {
            Destroy(child.gameObject);
        }

        reelContainer.localPosition = initialReelPosition;
        RouletteItem winningItem = GetRandomItemByChance();
        StartCoroutine(AnimateScrollingReel(winningItem));
    }

    private RouletteItem GetRandomItemByChance()
    {
        if (reelItems == null || reelItems.Count == 0)
        {
            Debug.LogWarning("Reel items are empty. Cannot get a random item.");
            return null;
        }

        float totalWeight = reelItems.Sum(i => i.Item.Weight);
        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var item in reelItems)
        {
            cumulativeWeight += item.Item.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return item;
            }
        }
        return reelItems[reelItems.Count - 1];
    }

    private IEnumerator AnimateScrollingReel(RouletteItem winningItem)
    {
        isScrolling = true;
        uiManager.LockUI();
        startGameButton.interactable = false;
        replayButton.interactable = false;
        backButton.interactable = false;

        float itemWidth = gridLayout.cellSize.x + gridLayout.spacing.x;
        float totalReelWidth = itemWidth * reelItems.Count;
        reelTransform.sizeDelta = new Vector2(totalReelWidth, reelTransform.sizeDelta.y);

        winningItemIndex = UnityEngine.Random.Range(reelItems.Count / 2, reelItems.Count - 4);
        reelItems[winningItemIndex] = winningItem;

        for (int i = 0; i < reelItems.Count; i++)
        {
            var itemData = reelItems[i];
            GameObject reelItem = Instantiate(rouletteItemPrefab, reelContainer);
            SetUpReelItem(reelItem, itemData.Item, itemData.OwnerIsPlayer);
        }

        randomOffset = UnityEngine.Random.Range(0f, 256f);
        float targetPosition = initialReelPosition.x - (itemWidth * winningItemIndex) + 800 - randomOffset;
        float elapsed = 0f;

        while (elapsed < easingDuration)
        {
            float t = elapsed / easingDuration;
            float erateFactor = ErateOutQuint(t);

            float currentPosition = Mathf.Lerp(initialReelPosition.x, targetPosition, erateFactor);
            reelContainer.localPosition = new Vector3(currentPosition, reelContainer.localPosition.y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        reelContainer.localPosition = new Vector3(targetPosition, reelContainer.localPosition.y, 0);
        EvaluateOutcome(winningItem);
        isScrolling = false;
        uiManager.UnlockUI();
        startGameButton.interactable = false;
        replayButton.interactable = true;
        backButton.interactable = false;
    }

    private void EvaluateOutcome(RouletteItem winningItem)
    {
        int winningIndex = reelItems.FindIndex(item => item.Item == winningItem.Item);

        Debug.Log("winningItem.Item.ID: " + winningItem.Item.ID);
        Debug.Log($"Winning item owner: {(winningItem.OwnerIsPlayer ? "Player":"Bot" )}");

        if (itemOwnership[winningItem.Item])
        {
            HashSet<ItemData> uniqueItemsToAdd = new HashSet<ItemData>();

            foreach (var item in reelItems)
            {
                uniqueItemsToAdd.Add(item.Item);
            }

            foreach (var uniqueItem in uniqueItemsToAdd)
            {
                InventoryManager.Instance.AddItemToInventory(uniqueItem);
            }
            outcomeText.text = "You won!";
        }
        else
        {
            foreach (var item in selectedPlayerItems)
            {
                InventoryManager.Instance.RemoveItemFromInventory(item);
            }
            outcomeText.text = "You lost!";
        }
    }

    private void SetUpReelItem(GameObject reelItem, ItemData itemData, bool isPlayerOwned)
    {
        Image itemImage = reelItem.transform.Find("ItemImage").GetComponent<Image>();
        Image rarityImage = reelItem.transform.Find("RarityImage").GetComponent<Image>();
        TextMeshProUGUI ownerText = reelItem.transform.Find("OwnerText").GetComponent<TextMeshProUGUI>();

        itemImage.sprite = Resources.Load<Sprite>($"ItemImages/{itemData.ID}");
        rarityImage.sprite = Resources.Load<Sprite>($"RarityImages/{itemData.Rarity}");

        ownerText.text = isPlayerOwned ? "Player" : "Bot";
        ownerText.color = isPlayerOwned ? Color.green : Color.red;
    }

    private float ErateOutQuint(float t)
    {
        return 1 - Mathf.Pow(1 - t, 5);
    }

    void UpdateUI()
    {
        float totalPlayerItemValue = selectedPlayerItems.Sum(item => item.Price);
        float totalUniqueBotValue = accumulatedBotValue;

        totalGameValue = totalPlayerItemValue + totalUniqueBotValue;

        int playerItemCount = selectedPlayerItems.Count;
        int reelItemCount = reelItems.Count;

        winChance = reelItemCount > 0 ? (float)playerItemCount / reelItemCount * 100f : 0f;

        chanceOfWinningText.text = $"Win Chance: {winChance:F2}%";
        totalValueText.text = $"Total Game Value: {totalGameValue:F2}";
        outcomeText.text = "";
    }

    void GenerateBotsAndPopulateReel()
    {
        // Clear existing items
        reelItems.Clear();
        itemOwnership.Clear();
        accumulatedBotValue = 0f;

        // Step 1: Add each player item to the reel exactly once
        foreach (var playerItem in selectedPlayerItems)
        {
            var playerRouletteItem = new RouletteItem { Item = playerItem, OwnerIsPlayer = true };
            reelItems.Add(playerRouletteItem);
            itemOwnership[playerItem] = true;
        }

        int playerItemCount = selectedPlayerItems.Count;
        int botItemsCount = Mathf.Clamp(playerItemCount * 5, 1, numberOfReelItems - playerItemCount);

        float scalingFactor = (float)botItemsCount / playerItemCount;
        float botItemTargetValue = totalPlayerValue * scalingFactor;
        HashSet<ItemData> usedBotItems = new HashSet<ItemData>();

        // Step 2: Generate unique bot items and add them to the reel
        for (int i = 0; i < botItemsCount; i++)
        {
            var botItem = GenerateRandomBotItem(botItemTargetValue / botItemsCount, usedBotItems);
            var botRouletteItem = new RouletteItem { Item = botItem, OwnerIsPlayer = false };
            reelItems.Add(botRouletteItem);
            itemOwnership[botItem] = false;
            accumulatedBotValue += botItem.Price;
        }

        int targetReelSize = Mathf.Max(numberOfReelItems, 40);
        int currentReelSize = reelItems.Count;

        // Step 3: Duplicate bot items until we reach the target reel size
        for (int i = 0; reelItems.Count < targetReelSize; i++)
        {
            // Use modulo to repeat bot items if necessary, avoiding modification of player items
            var botItemToAdd = reelItems[(i % botItemsCount) + playerItemCount];
            reelItems.Add(new RouletteItem { Item = botItemToAdd.Item, OwnerIsPlayer = botItemToAdd.OwnerIsPlayer });
        }

        // Step 4: Shuffle the entire reel including both player and bot items
        ShuffleReelItems();

        // Step 5: Confirm item counts for testing
        int actualPlayerItemCount = reelItems.Count(item => item.OwnerIsPlayer);
        Debug.Log("Expected Player Item Count: " + playerItemCount);
        Debug.Log("Actual Player Item Count in Reel: " + actualPlayerItemCount);

        UpdateUI();
    }


    void ShuffleReelItems()
    {
        for (int i = 0; i < reelItems.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, reelItems.Count);
            RouletteItem temp = reelItems[i];
            reelItems[i] = reelItems[randomIndex];
            reelItems[randomIndex] = temp;
        }
    }

    private ItemData GenerateRandomBotItem(float targetValue, HashSet<ItemData> usedBotItems)
    {
        if (allItems == null || allItems.Count == 0)
        {
            allItems = Resources.LoadAll<ItemData>("Items").ToList();
        }

        float maxVariance = 5.0f;
        float priceVariance = Mathf.Min(targetValue * 0.3f, maxVariance);

        List<ItemData> eligibleItems = allItems
            .Where(item => item.Price >= targetValue - priceVariance && item.Price <= targetValue + priceVariance)
            .Where(item => !usedBotItems.Contains(item))
            .ToList();

        if (eligibleItems.Count < 5)
        {
            eligibleItems = allItems
                .Where(item => !usedBotItems.Contains(item))
                .OrderBy(item => Mathf.Abs(item.Price - targetValue))
                .Take(15)
                .ToList();
        }

        ItemData selectedItem = eligibleItems[UnityEngine.Random.Range(0, eligibleItems.Count)];
        usedBotItems.Add(selectedItem);
        return selectedItem;
    }

    void RestartGame()
    {
        selectedPlayerItems.Clear();
        reelItems.Clear();
        itemOwnership.Clear();

        foreach (Transform child in reelContainer)
        {
            Destroy(child.gameObject);
        }

        totalPlayerValue = 0f;
        totalGameValue = 0f;
        winChance = 0f;
        startGameButton.interactable = true;
        backButton.interactable = true;
        itemSelectorView.SetActive(true);
        UpdateSelectedItemsGrid();
        PopulateInventoryCatalog();
        selectedPlayerItemsTotalValue.text = "Total Value: 0";
        gameView.SetActive(false);
    }

    void BackToSelection()
    {
        RestartGame();
    }
}
