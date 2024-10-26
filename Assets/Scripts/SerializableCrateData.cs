using System.Collections.Generic;

[System.Serializable]
public class SerializableCrateData
{
    public string ID;
    public string Name;
    public float Price;
    public List<SerializableItemData> Items;

    public SerializableCrateData(CrateData crate)
    {
        ID = crate.ID;
        Name = crate.Name;
        Price = crate.Price;
        Items = new List<SerializableItemData>();

        foreach (var item in crate.Items)
        {
            Items.Add(new SerializableItemData(item));
        }
    }
}