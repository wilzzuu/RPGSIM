using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item Data", order = 51)]
public class ItemData : ScriptableObject
{
    public string ID;
    public string Name;
    public string Rarity;
    public float BasePrice;
    public float Price;
    public int DemandScore;
    public int Weight;
    public string Color;
    public string Item;
    public string Style;

    private void OnEnable()
    {
        DemandScore = 0;
    }
}
