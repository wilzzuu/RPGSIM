using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrashManager : MonoBehaviour
{
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI outcomeText;
    public Button cashOutButton;
    public Button startGameButton;
    public TMP_InputField betAmountInput;

    private float currentMultiplier = 1.0f;
    private float initialMultiplierRate = 0.02f;
    private float accelerationFactor = 0.05f;
    private float elapsedTime = 0f;
    private float crashPoint;
    private bool isGameRunning = false;
    private float betAmount = 0f;
    private bool hasCashedOut = false;

    void Start()
    {
        UpdateUI();
        cashOutButton.onClick.AddListener(CashOut);
        startGameButton.onClick.AddListener(StartCrashGame);
        cashOutButton.interactable = false;
    }

    void Update()
    {
        if (isGameRunning && !hasCashedOut)
        {
            elapsedTime += Time.deltaTime;
            float currentMultiplierRate = initialMultiplierRate + (accelerationFactor * elapsedTime);
            currentMultiplier += currentMultiplierRate * Time.deltaTime;
            multiplierText.text = $"Multiplier: {currentMultiplier:F2}x";

            if (currentMultiplier >= crashPoint)
            {
                Crash();
            }
        }
    }

    public void StartCrashGame()
    {
        if (float.TryParse(betAmountInput.text, out betAmount) && betAmount > 0 && betAmount <= PlayerManager.Instance.GetPlayerBalance())
        {
            PlayerManager.Instance.DeductCurrency(betAmount);
            UpdateUI();
            ResetGame();
            crashPoint = Random.Range(1f, 20.0f);
            isGameRunning = true;
            cashOutButton.interactable = true;
            startGameButton.interactable = false;
        }
        else
        {
            Debug.LogWarning("Invalid bet amount!");
            return;
        }
    }

    private void ResetGame()
    {
        currentMultiplier = 1.0f;
        elapsedTime = 0f;
        hasCashedOut = false;
        outcomeText.text = "";
    }

    private void Crash()
    {
        isGameRunning = false;
        cashOutButton.interactable = false;
        startGameButton.interactable = true;
        Debug.Log("CRASH! Player lost the bet.");
        outcomeText.text = "You crashed..";
        UpdateUI();
    }

    private void CashOut()
    {
        if (isGameRunning)
        {
            float winnings = betAmount * currentMultiplier;
            PlayerManager.Instance.AddCurrency(winnings);
            Debug.Log($"Player cashed out at {currentMultiplier:F2}x and won {winnings:F2}!");
            outcomeText.text = $"You cashed out {winnings:F2}";
            hasCashedOut = true;
            EndGame();
        }
    }

    private void EndGame()
    {
        isGameRunning = false;
        cashOutButton.interactable = false;
        startGameButton.interactable = true;
        UpdateUI();
    }

    private void UpdateUI()
    {
        multiplierText.text = $"Multiplier: {currentMultiplier:F2}x";
    }
}
