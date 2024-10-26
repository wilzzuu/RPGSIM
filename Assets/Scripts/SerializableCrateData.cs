using System.Collections.Generic;

[System.Serializable]
public class SerializableCrateData
{
    public string CrateID;
    public string CrateName;
    public float Price;
    public List<SerializableItemData> Items;

    public SerializableCrateData(CrateData crate)
    {
        CrateID = crate.ID;
        CrateName = crate.Name;
        Price = crate.Price;
        Items = new List<SerializableItemData>();

        foreach (var item in crate.Items)
        {
            Items.Add(new SerializableItemData(item));
        }
    }
}