using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
public class JsonToScriptableObject : MonoBehaviour
{
    [MenuItem("Tools/Import JSON Data")]
    public static void ImportJsonData()
    {
        // Define the path to your JSON file in StreamingAssets
        string path = Application.dataPath + "/StreamingAssets/rpg_crate_data.json";

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            Debug.Log("JSON Content: " + jsonContent);

            try
            {
                // Parse JSON into CrateJsonDataWrapper
                CrateJsonDataWrapper crateDataWrapper = JsonUtility.FromJson<CrateJsonDataWrapper>(jsonContent);

                // Validate parsed data
                if (crateDataWrapper == null || crateDataWrapper.crates == null)
                {
                    Debug.LogError("Failed to parse JSON: Crate data is null or empty.");
                    return;
                }

                foreach (var crateData in crateDataWrapper.crates)
                {
                    Debug.Log($"Processing crate: {crateData.ID} | Name: {crateData.NAME} | Price: {crateData.PRICE}");
                    CreateCrateDataAsset(crateData);
                }

                Debug.Log("Data imported successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error parsing JSON: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("JSON file not found at path: " + path);
        }
    }

    private static void CreateCrateDataAsset(CrateJsonData jsonData)
    {
        // Create CrateData ScriptableObject
        CrateData crateAsset = ScriptableObject.CreateInstance<CrateData>();
        crateAsset.ID = jsonData.ID;
        crateAsset.Name = jsonData.NAME;
        crateAsset.Price = jsonData.PRICE;
        crateAsset.Items = new List<ItemData>();

        // Create ItemData assets and add them to the crate
        foreach (var itemJson in jsonData.items)
        {
            if (itemJson == null)
            {
                Debug.LogWarning("Item data is missing or null.");
                continue;
            }

            // Log each item for debugging
            Debug.Log($"Creating item: {itemJson.NAME} | Rarity: {itemJson.RARITY} | Price: {itemJson.PRICE}");

            // Create ItemData ScriptableObject
            ItemData itemAsset = ScriptableObject.CreateInstance<ItemData>();
            itemAsset.ID = itemJson.ID;
            itemAsset.Name = itemJson.NAME;
            itemAsset.Price = itemJson.PRICE;
            itemAsset.Rarity = itemJson.RARITY;
            itemAsset.Weight = itemJson.WEIGHT;
            itemAsset.Color = itemJson.COLOR;
            itemAsset.Item = itemJson.ITEM;
            itemAsset.Style = itemJson.STYLE;

            // Save each ItemData asset and add it to the CrateData
            SaveAsset(itemAsset, $"Assets/ScriptableObjects/Items/{itemAsset.ID}.asset");
            crateAsset.Items.Add(itemAsset);
        }

        // Save CrateData asset
        SaveAsset(crateAsset, $"Assets/ScriptableObjects/Crates/{jsonData.ID}.asset");
    }

    private static void SaveAsset(ScriptableObject asset, string path)
    {
        // Ensure the directory exists
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Create and save the asset
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
    }
}

// Wrapper class for the crates list
[System.Serializable]
public class CrateJsonDataWrapper
{
    public List<CrateJsonData> crates;
}

// Structure of Crate JSON data
[System.Serializable]
public class CrateJsonData
{
    public string ID;
    public string NAME;
    public float PRICE;
    public List<ItemJsonData> items;
}

// Structure of Item JSON data
[System.Serializable]
public class ItemJsonData
{
    public string ID;
    public string NAME;
    public string RARITY;
    public float PRICE;
    public int WEIGHT;
    public string COLOR;
    public string ITEM;
    public string STYLE;
}
