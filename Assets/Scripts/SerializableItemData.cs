[System.Serializable]
public class SerializableItemData
{
    public string ID;
    public string Name;
    public string Rarity;
    public float Price;
    public string Color;
    public string Type;
    public string Style;

    public SerializableItemData(ItemData item)
    {
        ID = item.ID;
        Name = item.Name;
        Rarity = item.Rarity;
        Price = item.Price;
        Color = item.Color;
        Type = item.Item;
        Style = item.Style;
    }
}