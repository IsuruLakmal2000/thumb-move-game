using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button startButton;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CatController catController;
    [SerializeField] private SwipeDetector swipeDetector;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ThumbVisualizer thumbVisualizer;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameOverPanelController gameOverPanel;
    [SerializeField] private CountdownController countdownController;
    
    [Header("VFX")]
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private Transform vfxSpawnPoint; // Optional: specific position for VFX
    [SerializeField] private float gameOverDelayAfterExplosion = 1f; // Delay before showing game over panel
    
    private bool gameStarted = false;
    private bool gameFailed = false;
    [SerializeField] private float freezeSecondsWhenBombAppears = 2f;
    
    private bool canDeactivateBomb = false; // Prevents bomb deactivation during initial swipe
    
    void Start()
    {
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
        
        // Setup game over panel events
        if (gameOverPanel != null)
        {
            gameOverPanel.OnStartAgainClicked += OnStartButtonPressed;
        }
        
        // Initialize UI
        if (uiManager != null && scoreManager != null)
        {
            uiManager.UpdateScore(scoreManager.CurrentScore);
        }
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
        
        if (gameOverPanel != null)
        {
            gameOverPanel.OnStartAgainClicked -= OnStartButtonPressed;
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
        
        // Hide game over panel if it was visible
        if (gameOverPanel != null)
        {
            gameOverPanel.HidePanel();
        }
        
        // Start countdown
            StartCoroutine(CountdownCoroutine());
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
        
        // Show game over panel with appropriate sprite
        if (gameOverPanel != null)
        {
            gameOverPanel.ShowGameOver(isTooLate);
        }
        
        // Optional: Keep the old text UI for backwards compatibility (can remove later)
        // if (uiManager != null)
        // {
        //     uiManager.ShowLevelFailed(message);
        // }
        
        // Note: Start button not needed anymore as the panel has its own "Start Again" button
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
        Debug.Log($"⏳ Waiting {delay}s before showing game over panel...");
        
        yield return new WaitForSeconds(delay);
        
        // Show game over panel with appropriate sprite
        if (gameOverPanel != null)
        {
            bool isTooLate = !isBombExplosion; // If not bomb explosion, it's a timeout
            gameOverPanel.ShowGameOver(isTooLate);
            Debug.Log($"🎮 Game over panel shown - Bomb Explosion: {isBombExplosion}, Too Late: {isTooLate}");
        }
    }
}
