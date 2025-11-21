using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI levelFailedText;
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Cat References")]
    [SerializeField] private SpriteRenderer catSpriteRenderer;
    [SerializeField] private Sprite catDownSprite;
    [SerializeField] private Sprite catUpSprite;
    
    [Header("Bomb References")]
    [SerializeField] private Sprite bombDownSprite;
    [SerializeField] private Sprite bombUpSprite;
    
    [Header("Settings")]
    [SerializeField] private float swipeThreshold = 50f;
    [SerializeField] private float bombAppearChance = 0.3f; // 30% chance
    [SerializeField] private float bombTimeLimit = 1f; // 1 second to react
    
    private bool gameStarted = false;
    private bool gameFailed = false;
    private Vector2 lastTouchPosition;
    private bool isTouching = false;
    private bool isCurrentlyCatUp = false;
    private bool isBombActive = false;
    private float bombTimer = 0f;
    private int currentScore = 0;
    
    void Start()
    {
        // Make sure cat starts with down sprite
        if (catSpriteRenderer != null && catDownSprite != null)
        {
            catSpriteRenderer.sprite = catDownSprite;
        }
        
        // Setup start button
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonPressed);
        }
        
        // Hide countdown text initially
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        // Hide level failed text initially
        if (levelFailedText != null)
        {
            levelFailedText.gameObject.SetActive(false);
        }
        
        // Initialize score display
        UpdateScoreDisplay();
    }
    
    void Update()
    {
        if (!gameStarted || gameFailed) return;
        
        // Handle bomb timer
        if (isBombActive)
        {
            bombTimer += Time.deltaTime;
            if (bombTimer >= bombTimeLimit)
            {
                // Time's up! Change bomb back to cat
                ChangeToCat();
            }
        }
        
        // Handle touch input for swipe detection
        HandleTouchInput();
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
        isBombActive = false;
        bombTimer = 0f;
        isCurrentlyCatUp = false;
        currentScore = 0;
        
        // Update score display
        UpdateScoreDisplay();
        
        // Reset cat sprite
        if (catSpriteRenderer != null && catDownSprite != null)
        {
            catSpriteRenderer.sprite = catDownSprite;
        }
        
        // Hide level failed text
        if (levelFailedText != null)
        {
            levelFailedText.gameObject.SetActive(false);
        }
        
        // Start countdown
        StartCoroutine(CountdownCoroutine());
    }
    
    private IEnumerator CountdownCoroutine()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }
        
        // Countdown from 3 to 1
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
            }
            yield return new WaitForSeconds(1f);
        }
        
        // Show "GO!" or hide countdown
        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }
        yield return new WaitForSeconds(0.5f);
        
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        // Start the game
        gameStarted = true;
    }
    
    private void HandleTouchInput()
    {
        // Check if there's any touch input
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            
            // Touch started
            if (touch.press.wasPressedThisFrame)
            {
                lastTouchPosition = touch.position.ReadValue();
                isTouching = true;
            }
            
            // Touch is being held - detect continuous swipes
            if (touch.press.isPressed && isTouching)
            {
                Vector2 currentTouchPosition = touch.position.ReadValue();
                DetectContinuousSwipe(lastTouchPosition, currentTouchPosition);
                lastTouchPosition = currentTouchPosition;
            }
            
            // Touch ended
            if (touch.press.wasReleasedThisFrame)
            {
                isTouching = false;
            }
        }
        // Fallback to mouse for testing in editor
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                lastTouchPosition = Mouse.current.position.ReadValue();
                isTouching = true;
            }
            
            if (Mouse.current.leftButton.isPressed && isTouching)
            {
                Vector2 currentTouchPosition = Mouse.current.position.ReadValue();
                DetectContinuousSwipe(lastTouchPosition, currentTouchPosition);
                lastTouchPosition = currentTouchPosition;
            }
            
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isTouching = false;
            }
        }
    }
    
    private void DetectContinuousSwipe(Vector2 lastPos, Vector2 currentPos)
    {
        Vector2 swipeDelta = currentPos - lastPos;
        
        // Check if movement is significant enough
        if (swipeDelta.magnitude < swipeThreshold)
        {
            return;
        }
        
        // Determine if it's a vertical swipe
        if (Mathf.Abs(swipeDelta.y) > Mathf.Abs(swipeDelta.x))
        {
            if (swipeDelta.y > 0 && !isCurrentlyCatUp)
            {
                // Swipe up - only trigger if cat is currently down
                OnSwipeUp();
            }
            else if (swipeDelta.y < 0 && isCurrentlyCatUp)
            {
                // Swipe down - only trigger if cat is currently up
                OnSwipeDown();
            }
        }
    }
    
    private void OnSwipeUp()
    {
        if (catSpriteRenderer == null) return;
        
        // Check if bomb is active - FAIL!
        if (isBombActive)
        {
            if (bombUpSprite != null)
            {
                catSpriteRenderer.sprite = bombUpSprite;
            }
            FailLevel();
            return;
        }
        
        // Normal cat swipe up
        if (catUpSprite != null)
        {
            catSpriteRenderer.sprite = catUpSprite;
            isCurrentlyCatUp = true;
            
            // Increment score for successful swipe up
            currentScore++;
            UpdateScoreDisplay();
            
            Debug.Log("Swipe Up - Cat Up! Score: " + currentScore);
        }
    }
    
    private void OnSwipeDown()
    {
        if (catSpriteRenderer == null) return;
        
        // If bomb is active, deactivate it and change to cat
        if (isBombActive)
        {
            ChangeToCat();
            return;
        }
        
        // Normal cat swipe down
        if (catDownSprite != null)
        {
            catSpriteRenderer.sprite = catDownSprite;
            isCurrentlyCatUp = false;
            
            // Random chance to spawn bomb
            if (Random.value < bombAppearChance)
            {
                StartCoroutine(ShowBombAfterDelay());
            }
            
            Debug.Log("Swipe Down - Cat Down!");
        }
    }
    
    private IEnumerator ShowBombAfterDelay()
    {
        // Small delay before showing bomb
        yield return new WaitForSeconds(0.1f);
        
        if (gameFailed || !gameStarted) yield break;
        
        // Show bomb down sprite
        if (catSpriteRenderer != null && bombDownSprite != null && !isCurrentlyCatUp)
        {
            catSpriteRenderer.sprite = bombDownSprite;
            isBombActive = true;
            bombTimer = 0f;
            Debug.Log("BOMB APPEARED! Swipe down within " + bombTimeLimit + " seconds!");
        }
    }
    
    private void ChangeToCat()
    {
        isBombActive = false;
        bombTimer = 0f;
        
        if (catSpriteRenderer != null && catDownSprite != null)
        {
            catSpriteRenderer.sprite = catDownSprite;
            isCurrentlyCatUp = false;
            Debug.Log("Bomb avoided! Changed back to cat.");
        }
    }
    
    private void FailLevel()
    {
        gameFailed = true;
        gameStarted = false;
        isBombActive = false;
        
        // Show level failed text
        if (levelFailedText != null)
        {
            levelFailedText.gameObject.SetActive(true);
            levelFailedText.text = "LEVEL FAILED!";
        }
        
        // Show start button again to retry
        if (startButton != null)
        {
            startButton.gameObject.SetActive(true);
        }
        
        Debug.Log("LEVEL FAILED! You swiped up on the bomb!");
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }
}
