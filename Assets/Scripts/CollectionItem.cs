using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionItem : MonoBehaviour
{
    public Image itemImage;
    public Image rarityImage;
    public TextMeshProUGUI itemNameText;

    public Material grayscaleMaterial;
    private Material originalMaterial;

    public void Setup(ItemData item, bool isCollected)
    {
        itemImage.sprite = Resources.Load<Sprite>($"ItemImages/{item.ID}");
        rarityImage.sprite = Resources.Load<Sprite>($"RarityImages/{item.Rarity}");
        itemNameText.text = item.Name;

        if (originalMaterial == null)
        {
            originalMaterial = itemImage.material;
        }

        if (!isCollected)
        {
            itemImage.material = grayscaleMaterial;
            itemImage.material.SetColor("_Color", Color.gray);
        }
        else
        {
            itemImage.material = originalMaterial;
        }
        
        //itemImage.material = isCollected ? grayscaleMaterial : originalMaterial;
        //itemImage.SetMaterialDirty();
    }
}
