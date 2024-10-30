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
    private float difficultyBias = 2.15f;
    private bool isGameRunning = false;
    private float betAmount = 0f;
    private bool hasCashedOut = false;


    public UIManager uiManager;

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

            crashPoint = GenerateBiasedCrashPoint(1.1f, 20.0f, difficultyBias);
            isGameRunning = true;
            cashOutButton.interactable = true;
            startGameButton.interactable = false;
            uiManager.LockUI();
        }
        else
        {
            Debug.LogWarning("Invalid bet amount!");
            return;
        }
    }

    private float GenerateBiasedCrashPoint(float min, float max, float bias)
    {
        float randomValue = Mathf.Pow(Random.value, bias);
        return Mathf.Lerp(min, max, randomValue);
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
        outcomeText.text = "You crashed..";
        UpdateUI();
        uiManager.UnlockUI();
    }

    private void CashOut()
    {
        if (isGameRunning)
        {
            float winnings = betAmount * currentMultiplier;
            PlayerManager.Instance.AddCurrency(winnings);
            outcomeText.text = $"You cashed out {winnings:F2}";
            hasCashedOut = true;
            EndGame();
            uiManager.UnlockUI();
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
