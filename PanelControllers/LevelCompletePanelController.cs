using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controls the Level Complete panel that shows when player reaches level target.
/// Displays level completion info, rewards, and allows progression to next level.
/// </summary>
public class LevelCompletePanelController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject levelCompletePanel;
    
    [Header("UI Elements")]
    [SerializeField] private TMPro.TextMeshProUGUI levelNumberText; // "Level 1 Complete!"
    [SerializeField] private TMPro.TextMeshProUGUI rewardCointCount; // "Score: 150"
    [SerializeField] private TMPro.TextMeshProUGUI nextLevelText; // "Next Level: 300"
    
    [Header("Button References")]
   // [SerializeField] private Button openBtn;
    [SerializeField] private Button nextLevelBtn;
    
    [Header("Audio References")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip levelCompleteSound; // Optional: level complete sound
    
    // Event that GameManager can subscribe to
    public event System.Action OnNextLevelClicked;
    
    private int currentRewardCoins = 0; // Store the calculated reward
    private int lastCompletedLevel = 0; // Store the last completed level for reward calculation
    
    private void Awake()
    {
        nextLevelBtn.gameObject.SetActive(true);
        
        if (audioManager == null)
    {
          audioManager = FindObjectOfType<AudioManager>();
    }
        // Setup button listeners
       
        if (nextLevelBtn != null)
        {
            nextLevelBtn.onClick.AddListener(HandleClaimButtonClicked);
        }
    }

    private void Start(){
        
    }

    // Coroutine to animate the reward coin count from 0 to target value
    private IEnumerator AnimateRewardCount(int target)
    {
        float duration = 1.0f; // seconds
        float elapsed = 0f;
        int start = 0;
        int current = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            current = Mathf.RoundToInt(Mathf.Lerp(start, target, t));
            if (rewardCointCount != null)
                rewardCointCount.text = $"{current}";
            yield return null;
        }
        if (rewardCointCount != null)
            rewardCointCount.text = $"{target}";
    }

    private void HandleOpenButtonClicked()
    {
        // Handle animation... play
        Debug.Log("🎁 Opening reward box...");
        
        // TODO: Play reward box opening animation here
        
        // After animation, show claim button
        nextLevelBtn.gameObject.SetActive(true);
       

        // Calculate and display reward coins
        currentRewardCoins = CalculateRewardCoins(lastCompletedLevel);
        if (rewardCointCount != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateRewardCount(currentRewardCoins));
        }
        
        Debug.Log($"🪙 Reward revealed: {currentRewardCoins} coins");
    }
    
    private void HandleClaimButtonClicked()
    {
        Debug.Log($"💰 Claiming {currentRewardCoins} coins...");
        
        // Award the coins to player
        AwardCoinsToPlayer(currentRewardCoins);
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }
        
        // Hide panel and notify GameManager
        HidePanel();
        OnNextLevelClicked?.Invoke();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from buttons
       
        
        if (nextLevelBtn != null)
        {
            nextLevelBtn.onClick.RemoveListener(HandleClaimButtonClicked);
        }
    }
    
    /// <summary>
    /// Calculates reward coins based on completed level
    /// Level 1: 50-100 coins
    /// Level 2: 100-200 coins
    /// Level 3: 300-500 coins
    /// Level 4: 500-800 coins
    /// Level 5+: 1000-1500 coins (scales with level)
    /// </summary>
    /// <param name="completedLevel">The level that was just completed</param>
    /// <returns>Random reward coin amount</returns>
    private int CalculateRewardCoins(int completedLevel)
    {
        int minReward = 0;
        int maxReward = 0;
        
        switch (completedLevel)
        {
            case 1:
                minReward = 50;
                maxReward = 100;
                break;
            case 2:
                minReward = 100;
                maxReward = 200;
                break;
            case 3:
                minReward = 300;
                maxReward = 500;
                break;
            case 4:
                minReward = 500;
                maxReward = 800;
                break;
            case 5:
                minReward = 800;
                maxReward = 1200;
                break;
            default:
                // For levels 6 and above, scale rewards
                // Level 6: 1000-1500, Level 7: 1200-1800, etc.
                minReward = 1000 + ((completedLevel - 6) * 200);
                maxReward = 1500 + ((completedLevel - 6) * 300);
                break;
        }
        
        int rewardCoins = Random.Range(minReward, maxReward + 1);
        Debug.Log($"🪙 Level {completedLevel} reward: {rewardCoins} coins (range: {minReward}-{maxReward})");
        
        return rewardCoins;
    }
    
    /// <summary>
    /// Shows the level complete panel with completion details
    /// </summary>
    /// <param name="completedLevel">The level that was just completed</param>
    /// <param name="finalScore">Final score achieved</param>
    /// <param name="nextLevelTarget">Target for next level</param>
    public void ShowLevelComplete(int completedLevel, int finalScore, int nextLevelTarget)
    {
        lastCompletedLevel = completedLevel;

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
        
        // Update level number text
        if (levelNumberText != null)
        {
            levelNumberText.text = $"Level {completedLevel} Complete!";
        }
        
      
        
        // Update next level target text
        if (nextLevelText != null)
        {
            nextLevelText.text = $"Next Level: {nextLevelTarget}";
        }
        
        // Play level complete sound
        if (audioManager != null && levelCompleteSound != null)
        {
            audioManager.PlayOneShot(levelCompleteSound);
        }
    }
    
    /// <summary>
    /// Hides the level complete panel
    /// </summary>
    public void HidePanel()
    {
        if (levelCompletePanel != null)
        {
            Destroy(levelCompletePanel);
            
        }
    }
    
    private void HandleNextLevelClicked()
    {
        
        
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }
        
        // Hide panel
       
        
        // Notify GameManager
        OnNextLevelClicked?.Invoke();
    }
    
    /// <summary>
    /// Awards coins to the player and saves to PlayerPrefs
    /// </summary>
    /// <param name="coins">Amount of coins to award</param>
    private void AwardCoinsToPlayer(int coins)
    {
        const string COINS_KEY = "TotalCoins";
        
        // Get current coins
        int currentCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
        int newTotal = currentCoins + coins;
        
        // Save new total
        PlayerPrefs.SetInt(COINS_KEY, newTotal);
        PlayerPrefs.Save();
        
        Debug.Log($"💰 Coins awarded: {currentCoins} + {coins} = {newTotal}");
        Debug.Log($"💾 Total coins saved to PlayerPrefs: {newTotal}");
    }
    
    /// <summary>
    /// Gets the current total coins from PlayerPrefs
    /// </summary>
    /// <returns>Total coins amount</returns>
    public static int GetTotalCoins()
    {
        return PlayerPrefs.GetInt("TotalCoins", 0);
    }
}
