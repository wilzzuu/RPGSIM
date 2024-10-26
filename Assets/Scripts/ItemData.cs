using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item Data", order = 51)]
public class ItemData : ScriptableObject
{
    public string ID;
    public string Name;
    public string Rarity;
    public float Price;
    public int Weight;
    public string Color;
    public string Item;
    public string Style;
}
