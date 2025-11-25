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
    
    [Header("Audio References")]
    [SerializeField] private AudioManager audioManager;
    
    // Event that GameManager can subscribe to
    public event System.Action OnStartAgainClicked;
    
    private void Start()
    {
        // Setup button listener
        if (startAgainButton != null)
        {
            startAgainButton.onClick.AddListener(HandleStartAgainClicked);
        }
        
        // Hide panel at start
        HidePanel();
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
        
        Debug.Log($"🔴 Game Over Panel displayed - Too Late: {isTooLate}");
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
