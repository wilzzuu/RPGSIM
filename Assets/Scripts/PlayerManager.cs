using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

[System.Serializable]
public class Player
{
    public float balance;

    public Player(float initialBalance)
    {
        balance = initialBalance;
    }
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public Player player;
    private string SaveFileName
    {
        get
        {
            #if UNITY_EDITOR
                return "PlayTest_PlayerData.dat";
            #else
                return "PlayerData.dat";
            #endif
        }
    }

    public delegate void BalanceChangedHandler();
    public event BalanceChangedHandler onBalanceChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitializePlayerAndInventory());
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private IEnumerator InitializePlayerAndInventory()
    {
        while (InventoryManager.Instance == null)
        {
            yield return null;
        }

        CheckPlayerSave();
    }

    public void AddCurrency(float amount)
    {
        player.balance += amount;
        onBalanceChanged?.Invoke();
        SavePlayerData();
    }

    public void DeductCurrency(float amount)
    {
        if (player.balance >= amount)
        {
            player.balance -= amount;
            onBalanceChanged?.Invoke();
            SavePlayerData();
        }
        else
        {
            Debug.Log("Not enough money");
        }
    }

    public float GetPlayerBalance()
    {
        return player.balance;
    }

    void CheckPlayerSave()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning("Player save file missing or tampered with. Clearing inventory as a security measure.");
            InventoryManager.Instance.ClearInventory();

            ResetProgress();
        }
        else 
        {
            LoadPlayerData();
        }
    }

    public void SavePlayerData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        using (FileStream file = File.Create(path))
        {
            bf.Serialize(file, player);
        }

        Debug.Log("Player data saved to " + path);
    }

    public void LoadPlayerData()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);

        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(path, FileMode.Open))
            {
                player = (Player)bf.Deserialize(file);
            }
            Debug.Log("Player data loaded from " + path);
        }
        else
        {
            player = new Player(2000f);
            Debug.Log("No save file found. Initialized player with default balance.");
        }
    }

    public void ResetProgress()
    {
        player = new Player(2000f);
        SavePlayerData();
        InventoryManager.Instance.ClearInventory();
    }
}

[System.Serializable]
public class PlayerData
{
    public float balance;

    public PlayerData(PlayerManager playerManager)
    {
        balance = playerManager.GetPlayerBalance();
    }
}
