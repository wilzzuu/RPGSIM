using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class RouletteManager : MonoBehaviour
{
    public static RouletteManager Instance { get; private set; }
    public GameObject rouletteItemPrefab;
    public GameObject inventoryItemPrefab;
    public GameObject itemSelectorView, gameView;
    public Transform reelContainer;
    public Transform inventoryCatalogGrid;
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

    private float totalPlayerValue = 0;
    private float totalGameValue = 0;
    private float winChance = 0;
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
        if (selectedPlayerItems.Contains(item) || selectedPlayerItems.Count >= 10)
        {
            Debug.LogWarning("Cannot select more than 10 items or duplicate items.");
            return;
        }
        
        selectedPlayerItems.Add(item);
        totalPlayerValue += item.Price;
        selectedPlayerItemsTotalValue.text = $"Total Value: {totalPlayerValue:F2}";
    }

    public void RemoveItemFromSelection(ItemData item)
    {
        if (selectedPlayerItems.Remove(item))
        {
            totalPlayerValue -= item.Price;
            selectedPlayerItemsTotalValue.text = $"Total Value: {totalPlayerValue:F2}";
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

        initialReelPosition = new Vector3(totalReelWidth / 2 - reelContainer.localScale.x / 2, reelContainer.localPosition.y, reelContainer.localPosition.z);
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
        float randomValue = Random.Range(0, totalWeight);
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

        winningItemIndex = Random.Range(reelItems.Count / 2, reelItems.Count - 4);

        for (int i = 0; i < reelItems.Count; i++)
        {
            RouletteItem itemData = (i == winningItemIndex) ? winningItem : GetRandomItemByChance();
            GameObject reelItem = Instantiate(rouletteItemPrefab, reelContainer);
            SetUpReelItem(reelItem, itemData.Item, itemOwnership[itemData.Item]);
        }

        randomOffset = Random.Range(0f, 160f);
        float targetPosition = initialReelPosition.x - (itemWidth * winningItemIndex) - randomOffset;
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

        float totalUniqueBotValue = reelItems
            .Where(item => !item.OwnerIsPlayer)
            .Select(item => item.Item)
            .Distinct()
            .Sum(item => item.Price);

        totalGameValue = totalPlayerItemValue + totalUniqueBotValue;

        int playerItemCount = selectedPlayerItems.Count;
        int reelItemCount = reelItems.Count;

        if (reelItemCount > 0)
        {
            winChance = (float)playerItemCount / reelItemCount * 100f;
        }
        else
        {
            winChance = 0f;
        }

        chanceOfWinningText.text = $"Win Chance: {winChance:F2}%";
        totalValueText.text = $"Total Game Value: {totalGameValue:F2}";
        outcomeText.text = "";
    }

    void GenerateBotsAndPopulateReel()
    {
        reelItems.Clear();
        itemOwnership.Clear();

        foreach (var playerItem in selectedPlayerItems)
        {
            reelItems.Add(new RouletteItem { Item = playerItem, OwnerIsPlayer = true });
            itemOwnership[playerItem] = true;
        }

        int playerItemCount = selectedPlayerItems.Count;
        int botItemsCount = Mathf.Clamp(playerItemCount * 5, 1, numberOfReelItems - playerItemCount);

        float botItemTargetValue = totalPlayerValue + Random.Range(-totalPlayerValue * 1f, totalPlayerValue * 1f);
        for (int i = 0; i < botItemsCount; i++)
        {
            var botItem = GenerateRandomBotItem(botItemTargetValue / botItemsCount);
            reelItems.Add(new RouletteItem { Item = botItem, OwnerIsPlayer = false });
            itemOwnership[botItem] = false;
        }

        int currentReelSize = reelItems.Count;
        int targetReelSize = Mathf.Max(numberOfReelItems, 40);
        while (reelItems.Count < targetReelSize)
        {
            reelItems.Add(reelItems[reelItems.Count % currentReelSize]);
        }

        ShuffleReelItems();
        UpdateUI();
    }

    void ShuffleReelItems()
    {
        for (int i = 0; i < reelItems.Count; i++)
        {
            int randomIndex = Random.Range(i, reelItems.Count);
            RouletteItem temp = reelItems[i];
            reelItems[i] = reelItems[randomIndex];
            reelItems[randomIndex] = temp;
        }
    }

    private ItemData GenerateRandomBotItem(float targetValue)
    {
        if (allItems == null || allItems.Count == 0)
        {
            allItems = Resources.LoadAll<ItemData>("Items").ToList();
        }

        float maxVariance = 5.0f;
        float priceVariance = Mathf.Min(targetValue * 0.3f, maxVariance);
        
        List<ItemData> eligibleItems = allItems.Where(item => 
            item.Price >= targetValue - priceVariance && 
            item.Price <= targetValue + priceVariance).ToList();

        if (eligibleItems.Count == 0)
        {
            eligibleItems = allItems
                .OrderBy(item => Mathf.Abs(item.Price - targetValue))
                .Take(10)
                .ToList();
        }
        return eligibleItems[Random.Range(0, eligibleItems.Count)];
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
        PopulateInventoryCatalog();
        selectedPlayerItemsTotalValue.text = "Total Value: 0";
        gameView.SetActive(false);
    }

    void BackToSelection()
    {
        RestartGame();
    }
}
