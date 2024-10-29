using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RouletteInventoryItem : MonoBehaviour
{
    public Image itemImage;
    public Image rarityImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button chooseButton;

    public TextMeshProUGUI buttonText;

    private ItemData itemData;

    public void Setup(ItemData item, bool isItemSelected)
    {
        itemData = item;
        
        itemImage.sprite = Resources.Load<Sprite>($"ItemImages/{itemData.ID}");
        rarityImage.sprite = Resources.Load<Sprite>($"RarityImages/{itemData.Rarity}");
        nameText.text = itemData.Name;
        priceText.text = $"{itemData.Price:0.00}";

        if (!isItemSelected)
        {
            chooseButton.onClick.RemoveAllListeners();
            chooseButton.onClick.AddListener(() => RouletteManager.Instance.AddItemToSelection(itemData));
            buttonText.text = "Select";
        }
        else
        {
            chooseButton.onClick.RemoveAllListeners();
            chooseButton.onClick.AddListener(() => RouletteManager.Instance.RemoveItemFromSelection(itemData));
            buttonText.text = "Unselect";
        }
    }
}
