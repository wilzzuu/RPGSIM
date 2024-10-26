using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;

public class CrateDataImporter : MonoBehaviour
{
    [System.Serializable]
    public class ItemDataJson
    {
        public string id;
        public string name;
        public float price;
        public string rarity;
        public string weight;
    }

    [System.Serializable]
    public class CrateDataJson
    {
        public string id;
        public string name;
        public float price;
        public List<ItemDataJson> items;
    }

    [System.Serializable]
    public class CrateDataJsonWrapper
    {
        public List<CrateDataJson> crates;
    }

    [MenuItem("Tools/Import Crate Data from JSON")]
    public static void ImportCrateData()
    {
        
        string jsonPath = Application.dataPath + "/StreamingAssets/rpg_crate_data.json";
        if (File.Exists(jsonPath))
        {
            string jsonData = File.ReadAllText(jsonPath);

            try
            {
                CrateDataJsonWrapper crateDataWrapper = JsonUtility.FromJson<CrateDataJsonWrapper>(jsonData);
                if (crateDataWrapper == null || crateDataWrapper.crates == null)
                {
                    Debug.LogError("Failed to parse JSON data.");
                    return;
                }

                string crateFolder = "Assets/ScriptableObjects/Crates";
                string itemFolder = "Assets/ScriptableObjects/Items";

                if (!AssetDatabase.IsValidFolder(crateFolder)) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Crates");
                if (!AssetDatabase.IsValidFolder(itemFolder)) AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Items");

                foreach (CrateDataJson crateJson in crateDataWrapper.crates)
                {
                    
                    CrateData crateData = ScriptableObject.CreateInstance<CrateData>();
                    crateData.ID = crateJson.id;
                    crateData.Name = crateJson.name;
                    crateData.Price = crateJson.price;
                    crateData.Items = new List<ItemData>();

                    foreach (ItemDataJson itemJson in crateJson.items)
                    {
                        
                        ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
                        itemData.ID = itemJson.id;
                        itemData.Name = itemJson.name;
                        itemData.Price = itemJson.price;
                        itemData.Rarity = itemJson.rarity;
                        itemData.Weight = Int32.Parse(itemJson.weight);
                        
                        string itemAssetPath = Path.Combine(itemFolder, $"{itemData.ID}.asset");
                        AssetDatabase.CreateAsset(itemData, itemAssetPath);

                        
                        crateData.Items.Add(itemData);
                    }

                    
                    string crateAssetPath = Path.Combine(crateFolder, $"{crateData.ID}.asset");
                    AssetDatabase.CreateAsset(crateData, crateAssetPath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Crate and Item data imported successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("An error occurred while parsing the JSON: " + ex.Message);
            }   
        }
        else
        {
            Debug.LogError("JSON file not found at path: " + jsonPath);
        }
    }
}
