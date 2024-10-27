using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ClickerManager : MonoBehaviour
{
    public Button clickButton;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI multiplierText;

    private float currency = 0f;
    private float sessionCurrency = 0f;
    private float baseGain = 0.01f;
    private float currentMultiplier = 1f;
    private float maxMultiplier = 10f;
    private float multiplierIncrement = 0.01f;
    private float multiplierDecayTime = 1f;

    private Coroutine multiplierResetCoroutine;

    void Start()
    {
        clickButton.onClick.AddListener(OnButtonClick);
        UpdateUI();
    }

    void OnButtonClick()
    {
        float earnedThisClick = baseGain * currentMultiplier;
        currency += earnedThisClick;
        sessionCurrency += earnedThisClick;

        currentMultiplier = Mathf.Min(currentMultiplier + multiplierIncrement, maxMultiplier);

        UpdateUI();

        if (multiplierResetCoroutine != null)
        {
            StopCoroutine(multiplierResetCoroutine);
        }
        multiplierResetCoroutine = StartCoroutine(ResetMultiplierAfterDelay());
    }

    private IEnumerator ResetMultiplierAfterDelay()
    {
        yield return new WaitForSeconds(multiplierDecayTime);

        PlayerManager.Instance.AddCurrency(sessionCurrency);
        sessionCurrency = 0f;

        currentMultiplier = 1f;
        UpdateUI();
    }

    private void UpdateUI()
    {
        currencyText.text = $"Currency Gained: \n{sessionCurrency:F2}";
        multiplierText.text = $"Multiplier: \nx{currentMultiplier:F2}";
    }
}
