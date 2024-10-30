using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance { get; private set; }

    public GameObject collectionItemPrefab;
    public Transform collectionGrid;

    private List<ItemData> allItems = new List<ItemData>();
    private HashSet<string> collectedItemIDs = new HashSet<string>();
    private string SaveFileName
    {
        get
        {
            #if UNITY_EDITOR
                return "PlayTest_CollectionData.dat";
            #else
                return "CollectionData.dat";
            #endif
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadCollection();
    }

    void Start()
    {
        LoadAllGameItems();
        UpdateCollectionUI();
    }

    public void AddItemToCollection(ItemData item)
    {
        if (collectedItemIDs.Contains(item.ID)) return;

        collectedItemIDs.Add(item.ID);
        SaveCollection();
        UpdateCollectionUI();
    }

    private void SaveCollection()
    {   
        BinaryFormatter bf = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, "SaveData/" + SaveFileName);
        using (FileStream file = File.Create(path))
        {
            bf.Serialize(file, collectedItemIDs);
        }
    }

    private void LoadCollection()
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveData/" + SaveFileName);
        if (File.Exists(path))
        {
            using (FileStream file = File.Open(path, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                collectedItemIDs = (HashSet<string>)bf.Deserialize(file);
            }
        }
        UpdateCollectionUI();
    }

    private List<ItemData> LoadAllGameItems()
    {
        allItems = Resources.LoadAll<ItemData>("Items").ToList();
        return allItems;
    }

    public void ClearCollection()
    {
        collectedItemIDs.Clear();
        string path = Path.Combine(Application.persistentDataPath, "SaveData/" + SaveFileName);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
        
        SaveCollection();
    }

    public void UpdateCollectionUI()
    {
        if (collectionGrid == null) return;

        foreach (Transform child in collectionGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in LoadAllGameItems())
        {
            GameObject itemObj = Instantiate(collectionItemPrefab, collectionGrid);
            CollectionItem collectionItem = itemObj.GetComponent<CollectionItem>();

            bool isCollected = collectedItemIDs.Contains(item.ID);
            collectionItem.Setup(item, isCollected);
        }
    }
}
