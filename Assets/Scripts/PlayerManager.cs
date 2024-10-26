using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
    private const string SaveFileName = "PlayerData.dat";

    public delegate void BalanceChangedHandler();
    public event BalanceChangedHandler onBalanceChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LoadPlayerData();
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

    // Save player data using binary serialization
    public void SavePlayerData()
    {
        PlayerData data = new PlayerData(this);

        BinaryFormatter bf = new BinaryFormatter();
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);

        using (FileStream file = File.Create(path))
        {
            bf.Serialize(file, data);
        }

        Debug.Log("Player data saved to " + path);
    }

    // Load player data using binary serialization
    public void LoadPlayerData()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);

        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(path, FileMode.Open))
            {
                PlayerData data = (PlayerData)bf.Deserialize(file);
                player = new Player(data.balance);
            }
            Debug.Log("Player data loaded from " + path);
        }
        else
        {
            // Initialize with a default balance if no save file exists
            player = new Player(500f);
            Debug.Log("No save file found. Initialized player with default balance.");
        }
    }

    public void ResetProgress()
    {
        player.balance = 500f;
        SavePlayerData();
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
