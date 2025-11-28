using UnityEngine;
using TMPro;

/// <summary>
/// Displays the total score from PlayerPrefs on a TextMeshProUGUI component.
/// Attach this script to a GameObject with a TextMeshProUGUI component.
/// The score updates on Start and can be refreshed by calling RefreshScore().
/// </summary>
public class TotalScoreDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Display Settings")]
    [SerializeField] private string prefix = "Total: ";
    [SerializeField] private string suffix = "";
    
    private const string TOTAL_SCORE_KEY = "totalScore";
    
    // Singleton instance for easy access from other scripts
    public static TotalScoreDisplay Instance { get; private set; }
    
    private void Awake()
    {
        // Set up singleton (optional, allows other scripts to easily refresh)
        if (Instance == null)
        {
            Instance = this;
        }
        
        // Try to get TextMeshProUGUI component if not assigned
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
        }
    }
    
    private void Start()
    {
        RefreshScore();
    }
    
    /// <summary>
    /// Refreshes the displayed score from PlayerPrefs.
    /// Call this after the total score is updated (level complete or game over).
    /// </summary>
    public void RefreshScore()
    {
        int totalScore = PlayerPrefs.GetInt(TOTAL_SCORE_KEY, 0);
        UpdateDisplay(totalScore);
    }
    
    /// <summary>
    /// Updates the display with a specific score value.
    /// </summary>
    /// <param name="score">The score to display</param>
    public void UpdateDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{prefix}{score}{suffix}";
        }
        else
        {
            Debug.LogWarning("TotalScoreDisplay: TextMeshProUGUI component not assigned!");
        }
    }
    
    /// <summary>
    /// Gets the current total score from PlayerPrefs.
    /// </summary>
    /// <returns>The total score</returns>
    public int GetTotalScore()
    {
        return PlayerPrefs.GetInt(TOTAL_SCORE_KEY, 0);
    }
}
