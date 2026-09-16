using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text resultText; // swap to Text if not using TextMeshPro
    [SerializeField] private Button restartButton;

    [Header("Result Sprites")]
    [SerializeField] private Image resultImage;       // single Image that swaps sprite
    [SerializeField] private Sprite fishWonSprite;     // shown when fisherman is eaten
    [SerializeField] private Sprite fishermanWonSprite; // dead fish sprite
    [SerializeField, Min(0.1f)] private float fishWonImageScale = 2f;

    private Vector3 originalImageScale;
    [Header("Hit Feedback")]
    [SerializeField] private HitStop hitStop;

    private bool gameOverPending;

    private void Awake()
    {
        if (resultImage != null)
        {
            originalImageScale = resultImage.transform.localScale;
        }

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
        StartCoroutine(ShowGameOverAfterHitStop("Fish Won!", fishWonSprite));
    }

    private void HandleFishermanWon()
    {
        StartCoroutine(ShowGameOverAfterHitStop("Fisherman Won!", fishermanWonSprite));
    }

    private IEnumerator ShowGameOverAfterHitStop(string message, Sprite sprite)
    {
        if (gameOverPending) yield break;
        gameOverPending = true;

        while (hitStop != null && hitStop.IsPlaying)
        {
            yield return null;
        }

        ShowGameOver(message, sprite);
    }

    private void ShowGameOver(string message, Sprite sprite)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }

        if (resultImage != null && sprite != null)
        {
            resultImage.sprite = sprite;
            resultImage.type = Image.Type.Simple;
            resultImage.preserveAspect = true;

            float scaleMultiplier = sprite == fishWonSprite
                ? fishWonImageScale
                : 1f;

            resultImage.transform.localScale = originalImageScale * scaleMultiplier;

            resultImage.enabled = true;
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