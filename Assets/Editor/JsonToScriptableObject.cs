using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class JsonToScriptableObject : MonoBehaviour
{
    [MenuItem("Tools/Import JSON Data")]
    public static void ImportJsonData()
    {
        
        string path = Application.dataPath + "/StreamingAssets/rpg_crate_data.json";

        if (File.Exists(path))
        {
            
            string jsonContent = File.ReadAllText(path);

            
            Debug.Log("JSON Content: " + jsonContent);

            
            try
            {
                
                CrateJsonDataWrapper crateDataWrapper = JsonUtility.FromJson<CrateJsonDataWrapper>(jsonContent);

                
                if (crateDataWrapper == null || crateDataWrapper.crates == null)
                {
                    Debug.LogError("Failed to parse JSON. The result is null.");
                    return;
                }

                
                foreach (var crateData in crateDataWrapper.crates)
                {
                    Debug.Log($"Parsing crate: {crateData.ID}");
                    Debug.Log($"Crate Name: {crateData.NAME}");
                    Debug.Log($"Crate Price: {crateData.PRICE}");
                    Debug.Log($"Number of items in the crate: {crateData.items.Count}");

                    
                    foreach (var item in crateData.items)
                    {
                        Debug.Log($"Item ID: {item.ID}");
                        Debug.Log($"Item Name: {item.NAME}");
                        Debug.Log($"Item Price: {item.PRICE}");
                        Debug.Log($"Item Rarity: {item.RARITY}");
                        Debug.Log($"Item Weight: {item.WEIGHT}");
                    }

                    CreateCrateDataAsset(crateData);
                }

                Debug.Log("Data imported successfully!");
            }
            catch (System.Exception ex)
            {
                
                Debug.LogError("An error occurred while parsing the JSON: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("JSON file not found at path: " + path);
        }
    }

    private static void CreateCrateDataAsset(CrateJsonData jsonData)
    {
        
        CrateData crateAsset = ScriptableObject.CreateInstance<CrateData>();

        
        crateAsset.ID = jsonData.ID;        
        crateAsset.Name = jsonData.NAME;    
        crateAsset.Price = jsonData.PRICE;
        crateAsset.Items = new List<ItemData>();

        
        foreach (var itemJson in jsonData.items)
        {
            ItemData itemAsset = ScriptableObject.CreateInstance<ItemData>();
            itemAsset.ID = itemJson.ID;
            itemAsset.Name = itemJson.NAME;
            itemAsset.Price = itemJson.PRICE;
            itemAsset.Rarity = itemJson.RARITY;
            itemAsset.Weight = itemJson.WEIGHT;

            
            SaveAsset(itemAsset, $"Assets/ScriptableObjects/Items/{itemAsset.ID}.asset");

            
            crateAsset.Items.Add(itemAsset);
        }

        
        SaveAsset(crateAsset, $"Assets/ScriptableObjects/Crates/{jsonData.ID}.asset");
    }

    private static void SaveAsset(ScriptableObject asset, string path)
    {
        
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
    }
}


[System.Serializable]
public class CrateJsonDataWrapper
{
    public List<CrateJsonData> crates; 
}

[System.Serializable]
public class CrateJsonData
{
    public string ID;  
    public string NAME; 
    public float PRICE;
    public List<ItemJsonData> items; 
}

[System.Serializable]
public class ItemJsonData
{
    public string ID;
    public string NAME;
    public string RARITY;
    public float PRICE;
    public int WEIGHT;
}