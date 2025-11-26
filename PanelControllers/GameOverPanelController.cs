using UnityEngine;
using UnityEngine.UI;

public class GameOverPanelController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Game Over Sprites")]
    [SerializeField] private Image gameOverImage;
    [SerializeField] private Sprite gameOverNormalSprite;
    [SerializeField] private Sprite gameOverTooLateSprite;
    
    [Header("Button References")]
    [SerializeField] private Button startAgainButton;
    
    [Header("Progress UI")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMPro.TextMeshProUGUI progressText; // Shows "120 / 300" (total score / level target)
    [SerializeField] private TMPro.TextMeshProUGUI attemptScoreText; // Shows "This Attempt: +45"
    
    [Header("Audio References")]
    [SerializeField] private AudioManager audioManager;
    
    // Event that GameManager can subscribe to
    public event System.Action OnStartAgainClicked;
    
    private void Awake()
    {
        // Setup button listener
        if (startAgainButton != null)
        {
            startAgainButton.onClick.AddListener(HandleStartAgainClicked);
        }
        
        // Don't hide panel in Awake - let the instantiator control visibility
        // If this prefab is in the scene, it should start inactive in the Inspector
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from button
        if (startAgainButton != null)
        {
            startAgainButton.onClick.RemoveListener(HandleStartAgainClicked);
        }
    }
    
    /// <summary>
    /// Shows the game over panel with normal "LEVEL FAILED" message
    /// </summary>
    public void ShowGameOver()
    {
        ShowGameOver(false);
    }
    
    /// <summary>
    /// Shows the game over panel with specified failure type
    /// </summary>
    /// <param name="isTooLate">True if player was too slow, false for normal game over</param>
    public void ShowGameOver(bool isTooLate)
    {
        ShowGameOver(isTooLate, 0, 100);
    }
    
    /// <summary>
    /// Shows the game over panel with score/level progress
    /// </summary>
    /// <param name="isTooLate">True if player was too slow, false for normal game over</param>
    /// <param name="totalScore">Current total score</param>
    /// <param name="levelTarget">Target score for current level</param>
    public void ShowGameOver(bool isTooLate, int totalScore, int levelTarget)
    {
        ShowGameOver(isTooLate, totalScore, levelTarget, 0);
    }
    
    /// <summary>
    /// Shows the game over panel with detailed score information
    /// </summary>
    /// <param name="isTooLate">True if player was too slow, false for normal game over</param>
    /// <param name="totalScore">Current total score</param>
    /// <param name="levelTarget">Target score for current level</param>
    /// <param name="attemptScore">Score achieved in this attempt</param>
    public void ShowGameOver(bool isTooLate, int totalScore, int levelTarget, int attemptScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Set appropriate sprite based on failure type
        if (gameOverImage != null)
        {
            if (isTooLate && gameOverTooLateSprite != null)
            {
                gameOverImage.sprite = gameOverTooLateSprite;
                Debug.Log("🔴 Game Over Panel: Showing 'TOO LATE' sprite");
            }
            else if (gameOverNormalSprite != null)
            {
                gameOverImage.sprite = gameOverNormalSprite;
                Debug.Log("🔴 Game Over Panel: Showing 'NORMAL' game over sprite");
            }
        }
        
        // Update progress slider and text (total score progress)
        UpdateProgress(totalScore, levelTarget);
        
        // Update attempt score text if provided
        UpdateAttemptScore(attemptScore);
        
        Debug.Log($"🔴 Game Over Panel displayed - Too Late: {isTooLate}, Total: {totalScore}/{levelTarget}, Attempt: +{attemptScore}");
    }
    
    /// <summary>
    /// Updates the progress slider and text to show total score progression toward level target
    /// </summary>
    public void UpdateProgress(int totalScore, int levelTarget)
    {
        // Update slider
        if (progressSlider != null)
        {
            float progress = levelTarget > 0 ? (float)totalScore / levelTarget : 0f;
            progressSlider.value = progress;
            Debug.Log($"📊 Level progress: {progress:P0} ({totalScore}/{levelTarget})");
        }
        
        // Update text
        if (progressText != null)
        {
            progressText.text = $"{totalScore} / {levelTarget}";
            Debug.Log($"📝 Level progress text: {totalScore} / {levelTarget}");
        }
    }
    
    /// <summary>
    /// Updates the attempt score display
    /// </summary>
    public void UpdateAttemptScore(int attemptScore)
    {
        if (attemptScoreText != null && attemptScore > 0)
        {
            attemptScoreText.text = $"This Attempt: +{attemptScore}";
            attemptScoreText.gameObject.SetActive(true);
            Debug.Log($"📝 Attempt score text: +{attemptScore}");
        }
        else if (attemptScoreText != null)
        {
            attemptScoreText.gameObject.SetActive(false);
        }
    }
    

    
    /// <summary>
    /// Hides the game over panel
    /// </summary>
    public void HidePanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("✅ Game Over Panel hidden");
        }
    }
    
    private void HandleStartAgainClicked()
    {
        Debug.Log("🔄 Start Again button clicked");
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }
        
        // Hide the panel
        HidePanel();
        
        // Notify subscribers (GameManager)
        OnStartAgainClicked?.Invoke();
    }
    
    /// <summary>
    /// Check if the panel is currently visible
    /// </summary>
    public bool IsVisible()
    {
        return gameOverPanel != null && gameOverPanel.activeSelf;
    }
}
