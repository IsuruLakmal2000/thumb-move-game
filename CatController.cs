using UnityEngine;
using System;

public class CatController : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite catDownSprite;
    [SerializeField] private Sprite catUpSprite;
    [SerializeField] private Sprite bombDownSprite;
    [SerializeField] private Sprite bombUpSprite;
    [SerializeField] private Sprite dogDownSprite;
    [SerializeField] private Sprite dogUpSprite;
    
    [Header("Couple Settings")]
    [SerializeField] private int minRoundsPerCouple = 3; // Minimum rounds before switching couple
    [SerializeField] private float coupleChangeChance = 0.4f; // Chance to change couple after min rounds
    
    [Header("Bomb Settings")]
    [SerializeField] private float bombAppearChance = 0.3f; // 30% chance (base)
    [SerializeField] private float bombTimeLimit = 1f; // 1 second to react
    [SerializeField] private int sameCoupleThreshold = 5; // After 5 swipes of same couple, increase bomb chance
    [SerializeField] private int totalSwipesThreshold = 7; // After 7 total swipes (mixed couples), increase bomb chance
    [SerializeField] private float increasedBombChance = 0.7f; // 70% chance when threshold reached
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeSpeed = 50f;
    
    public event Action OnBombTimeout;
    
    public enum CoupleType { Cat, Bomb, Dog }
    
    private bool isUp = false;
    private bool isBomb = false;
    private bool isBombActive = false;
    private float bombTimer = 0f;
    private bool bombSystemEnabled = false;
    
    private Vector3 originalPosition;
    private bool isShaking = false;
    private float shakeTimer = 0f;
    
    private CoupleType currentCouple = CoupleType.Cat;
    private int currentCoupleRounds = 0;
    
    // Dynamic bomb chance tracking
    private int consecutiveSameCoupleSwipes = 0; // Tracks swipes with same couple (Cat or Dog)
    private int totalConsecutiveSwipes = 0; // Tracks all swipes since last bomb
    private CoupleType lastNonBombCouple = CoupleType.Cat; // Track which couple was used last
    
    public bool IsUp => isUp;
    public bool IsBomb => isBomb;
    public bool IsBombActive => isBombActive;
    public CoupleType CurrentCouple => currentCouple;
    
    private void Start()
    {
        // Initialize with cat down
        ShowCatDown();
        
        // Store original position
        originalPosition = transform.localPosition;
    }
    
    void Update()
    {
        // Handle shake animation
        if (isShaking)
        {
            shakeTimer += Time.deltaTime;
            
            if (shakeTimer < shakeDuration)
            {
                // Apply shake offset
                float offsetX = Mathf.Sin(shakeTimer * shakeSpeed) * shakeIntensity;
                float offsetY = Mathf.Cos(shakeTimer * shakeSpeed * 1.5f) * shakeIntensity;
                transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);
            }
            else
            {
                // Stop shaking and reset position
                isShaking = false;
                shakeTimer = 0f;
                transform.localPosition = originalPosition;
            }
        }
        
        // Handle bomb timer
        if (!bombSystemEnabled || !isBombActive) return;
        
        bombTimer += Time.deltaTime;
        if (bombTimer >= bombTimeLimit)
        {
            // Time's up! Bomb timeout
            DeactivateBomb();
            OnBombTimeout?.Invoke();
        }
    }
    
    public void ShowDown()
    {
        if (spriteRenderer == null) return;
        
        Sprite downSprite = GetCurrentDownSprite();
        if (downSprite != null)
        {
            spriteRenderer.sprite = downSprite;
            isUp = false;
            isBomb = (currentCouple == CoupleType.Bomb);
        }
    }
    
    public void ShowUp()
    {
        if (spriteRenderer == null) return;
        
        Sprite upSprite = GetCurrentUpSprite();
        if (upSprite != null)
        {
            spriteRenderer.sprite = upSprite;
            isUp = true;
            isBomb = false;
            
            // Count this as a round completion
            currentCoupleRounds++;
            CheckForCoupleChange();
        }
    }
    
    // Legacy methods for backward compatibility
    public void ShowCatDown()
    {
        currentCouple = CoupleType.Cat;
        ShowDown();
    }
    
    public void ShowCatUp()
    {
        currentCouple = CoupleType.Cat;
        ShowUp();
    }
    
    public void ShowBombDown()
    {
        currentCouple = CoupleType.Bomb;
        if (spriteRenderer != null && bombDownSprite != null)
        {
            spriteRenderer.sprite = bombDownSprite;
            isUp = false;
            isBomb = true;
            
            // Trigger shake animation when bomb appears
            StartShake();
        }
    }
    
    public void ShowBombUp()
    {
        currentCouple = CoupleType.Bomb;
        if (spriteRenderer != null && bombUpSprite != null)
        {
            spriteRenderer.sprite = bombUpSprite;
            isUp = true;
            isBomb = true;
        }
    }
    
    public void ShowDogDown()
    {
        currentCouple = CoupleType.Dog;
        if (spriteRenderer != null && dogDownSprite != null)
        {
            spriteRenderer.sprite = dogDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowDogUp()
    {
        currentCouple = CoupleType.Dog;
        if (spriteRenderer != null && dogUpSprite != null)
        {
            spriteRenderer.sprite = dogUpSprite;
            isUp = true;
            isBomb = false;
        }
    }
    
    public void StartShake()
    {
        isShaking = true;
        shakeTimer = 0f;
        originalPosition = transform.localPosition;
    }
    
    public void Reset()
    {
        ShowCatDown();
        DeactivateBomb();
        ResetSwipeCounters();
        lastNonBombCouple = CoupleType.Cat;
    }
    
    // Bomb Management Methods
    public void EnableBombSystem(bool enable)
    {
        bombSystemEnabled = enable;
        if (!enable)
        {
            DeactivateBomb();
        }
    }
    
    public bool ShouldSpawnBomb()
    {
        // Calculate dynamic bomb chance based on consecutive swipes
        float currentBombChance = CalculateDynamicBombChance();
        bool shouldSpawn = UnityEngine.Random.value < currentBombChance;
        
        if (shouldSpawn)
        {
            Debug.Log($"BOMB SPAWNING! Chance was {currentBombChance:P0} (Same couple: {consecutiveSameCoupleSwipes}, Total: {totalConsecutiveSwipes})");
        }
        
        return shouldSpawn;
    }
    
    private float CalculateDynamicBombChance()
    {
        // Check if we've exceeded either threshold
        bool sameCoupleThresholdReached = consecutiveSameCoupleSwipes >= sameCoupleThreshold;
        bool totalSwipesThresholdReached = totalConsecutiveSwipes >= totalSwipesThreshold;
        
        if (sameCoupleThresholdReached || totalSwipesThresholdReached)
        {
            // High chance to spawn bomb
            return increasedBombChance;
        }
        
        // Normal base chance
        return bombAppearChance;
    }
    
    public void TrackSwipeDown()
    {
        // Increment total swipes counter
        totalConsecutiveSwipes++;
        
        // Track same couple swipes
        if (currentCouple == lastNonBombCouple)
        {
            consecutiveSameCoupleSwipes++;
        }
        else
        {
            // Different couple, reset same couple counter
            consecutiveSameCoupleSwipes = 1;
            lastNonBombCouple = currentCouple;
        }
        
        Debug.Log($"Swipe tracked - Same couple: {consecutiveSameCoupleSwipes}, Total: {totalConsecutiveSwipes}, Current: {currentCouple}");
    }
    
    public void ActivateBomb()
    {
        isBombActive = true;
        bombTimer = 0f;
        
        // Reset counters when bomb appears
        ResetSwipeCounters();
        
        Debug.Log("BOMB APPEARED! Swipe down within " + bombTimeLimit + " seconds!");
    }
    
    public void DeactivateBomb()
    {
        if (isBombActive)
        {
            Debug.Log("Bomb deactivated.");
        }
        isBombActive = false;
        bombTimer = 0f;
    }
    
    private void ResetSwipeCounters()
    {
        consecutiveSameCoupleSwipes = 0;
        totalConsecutiveSwipes = 0;
        Debug.Log("Swipe counters reset after bomb appearance");
    }
    
    // Helper methods for couple management
    private Sprite GetCurrentDownSprite()
    {
        switch (currentCouple)
        {
            case CoupleType.Cat:
                return catDownSprite;
            case CoupleType.Bomb:
                return bombDownSprite;
            case CoupleType.Dog:
                return dogDownSprite;
            default:
                return catDownSprite;
        }
    }
    
    private Sprite GetCurrentUpSprite()
    {
        switch (currentCouple)
        {
            case CoupleType.Cat:
                return catUpSprite;
            case CoupleType.Bomb:
                return bombUpSprite;
            case CoupleType.Dog:
                return dogUpSprite;
            default:
                return catUpSprite;
        }
    }
    
    private void CheckForCoupleChange()
    {
        // Only check for change after minimum rounds
        if (currentCoupleRounds < minRoundsPerCouple)
        {
            Debug.Log($"Current couple: {currentCouple}, Rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            return;
        }
        
        // Random chance to change couple after min rounds
        if (UnityEngine.Random.value < coupleChangeChance)
        {
            ChangeToRandomCouple();
        }
        else
        {
            Debug.Log($"Staying with {currentCouple} couple. Rounds: {currentCoupleRounds}");
        }
    }
    
    private void ChangeToRandomCouple()
    {
        CoupleType oldCouple = currentCouple;
        
        // Get a different couple type
        CoupleType[] allCouples = { CoupleType.Cat, CoupleType.Bomb, CoupleType.Dog };
        CoupleType newCouple;
        
        do
        {
            newCouple = allCouples[UnityEngine.Random.Range(0, allCouples.Length)];
        }
        while (newCouple == currentCouple);
        
        currentCouple = newCouple;
        currentCoupleRounds = 0;
        
        Debug.Log($"Couple changed from {oldCouple} to {newCouple}!");
    }
    
    public void SetCouple(CoupleType coupleType)
    {
        currentCouple = coupleType;
        currentCoupleRounds = 0;
    }
}
