using UnityEngine;

/// <summary>
/// Manages game levels and progression.
/// Handles level targets, progression, and level completion rewards.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int baseScoreTarget = 100; // Level 1 target
    [SerializeField] private int scoreIncreasePerLevel = 200; // Increase per level (e.g., Level 2 = 300, Level 3 = 500)
    
    [Header("Current Level State")]
    private int currentLevel = 1;
    private int currentLevelTarget = 100;
    
    // Events
    public event System.Action<int, int> OnLevelChanged; // (newLevel, newTarget)
    public event System.Action<int> OnLevelCompleted; // (completedLevel)
    
    private const string LEVEL_KEY = "CurrentLevel";
    
    private void Awake()
    {
        LoadLevel();
    }
    
    /// <summary>
    /// Gets the current level number
    /// </summary>
    public int CurrentLevel => currentLevel;
    
    /// <summary>
    /// Gets the score target for current level
    /// </summary>
    public int CurrentLevelTarget => currentLevelTarget;
    
    /// <summary>
    /// Initializes the level system based on total score
    /// </summary>
    private void LoadLevel()
    {
        // Get current total score
        int totalScore = PlayerPrefs.GetInt("totalScore", 0);
        
        // Calculate current level based on total score
        currentLevel = CalculateLevelFromTotalScore(totalScore);
        currentLevelTarget = CalculateLevelTarget(currentLevel);
        
        // Save the calculated level
        PlayerPrefs.SetInt(LEVEL_KEY, currentLevel);
        PlayerPrefs.Save();
        
        Debug.Log($"📊 Level Manager: Total Score {totalScore} → Level {currentLevel}, Target: {currentLevelTarget}");
    }
    
    /// <summary>
    /// Calculates what level the player should be at based on total score
    /// Level 1: 0-99 total score (target: 100)
    /// Level 2: 100-299 total score (target: 300) 
    /// Level 3: 300-499 total score (target: 500)
    /// etc.
    /// </summary>
    private int CalculateLevelFromTotalScore(int totalScore)
    {
        if (totalScore < 100) return 1;
        if (totalScore < 300) return 2;
        if (totalScore < 500) return 3;
        if (totalScore < 700) return 4;
        if (totalScore < 900) return 5;
        
        // For higher levels: every 200 points = new level after level 5
        return 5 + ((totalScore - 900) / 200);
    }
    
    /// <summary>
    /// Saves current level to persistent storage
    /// </summary>
    private void SaveLevel()
    {
        PlayerPrefs.SetInt(LEVEL_KEY, currentLevel);
        PlayerPrefs.Save();
        Debug.Log($"💾 Level saved: {currentLevel}");
    }
    
    /// <summary>
    /// Calculates the score target for a given level
    /// </summary>
    /// <param name="level">The level number</param>
    /// <returns>Score target for that level</returns>
    private int CalculateLevelTarget(int level)
    {
        
        return baseScoreTarget + (scoreIncreasePerLevel * (level - 1));
    }
    
    /// <summary>
    /// Checks if the player has reached the current level target
    /// </summary>
    /// <param name="score">Current score</param>
    /// <returns>True if level completed</returns>
    public bool CheckLevelCompletion(int score)
    {
        return score >= currentLevelTarget;
    }
    
    /// <summary>
    /// Advances to the next level
    /// </summary>
    public void CompleteLevel()
    {
        Debug.Log($"🎉 Level {currentLevel} completed!");
        
        // Invoke completion event
        OnLevelCompleted?.Invoke(currentLevel);
        
        // Advance to next level
        currentLevel++;
        currentLevelTarget = CalculateLevelTarget(currentLevel);
        
        // Save progress
        SaveLevel();
        
        // Notify listeners
        OnLevelChanged?.Invoke(currentLevel, currentLevelTarget);
        
      
    }
    
    /// <summary>
    /// Gets the score target for a specific level
    /// </summary>
    public int GetTargetForLevel(int level)
    {
        return CalculateLevelTarget(level);
    }
    
    /// <summary>
    /// Resets to starting level (useful for testing)
    /// </summary>
    public void ResetToStartingLevel()
    {
        currentLevel = startingLevel;
        currentLevelTarget = CalculateLevelTarget(currentLevel);
        SaveLevel();
        
        OnLevelChanged?.Invoke(currentLevel, currentLevelTarget);
        
        Debug.Log($"🔄 Level reset to {startingLevel}");
    }
    
    /// <summary>
    /// Gets the current level progress as a percentage
    /// </summary>
    public float GetLevelProgress(int currentScore)
    {
        return Mathf.Clamp01((float)currentScore / currentLevelTarget);
    }
    
    /// <summary>
    /// Updates the level based on current total score
    /// Call this when total score changes to check for level progression
    /// </summary>
    public void UpdateLevelFromTotalScore()
    {
        int totalScore = PlayerPrefs.GetInt("totalScore", 0);
        int newLevel = CalculateLevelFromTotalScore(totalScore);
        
        if (newLevel != currentLevel)
        {
            int oldLevel = currentLevel;
            currentLevel = newLevel;
            currentLevelTarget = CalculateLevelTarget(currentLevel);
            
            // Save new level
            PlayerPrefs.SetInt(LEVEL_KEY, currentLevel);
            PlayerPrefs.Save();
            
            // Notify listeners
            OnLevelChanged?.Invoke(currentLevel, currentLevelTarget);
            
            Debug.Log($"📈 Level up! {oldLevel} → {currentLevel} (Total Score: {totalScore}, New Target: {currentLevelTarget})");
        }
    }
    
    /// <summary>
    /// Gets the current level progress as a percentage based on total score
    /// </summary>
    public float GetTotalScoreProgress()
    {
        int totalScore = PlayerPrefs.GetInt("totalScore", 0);
        return GetTotalScoreProgress(totalScore);
    }
    
    /// <summary>
    /// Gets the level progress as a percentage for a specific total score
    /// </summary>
    public float GetTotalScoreProgress(int totalScore)
    {
        return currentLevelTarget > 0 ? Mathf.Clamp01((float)totalScore / currentLevelTarget) : 0f;
    }

}
