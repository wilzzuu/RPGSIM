using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New CrateData", menuName = "Crate Data", order = 51)]
public class CrateData : ScriptableObject
{
    public string ID;
    public string Name;
    public float Price;
    public List<ItemData> Items;
}
