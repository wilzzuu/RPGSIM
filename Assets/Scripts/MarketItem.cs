using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarketplaceItem : MonoBehaviour
{
    public Image itemImage;
    public Image rarityImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public Button sellButton;

    private ItemData itemData;

    private const float buyMarkup = 1.15f;
    private const float sellDiscount = 0.85f;

    public void Setup(ItemData item, bool isBuying)
    {
        itemData = item;
        
        itemImage.sprite = Resources.Load<Sprite>($"ItemImages/{itemData.ID}");
        rarityImage.sprite = Resources.Load<Sprite>($"RarityImages/{itemData.Rarity}");

        nameText.text = itemData.Name;
        priceText.text = isBuying
            ? $"{itemData.Price * buyMarkup:F2}"
            : $"{itemData.Price * sellDiscount:F2}";

        buyButton.gameObject.SetActive(isBuying);
        sellButton.gameObject.SetActive(!isBuying);

        if (isBuying)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => MarketManager.instance.BuyItem(itemData));
        }
        else
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => MarketManager.instance.SellItem(itemData, gameObject));
        }
    }
    
    public void UpdatePrice(bool isBuying)
    {
        priceText.text = isBuying
            ? $"{itemData.Price * buyMarkup:F2}"
            : $"{itemData.Price * sellDiscount:F2}";
    }
}
