using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI balance;


    void Start()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("PlayerManager does not exist in the current context.");
            return;
        }
        PlayerManager.Instance.onBalanceChanged += UpdateBalanceDisplay;

        UpdateBalanceDisplay();
    }

    void OnDestroy()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.onBalanceChanged -= UpdateBalanceDisplay;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        gameObject.SetActive(scene.name != "MainMenu");
    }

    void UpdateBalanceDisplay()
    {
        balance.text = $"{PlayerManager.Instance.GetPlayerBalance()}€";
    }
}
