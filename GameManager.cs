using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button startButton;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CatController catController;
    [SerializeField] private SwipeDetector swipeDetector;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private ThumbVisualizer thumbVisualizer;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private CountdownController countdownController;
    
    [Header("UI Prefabs")]
    [SerializeField] private GameObject gameOverPanelPrefab; // Prefab of the game over panel
    [SerializeField] private GameObject levelCompletePanelPrefab; // Prefab of the level complete panel
    [SerializeField] private Transform uiParent; // Optional: Parent for instantiated UI (usually Canvas)
    
    [Header("VFX")]
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private Transform vfxSpawnPoint; // Optional: specific position for VFX
    [SerializeField] private float gameOverDelayAfterExplosion = 1f; // Delay before showing game over panel
    
     [SerializeField] private TextMeshProUGUI levelNumber;
    private bool gameStarted = false;
    private bool gameFailed = false;
    [SerializeField] private float freezeSecondsWhenBombAppears = 2f;
    
    private bool canDeactivateBomb = false; // Prevents bomb deactivation during initial swipe
    private GameOverPanelController gameOverPanelInstance; // Runtime instance of the game over panel
    private LevelCompletePanelController levelCompletePanelInstance; // Runtime instance of level complete panel
    
    void Start()
    {
        // InstantiateLevelCompletePanel(1, 1, 1);
        // Setup start button
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonPressed);
        }
        
        // Setup swipe detector events
        if (swipeDetector != null)
        {
            swipeDetector.OnSwipeUp += HandleSwipeUp;
            swipeDetector.OnSwipeDown += HandleSwipeDown;
            swipeDetector.OnTouchEnded += HandleTouchEnded;
        }
        
        // Setup cat controller events
        if (catController != null)
        {
            catController.OnBombTimeout += HandleBombTimeout;
        }
        
        // Setup thumb visualizer timeout event
        if (thumbVisualizer != null)
        {
            thumbVisualizer.OnUserTooSlow += HandleUserTooSlow;
        }
        
        // Initialize UI
        if (uiManager != null && scoreManager != null)
        {
            uiManager.UpdateScore(scoreManager.CurrentScore);
        }
        
        // Update level manager based on current total score
        if (levelManager != null)
        {
            levelManager.UpdateLevelFromTotalScore();
        }

       // levelNumber.text =levelManager.CurrentLevel.ToString();

    }

    void OnGUI(){
         levelNumber.text =levelManager.CurrentLevel.ToString();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (swipeDetector != null)
        {
            swipeDetector.OnSwipeUp -= HandleSwipeUp;
            swipeDetector.OnSwipeDown -= HandleSwipeDown;
            swipeDetector.OnTouchEnded -= HandleTouchEnded;
        }
        
        if (catController != null)
        {
            catController.OnBombTimeout -= HandleBombTimeout;
        }
        
        if (thumbVisualizer != null)
        {
            thumbVisualizer.OnUserTooSlow -= HandleUserTooSlow;
        }
        
        // Unsubscribe from game over panel if it exists
        if (gameOverPanelInstance != null)
        {
            gameOverPanelInstance.OnStartAgainClicked -= OnStartButtonPressed;
        }
        
        // Unsubscribe from level complete panel if it exists
        if (levelCompletePanelInstance != null)
        {
            levelCompletePanelInstance.OnNextLevelClicked -= OnNextLevelButtonPressed;
        }
    }
    
    private void OnStartButtonPressed()
    {
      
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }
        
        // Hide the start button
        if (startButton != null)
        {
            startButton.gameObject.SetActive(false);
        }
      
        // Reset game state
        gameFailed = false;
        gameStarted = false;
        
        // Reset all managers
        if (catController != null)
        {
            catController.Reset();
            catController.EnableBombSystem(false);
        }
        
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }
        
        if (uiManager != null)
        {
            uiManager.HideLevelFailed();
            uiManager.UpdateScore(0);
        }
        
        if (swipeDetector != null)
        {
            swipeDetector.EnableSwipeDetection(false);
        }
        
        if (thumbVisualizer != null)
        {
            thumbVisualizer.Reset();
        }
        
        // Destroy game over panel if it exists
        if (gameOverPanelInstance != null)
        {
            Destroy(gameOverPanelInstance.gameObject);
            gameOverPanelInstance = null;
        }
        
        // Destroy level complete panel if it exists
        if (levelCompletePanelInstance != null)
        {
            Destroy(levelCompletePanelInstance.gameObject);
            levelCompletePanelInstance = null;
        }
        
        // Start countdown
        StartCoroutine(CountdownCoroutine());
    }
    
    private void OnNextLevelButtonPressed()
    {
        // Same as starting new game, but level is already advanced
        OnStartButtonPressed();
    }
    
    private IEnumerator CountdownCoroutine()
    {
        // Countdown from 3 to 1 using sprite-based countdown
        for (int i = 3; i > 0; i--)
        {
            // Show sprite countdown (supports both old text and new sprite system)
            if (countdownController != null)
            {
                countdownController.ShowCountdown(i);
            }
            else if (uiManager != null)
            {
                // Fallback to text-based countdown if no CountdownController
                uiManager.ShowCountdown(i.ToString());
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        // Show "GO!" sprite
        if (countdownController != null)
        {
            countdownController.ShowCountdown(0); // 0 means "GO"
        }
        else if (uiManager != null)
        {
            uiManager.ShowCountdown("GO!");
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Hide countdown
        if (countdownController != null)
        {
            countdownController.HideCountdown();
        }
        else if (uiManager != null)
        {
            uiManager.HideCountdown();
        }
        
        // Start the game
        gameStarted = true;
        
        if (swipeDetector != null)
        {
            swipeDetector.EnableSwipeDetection(true);
        }
        
        if (catController != null)
        {
            catController.EnableBombSystem(true);
        }
        
        if (thumbVisualizer != null)
        {
            thumbVisualizer.StartRhythm();
        }
    }
    
    private void HandleSwipeUp()
    {
        if (!gameStarted || gameFailed) return;
        if (catController == null) return;
        
        // Check if bomb is active - FAIL!
        if (catController.IsBombActive)
        {
            catController.ShowBombUp();
            
            // Mark game as failed immediately to prevent further input
            gameFailed = true;
            gameStarted = false;
            
            // Disable systems immediately
            if (swipeDetector != null)
            {
                swipeDetector.EnableSwipeDetection(false);
            }
            
            if (catController != null)
            {
                catController.EnableBombSystem(false);
            }
            
            if (thumbVisualizer != null)
            {
                thumbVisualizer.StopRhythm();
            }
            
            // Spawn explosion VFX before showing game over
            SpawnExplosionVFX();
            
            // Play level failed sound (includes explosion)
            if (audioManager != null)
            {
                audioManager.PlayLevelFailedSound();
            }
            
            // Delay the game over panel to let explosion play
            StartCoroutine(ShowGameOverPanelAfterDelay(gameOverDelayAfterExplosion, true));
            return;
        }
        
        // Normal swipe up - works with any couple
        if (!catController.IsUp)
        {
            // Show appropriate up sprite based on current couple
            switch (catController.CurrentCouple)
            {
                case CatController.CoupleType.Cat:
                    catController.ShowCatUp();
                    break;
                case CatController.CoupleType.Dog:
                    catController.ShowDogUp();
                    break;
                case CatController.CoupleType.Pig:
                    catController.ShowPigUp();
                    break;
                case CatController.CoupleType.CatSnake:
                    catController.ShowCatSnakeUp();
                    break;
                case CatController.CoupleType.Bomb:
                    catController.ShowCatUp(); // Fallback
                    break;
            }
            
            // Notify thumb visualizer that cat sprite appeared UP
            if (thumbVisualizer != null)
            {
                thumbVisualizer.OnCatSpriteAppeared(true);
            }
            
            // Play cat up sound
            if (audioManager != null)
            {
                audioManager.PlayCatUpSound();
            }
            
            // Add score for successful swipe up
            if (scoreManager != null)
            {
                scoreManager.AddPoint();
                
                if (uiManager != null)
                {
                    uiManager.UpdateScore(scoreManager.CurrentScore);
                }
                
                // Check for level completion
                CheckLevelCompletion();
            }
        }
    }
    
    private void HandleSwipeDown()
    {
        if (!gameStarted || gameFailed) return;
        if (catController == null) return;
        
        // If bomb is active, deactivate it and show current couple's down sprite
        // BUT only if we're allowed to (not during the initial swipe that spawned it)
        if (catController.IsBombActive)
        {
            if (canDeactivateBomb)
            {
                catController.DeactivateBomb();
                
                // Show down sprite of the CURRENT couple (not always cat)
                switch (catController.CurrentCouple)
                {
                    case CatController.CoupleType.Cat:
                        catController.ShowCatDown();
                        break;
                    case CatController.CoupleType.Dog:
                        catController.ShowDogDown();
                        break;
                    case CatController.CoupleType.Pig:
                        catController.ShowPigDown();
                        break;
                    case CatController.CoupleType.CatSnake:
                        catController.ShowCatSnakeDown();
                        break;
                    default:
                        catController.ShowCatDown();
                        break;
                }
                
                // Notify thumb visualizer that sprite appeared DOWN (after bomb deactivation)
                if (thumbVisualizer != null)
                {
                    thumbVisualizer.OnCatSpriteAppeared(false);
                }
                
                canDeactivateBomb = false; // Reset flag
            }
            return;
        }
        
        // Normal swipe down - works with any couple
        if (catController.IsUp)
        {
            // Show appropriate down sprite based on current couple
            switch (catController.CurrentCouple)
            {
                case CatController.CoupleType.Cat:
                    catController.ShowCatDown();
                    break;
                case CatController.CoupleType.Dog:
                    catController.ShowDogDown();
                    break;
                case CatController.CoupleType.Pig:
                    catController.ShowPigDown();
                    break;
                case CatController.CoupleType.CatSnake:
                    catController.ShowCatSnakeDown();
                    break;
                case CatController.CoupleType.Bomb:
                    catController.ShowCatDown(); // Fallback
                    break;
            }
            
            // Notify thumb visualizer that cat sprite appeared DOWN
            if (thumbVisualizer != null)
            {
                thumbVisualizer.OnCatSpriteAppeared(false);
            }
            
            // Track this swipe for dynamic bomb chance calculation
            catController.TrackSwipeDown();
            
            // Random chance to spawn bomb (with dynamic probability)
            if (catController.ShouldSpawnBomb())
            {
                canDeactivateBomb = false; // Prevent immediate deactivation
                StartCoroutine(ShowBombAfterDelay());
            }
        }
    }
    
    private IEnumerator ShowBombAfterDelay()
    {
        // Small delay before showing bomb
        yield return new WaitForSeconds(0.1f);
        
        if (gameFailed || !gameStarted) yield break;
        
        // Show bomb
        if (catController != null && !catController.IsUp)
        {
            catController.ShowBombDown();
            catController.ActivateBomb();
            
            // Freeze thumb rotation for 2 seconds when bomb appears
            if (thumbVisualizer != null)
            {
                thumbVisualizer.FreezeForDuration(freezeSecondsWhenBombAppears);
            }
        }
    }
    
    private void HandleTouchEnded()
    {
        // When touch ends, allow bomb to be deactivated on the NEXT swipe down
        if (catController != null && catController.IsBombActive)
        {
            canDeactivateBomb = true;
        }
    }
    
    private void HandleBombTimeout()
    {
        // Bomb timed out, show current couple's down sprite
        if (catController != null)
        {
            // Show down sprite of the CURRENT couple (not always cat)
            switch (catController.CurrentCouple)
            {
                case CatController.CoupleType.Cat:
                    catController.ShowCatDown();
                    break;
                case CatController.CoupleType.Dog:
                    catController.ShowDogDown();
                    break;
                case CatController.CoupleType.Pig:
                    catController.ShowPigDown();
                    break;
                case CatController.CoupleType.CatSnake:
                    catController.ShowCatSnakeDown();
                    break;
                default:
                    catController.ShowCatDown();
                    break;
            }
        }
        canDeactivateBomb = false; // Reset flag
    }
    
    private void HandleUserTooSlow()
    {
        Debug.Log("🔴 HandleUserTooSlow() CALLED in GameManager");
        Debug.Log($"gameStarted = {gameStarted}, gameFailed = {gameFailed}");
        
        if (!gameStarted || gameFailed)
        {
            Debug.Log("⚠️ Ignoring timeout - game not started or already failed");
            return;
        }
        
        Debug.Log("💥 Executing FailLevel('TOO LATE!')");
        FailLevel("TOO LATE!");
    }
    
    /// <summary>
    /// Checks if the player has reached the level target based on total score
    /// </summary>
    private void CheckLevelCompletion()
    {
        if (levelManager == null || scoreManager == null) return;
        
        // Calculate what total score would be after this attempt
        int currentScore = scoreManager.CurrentScore;
        int currentTotal = PlayerPrefs.GetInt("totalScore", 0);
        int potentialTotal = currentTotal + currentScore;
        
        // Check if the potential total reaches the current level target
        if (potentialTotal >= levelManager.CurrentLevelTarget)
        {
            Debug.Log($"🎉 Level {levelManager.CurrentLevel} completed! Attempt: {currentScore}, Total will be: {potentialTotal}/{levelManager.CurrentLevelTarget}");
            
            // Mark game as completed
            gameFailed = false;
            gameStarted = false;
            
            // Disable systems
            if (swipeDetector != null)
            {
                swipeDetector.EnableSwipeDetection(false);
            }
            
            if (catController != null)
            {
                catController.EnableBombSystem(false);
            }
            
            if (thumbVisualizer != null)
            {
                thumbVisualizer.StopRhythm();
            }
            
            // Update total score
            PlayerPrefs.SetInt("totalScore", potentialTotal);
            PlayerPrefs.Save();
            
            // Store current level data before advancing
            int completedLevel = levelManager.CurrentLevel;
            int finalScore = currentScore;
            
            // Update level based on new total (this might advance to next level)
            levelManager.UpdateLevelFromTotalScore();
            
            // Get next level target
            int nextLevelTarget = levelManager.CurrentLevelTarget;
            
            // Show level complete panel
            InstantiateLevelCompletePanel(completedLevel, finalScore, nextLevelTarget);
        }
    }
    
    private void FailLevel()
    {
        FailLevel("LEVEL FAILED!");
    }
    
    private void FailLevel(string message)
    {
        gameFailed = true;
        gameStarted = false;
        
        // Disable systems
        if (swipeDetector != null)
        {
            swipeDetector.EnableSwipeDetection(false);
        }
        
        if (catController != null)
        {
            catController.EnableBombSystem(false);
        }
        
        if (thumbVisualizer != null)
        {
            thumbVisualizer.StopRhythm();
        }
        
        // Determine failure type and play appropriate sound
        bool isTooLate = message.Contains("TOO LATE");
        
        if (audioManager != null)
        {
            if (isTooLate)
            {
                audioManager.PlayTooLateSound();
            }
            else
            {
                audioManager.PlayLevelFailedSound();
            }
        }
        
        // Get current data and update total score
        int currentScore = scoreManager != null ? scoreManager.CurrentScore : 0;
        int currentTotal = PlayerPrefs.GetInt("totalScore", 0);
        int newTotal = currentTotal + currentScore;
        PlayerPrefs.SetInt("totalScore", newTotal);
        PlayerPrefs.Save();
        
        Debug.Log($"💎 Total Score updated: {currentTotal} + {currentScore} = {newTotal}");
        
        // Update level based on new total score
        if (levelManager != null)
        {
            levelManager.UpdateLevelFromTotalScore();
        }
        
        int levelTarget = levelManager != null ? levelManager.CurrentLevelTarget : 100;
        
        Debug.Log($"🎯 FAIL LEVEL - Score: {currentScore}, Target: {levelTarget}");
        
        // Get updated total score for progress display
        int updatedTotalScore = PlayerPrefs.GetInt("totalScore", 0);
        
        // Instantiate and show game over panel
        InstantiateGameOverPanel(isTooLate, updatedTotalScore, levelTarget, currentScore);
        
        // Optional: Keep the old text UI for backwards compatibility (can remove later)
        // if (uiManager != null)
        // {
        //     uiManager.ShowLevelFailed(message);
        // }
        
        // Note: Start button not needed anymore as the panel has its own "Start Again" button
    }
    
    /// <summary>
    /// Instantiates the game over panel from prefab and shows it
    /// </summary>
    /// <param name="isTooLate">True if timeout failure, false if bomb explosion</param>
    private void InstantiateGameOverPanel(bool isTooLate)
    {
        InstantiateGameOverPanel(isTooLate, 0, 100);
    }
    
    /// <summary>
    /// Instantiates the game over panel from prefab and shows it with score info
    /// </summary>
    /// <param name="isTooLate">True if timeout failure, false if bomb explosion</param>
    /// <param name="totalScore">Current total score</param>
    /// <param name="levelTarget">Target score for current level</param>
    private void InstantiateGameOverPanel(bool isTooLate, int totalScore, int levelTarget)
    {
        InstantiateGameOverPanel(isTooLate, totalScore, levelTarget, 0);
    }
    
    /// <summary>
    /// Instantiates the game over panel from prefab and shows it with detailed score info
    /// </summary>
    /// <param name="isTooLate">True if timeout failure, false if bomb explosion</param>
    /// <param name="totalScore">Current total score</param>
    /// <param name="levelTarget">Target score for current level</param>
    /// <param name="attemptScore">Score achieved in this attempt</param>
    private void InstantiateGameOverPanel(bool isTooLate, int totalScore, int levelTarget, int attemptScore)
    {
        if (gameOverPanelPrefab == null)
        {
            Debug.LogWarning("⚠️ Game Over Panel Prefab not assigned in GameManager!");
            return;
        }
        
        // Don't create if one already exists
        if (gameOverPanelInstance != null)
        {
            Debug.LogWarning("⚠️ Game Over Panel already exists! Destroying old one...");
            Destroy(gameOverPanelInstance.gameObject);
        }
        
        // Determine parent (Canvas or root)
        Transform parent = uiParent;
        if (parent == null)
        {
            // Try to find Canvas automatically
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                parent = canvas.transform;
                Debug.Log("📍 Using auto-found Canvas as parent for Game Over Panel");
            }
        }
        
        // Instantiate the prefab
        GameObject panelObject = Instantiate(gameOverPanelPrefab, parent);
        
        // Ensure the panel GameObject is active
        panelObject.SetActive(true);
        
        // Get the controller component
        gameOverPanelInstance = panelObject.GetComponent<GameOverPanelController>();
        
        if (gameOverPanelInstance == null)
        {
            Debug.LogError("❌ Game Over Panel Prefab does not have GameOverPanelController component!");
            Destroy(panelObject);
            return;
        }
        
        // Subscribe to the start again event
        gameOverPanelInstance.OnStartAgainClicked += OnStartButtonPressed;
        
        // Show the panel with total score progress and attempt score
        gameOverPanelInstance.ShowGameOver(isTooLate, totalScore, levelTarget, attemptScore);
        
        Debug.Log($"🎮 Game Over Panel: Attempt {attemptScore}, Total {totalScore}/{levelTarget}");
    }
    
    /// <summary>
    /// Instantiates the level complete panel from prefab and shows it
    /// </summary>
    private void InstantiateLevelCompletePanel(int completedLevel, int finalScore, int nextLevelTarget)
    {
     
        // Don't create if one already exists
        if (levelCompletePanelInstance != null)
        {
            Debug.LogWarning("⚠️ Level Complete Panel already exists! Destroying old one...");
            Destroy(levelCompletePanelInstance.gameObject);
        }
        
        // Determine parent (Canvas or root)
        Transform parent = uiParent;
        
        
        // Instantiate the prefab
        GameObject panelObject = Instantiate(levelCompletePanelPrefab, parent);
        
        // Ensure the panel GameObject is active
        panelObject.SetActive(true);
        
        // Get the controller component
        levelCompletePanelInstance = panelObject.GetComponent<LevelCompletePanelController>();
        
        if (levelCompletePanelInstance == null)
        {
            Debug.LogError("❌ Level Complete Panel Prefab does not have LevelCompletePanelController component!");
            Destroy(panelObject);
            return;
        }
        
        // Subscribe to the next level event
        levelCompletePanelInstance.OnNextLevelClicked += OnNextLevelButtonPressed;
        
        // Show the panel with level completion info
        levelCompletePanelInstance.ShowLevelComplete(completedLevel, finalScore, nextLevelTarget);
        
        Debug.Log($"🎉 Level Complete Panel instantiated - Level: {completedLevel}, Score: {finalScore}, Next Target: {nextLevelTarget}");
    }
    
    /// <summary>
    /// Spawns explosion VFX at the bomb location
    /// </summary>
    private void SpawnExplosionVFX()
    {
        if (explosionVFXPrefab == null)
        {
            Debug.LogWarning("⚠️ Explosion VFX Prefab not assigned in GameManager!");
            return;
        }
        
        // Determine spawn position
        Vector3 spawnPosition;
        
        if (vfxSpawnPoint != null)
        {
            // Use specified spawn point
            spawnPosition = vfxSpawnPoint.position;
        }
        else if (catController != null)
        {
            // Use cat controller position (where the bomb is)
            spawnPosition = catController.transform.position;
            
            // Move VFX slightly forward in Z to render on top of sprites
            spawnPosition.z -= 1f; // Negative Z is closer to camera in 2D
        }
        else
        {
            // Fallback to center of screen
            spawnPosition = Vector3.zero;
        }
        
        // Instantiate explosion VFX
        GameObject explosion = Instantiate(explosionVFXPrefab, spawnPosition, Quaternion.identity);
        
        // Ensure VFX renders on top by setting sorting layer and order
        ParticleSystemRenderer[] renderers = explosion.GetComponentsInChildren<ParticleSystemRenderer>();
        foreach (ParticleSystemRenderer renderer in renderers)
        {
            // Set to a high sorting order to render on top
            renderer.sortingOrder = 100; // High value to be above sprites
            
            // Optional: Set sorting layer if needed (uncomment and adjust if you have custom layers)
            // renderer.sortingLayerName = "Effects"; // Create this layer in Unity if needed
            
            Debug.Log($"🎨 VFX Renderer sorting order set to: {renderer.sortingOrder}");
        }
        
        Debug.Log($"💥 Explosion VFX spawned at position: {spawnPosition}");
        
        // Auto-destroy the VFX after some time (assuming particle systems have auto-destroy or we clean up manually)
        // Most VFX prefabs have ParticleSystem with "Stop Action: Destroy" so they clean themselves
        // But we'll add a safety destroy after 5 seconds just in case
        Destroy(explosion, 5f);
    }
    
    /// <summary>
    /// Shows the game over panel after a delay (to let explosion VFX play)
    /// </summary>
    /// <param name="delay">Delay in seconds</param>
    /// <param name="isBombExplosion">True if this was caused by bomb explosion (normal fail), false for timeout</param>
    private IEnumerator ShowGameOverPanelAfterDelay(float delay, bool isBombExplosion)
    {
        
        
        yield return new WaitForSeconds(delay);
        
        // Get current data
        int currentScore = scoreManager != null ? scoreManager.CurrentScore : 0;
        int currentTotal = PlayerPrefs.GetInt("totalScore", 0);
        int newTotal = currentTotal + currentScore;
        PlayerPrefs.SetInt("totalScore", newTotal);
        PlayerPrefs.Save();
        
        Debug.Log($"💎 Total Score updated: {currentTotal} + {currentScore} = {newTotal}");
        
        // Update level based on new total score
        if (levelManager != null)
        {
            levelManager.UpdateLevelFromTotalScore();
        }
        
        int levelTarget = levelManager != null ? levelManager.CurrentLevelTarget : 100;
        
        // Get updated total score for progress display
        int updatedTotalScore = PlayerPrefs.GetInt("totalScore", 0);
        
        // Instantiate and show game over panel
        bool isTooLate = !isBombExplosion; // If not bomb explosion, it's a timeout
        InstantiateGameOverPanel(isTooLate, updatedTotalScore, levelTarget, currentScore);
        
    }
}
