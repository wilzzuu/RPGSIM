using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataSerializationManager : MonoBehaviour
{
    public static DataSerializationManager Instance { get; private set; }
    private const string SaveFileName = "gameData.dat";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Save the list of items in binary format
    public void SaveGameData(List<SerializableItemData> items)
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        using (FileStream file = File.Create(path))
        {
            bf.Serialize(file, items);
        }
        Debug.Log("Game data saved successfully to " + path);
    }

    // Load data from file
    public List<SerializableItemData> LoadGameData()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(path, FileMode.Open))
            {
                List<SerializableItemData> items = (List<SerializableItemData>)bf.Deserialize(file);
                Debug.Log("Game data loaded successfully from " + path);
                return items;
            }
        }
        else
        {
            Debug.LogWarning("Save file not found at " + path);
            return new List<SerializableItemData>(); // Return an empty list if no save file
        }
    }
}
