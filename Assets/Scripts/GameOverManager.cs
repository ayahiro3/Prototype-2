using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text resultText; // swap to Text if not using TextMeshPro
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    private void OnEnable()
    {
        FishController.OnFishermanEaten += HandleFishWon;
        FishController.OnFishKilled += HandleFishermanWon;
    }

    private void OnDisable()
    {
        FishController.OnFishermanEaten -= HandleFishWon;
        FishController.OnFishKilled -= HandleFishermanWon;
    }

    private void HandleFishWon()
    {
        ShowGameOver("Fish Won!");
    }

    private void HandleFishermanWon()
    {
        ShowGameOver("Fisherman Won!");
    }

    private void ShowGameOver(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}