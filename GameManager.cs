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
    }
    
    private void OnStartButtonPressed()
    {
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
        
        // Start countdown
        StartCoroutine(CountdownCoroutine());
    }
    
    private IEnumerator CountdownCoroutine()
    {
        // Countdown from 3 to 1
        for (int i = 3; i > 0; i--)
        {
            if (uiManager != null)
            {
                uiManager.ShowCountdown(i.ToString());
            }
            yield return new WaitForSeconds(1f);
        }
        
        // Show "GO!"
        if (uiManager != null)
        {
            uiManager.ShowCountdown("GO!");
        }
        yield return new WaitForSeconds(0.5f);
        
        if (uiManager != null)
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
            FailLevel();
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
        
        // If bomb is active, deactivate it and change to cat
        // BUT only if we're allowed to (not during the initial swipe that spawned it)
        if (catController.IsBombActive)
        {
            if (canDeactivateBomb)
            {
                catController.DeactivateBomb();
                catController.ShowCatDown();
                
                // Notify thumb visualizer that cat sprite appeared DOWN (after bomb deactivation)
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
        // Bomb timed out, change back to cat
        if (catController != null)
        {
            catController.ShowCatDown();
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
        
        // Show fail UI with custom message
        if (uiManager != null)
        {
            uiManager.ShowLevelFailed(message);
        }
        
        // Show start button again to retry
        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
        }
    }
}
