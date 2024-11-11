using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI balance;

    [SerializeField] private List<Button> buttonList = new List<Button>();

    private bool isUILocked = false;

    void Start()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("PlayerManager does not exist in the current context.");
            return;
        }
        PlayerManager.Instance.OnBalanceChanged += UpdateBalanceDisplay;

        UpdateBalanceDisplay();
    }

    void OnDestroy()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnBalanceChanged -= UpdateBalanceDisplay;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        gameObject.SetActive(scene.name != "MainMenu");
    }

    void UpdateBalanceDisplay()
    {
        float balance_preformat = PlayerManager.Instance.GetPlayerBalance();
        string formatted_balance = balance_preformat.ToString("F2");
        balance.text = formatted_balance;
    }

    public void LockButton(Button button)
    {
        button.interactable = false;   
    }

    public void UnlockButton(Button button)
    {
        button.interactable = true;   
    }

    public void LockUI()
    {
        if (!isUILocked)
        {
            foreach (Button button in buttonList)
            {
                LockButton(button);
            }
            isUILocked = true;
        }
    }

    public void UnlockUI()
    {
        if (isUILocked)
        {
            foreach (Button button in buttonList)
            {
                UnlockButton(button);
            }
        }
        isUILocked = false;
    }
}
