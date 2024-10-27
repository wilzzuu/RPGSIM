using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgraderItem : MonoBehaviour
{
    public Image itemImage;
    public Image rarityImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button chooseButton;

    private ItemData itemData;

    // Setup function to initialize the item details
    public void Setup(ItemData item, bool isInventoryItem)
    {
        itemData = item;
        
        itemImage.sprite = Resources.Load<Sprite>($"ItemImages/{itemData.ID}");
        rarityImage.sprite = Resources.Load<Sprite>($"RarityImages/{itemData.Rarity}");
        nameText.text = itemData.Name;
        priceText.text = $"${itemData.Price:0.00}";

        if (isInventoryItem)
        {
            chooseButton.onClick.RemoveAllListeners();
            chooseButton.onClick.AddListener(() => UpgraderManager.instance.SelectInventoryItem(itemData));
        }
        else
        {
            chooseButton.onClick.RemoveAllListeners();
            chooseButton.onClick.AddListener(() => UpgraderManager.instance.SelectUpgradeItem(itemData));
        }
    }
}
