using UnityEngine;

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
    private const string SaveKey = "PlayerData";

    public delegate void balanceChangedHandler();
    public event balanceChangedHandler onBalanceChanged;

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

    
    public void LoadBalance()
    {
        
       player.balance = PlayerPrefs.GetFloat("PlayerBalance", 0); 
    }

    public float GetPlayerBalance()
    {
        return player.balance;
    }

    public void SavePlayerData()
    {
        PlayerData data = new PlayerData(this);
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            player.balance = data.balance;
        }
        else
        {
            player = new Player(100f);
        }
    }

    public void ResetProgress()
    {
        player.balance = 100f;
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