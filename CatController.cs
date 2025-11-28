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
    [SerializeField] private Sprite pigDownSprite;
    [SerializeField] private Sprite pigUpSprite;
    [SerializeField] private Sprite catSnakeDownSprite;
    [SerializeField] private Sprite catSnakeUpSprite;
    
    [Header("Santa Couple Sprites")]
    [SerializeField] private Sprite santaDollDownSprite;
    [SerializeField] private Sprite santaDollUpSprite;
    [SerializeField] private Sprite santaBombDownSprite;
    [SerializeField] private Sprite santaBombUpSprite;
    [SerializeField] private Sprite santaSockCatDownSprite;
    [SerializeField] private Sprite santaSockCatUpSprite;
    [SerializeField] private Sprite ckBombDownSprite;
    [SerializeField] private Sprite ckBombUpSprite;
    
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
    
    // Regular couples for normal gameplay
    public enum CoupleType { Cat, Bomb, Dog, Pig, CatSnake, SantaDoll, SantaSockCat }
    
    // Bomb variants - these are different visual styles for the bomb obstacle (Default, SantaBomb, CKBomb)
    public enum BombVariant { Default, SantaBomb, CKBomb }
    
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
    private CoupleType coupleBeforeBomb = CoupleType.Cat; // Store couple before bomb appears
    private BombVariant currentBombVariant = BombVariant.Default; // Track which bomb variant is showing
    
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
            
            // Count this as a round completion (one complete DOWN->UP cycle)
            currentCoupleRounds++;
            Debug.Log($"✅ Round completed for {currentCouple}. Total rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            CheckForCoupleChange();
        }
    }
    
    // Legacy methods - these DO NOT change the couple type, just show the sprite
    public void ShowCatDown()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && catDownSprite != null)
        {
            spriteRenderer.sprite = catDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowCatUp()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && catUpSprite != null)
        {
            spriteRenderer.sprite = catUpSprite;
            isUp = true;
            isBomb = false;
            
            // Count round and check for change
            currentCoupleRounds++;
            Debug.Log($"✅ Round completed for {currentCouple}. Total rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            CheckForCoupleChange();
        }
    }
    
    public void ShowBombDown()
    {
        // DON'T change currentCouple - bomb is temporary, keep the active couple
        // Store the current couple so we can restore it after bomb
        coupleBeforeBomb = currentCouple;
        
        // Randomly select a bomb variant (Default, SantaBomb, or CKBomb)
        BombVariant[] variants = { BombVariant.Default, BombVariant.SantaBomb, BombVariant.CKBomb };
        currentBombVariant = variants[UnityEngine.Random.Range(0, variants.Length)];
        
        // Get the appropriate down sprite for this bomb variant
        Sprite bombSprite = GetBombDownSprite(currentBombVariant);
        
        if (spriteRenderer != null && bombSprite != null)
        {
            spriteRenderer.sprite = bombSprite;
            isUp = false;
            isBomb = true;
            
            // Trigger shake animation when bomb appears
            StartShake();
            
            Debug.Log($"💣 Bomb appeared ({currentBombVariant})! Preserving couple: {currentCouple} (rounds: {currentCoupleRounds})");
        }
    }
    
    /// <summary>
    /// Gets the down sprite for the specified bomb variant
    /// </summary>
    private Sprite GetBombDownSprite(BombVariant variant)
    {
        switch (variant)
        {
            case BombVariant.SantaBomb:
                return santaBombDownSprite;
            case BombVariant.CKBomb:
                return ckBombDownSprite;
            case BombVariant.Default:
            default:
                return bombDownSprite;
        }
    }
    
    /// <summary>
    /// Gets the up sprite for the specified bomb variant
    /// </summary>
    private Sprite GetBombUpSprite(BombVariant variant)
    {
        switch (variant)
        {
            case BombVariant.SantaBomb:
                return santaBombUpSprite;
            case BombVariant.CKBomb:
                return ckBombUpSprite;
            case BombVariant.Default:
            default:
                return bombUpSprite;
        }
    }
    
    public void ShowBombUp()
    {
        // DON'T change currentCouple - just show the matching bomb variant up sprite
        Sprite bombUpSpriteToShow = GetBombUpSprite(currentBombVariant);
        
        if (spriteRenderer != null && bombUpSpriteToShow != null)
        {
            spriteRenderer.sprite = bombUpSpriteToShow;
            isUp = true;
            isBomb = true;
        }
    }
    
    public void ShowDogDown()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && dogDownSprite != null)
        {
            spriteRenderer.sprite = dogDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowDogUp()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && dogUpSprite != null)
        {
            spriteRenderer.sprite = dogUpSprite;
            isUp = true;
            isBomb = false;
            
            // Count round and check for change
            currentCoupleRounds++;
            Debug.Log($"✅ Round completed for {currentCouple}. Total rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            CheckForCoupleChange();
        }
    }
    
    public void ShowPigDown()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && pigDownSprite != null)
        {
            spriteRenderer.sprite = pigDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowPigUp()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && pigUpSprite != null)
        {
            spriteRenderer.sprite = pigUpSprite;
            isUp = true;
            isBomb = false;
            
            // Count round and check for change
            currentCoupleRounds++;
            Debug.Log($"✅ Round completed for {currentCouple}. Total rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            CheckForCoupleChange();
        }
    }
    
    public void ShowCatSnakeDown()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && catSnakeDownSprite != null)
        {
            spriteRenderer.sprite = catSnakeDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowCatSnakeUp()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && catSnakeUpSprite != null)
        {
            spriteRenderer.sprite = catSnakeUpSprite;
            isUp = true;
            isBomb = false;
            
            // Count round and check for change
            currentCoupleRounds++;
            Debug.Log($"✅ Round completed for {currentCouple}. Total rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            CheckForCoupleChange();
        }
    }
    
    // ====== SANTA DOLL COUPLE ======
    public void ShowSantaDollDown()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && santaDollDownSprite != null)
        {
            spriteRenderer.sprite = santaDollDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowSantaDollUp()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && santaDollUpSprite != null)
        {
            spriteRenderer.sprite = santaDollUpSprite;
            isUp = true;
            isBomb = false;
            
            // Count round and check for change
            currentCoupleRounds++;
            Debug.Log($"✅ Round completed for {currentCouple}. Total rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            CheckForCoupleChange();
        }
    }
    
    // ====== SANTA BOMB COUPLE ======
    public void ShowSantaBombDown()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && santaBombDownSprite != null)
        {
            spriteRenderer.sprite = santaBombDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowSantaBombUp()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && santaBombUpSprite != null)
        {
            spriteRenderer.sprite = santaBombUpSprite;
            isUp = true;
            isBomb = false;
            
            // Count round and check for change
            currentCoupleRounds++;
            Debug.Log($"✅ Round completed for {currentCouple}. Total rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            CheckForCoupleChange();
        }
    }
    
    // ====== SANTA SOCK CAT COUPLE ======
    public void ShowSantaSockCatDown()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && santaSockCatDownSprite != null)
        {
            spriteRenderer.sprite = santaSockCatDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowSantaSockCatUp()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && santaSockCatUpSprite != null)
        {
            spriteRenderer.sprite = santaSockCatUpSprite;
            isUp = true;
            isBomb = false;
            
            // Count round and check for change
            currentCoupleRounds++;
            Debug.Log($"✅ Round completed for {currentCouple}. Total rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            CheckForCoupleChange();
        }
    }
    
    // ====== CK BOMB COUPLE ======
    public void ShowCKBombDown()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && ckBombDownSprite != null)
        {
            spriteRenderer.sprite = ckBombDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowCKBombUp()
    {
        // Only show sprite, don't change couple type
        if (spriteRenderer != null && ckBombUpSprite != null)
        {
            spriteRenderer.sprite = ckBombUpSprite;
            isUp = true;
            isBomb = false;
            
            // Count round and check for change
            currentCoupleRounds++;
            Debug.Log($"✅ Round completed for {currentCouple}. Total rounds: {currentCoupleRounds}/{minRoundsPerCouple}");
            CheckForCoupleChange();
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
        // Reset to Cat couple at start
        currentCouple = CoupleType.Cat;
        currentCoupleRounds = 0;
        
        ShowCatDown();
        DeactivateBomb();
        ResetSwipeCounters();
        lastNonBombCouple = CoupleType.Cat;
        
        Debug.Log($"🔄 CatController Reset - Starting with {currentCouple} couple");
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
        // IMPORTANT: Don't allow bombs until current couple has completed minimum rounds
        if (currentCoupleRounds < minRoundsPerCouple)
        {
            Debug.Log($"🚫 BOMB BLOCKED! Current couple ({currentCouple}) only has {currentCoupleRounds}/{minRoundsPerCouple} rounds");
            return false;
        }
        
        // Calculate dynamic bomb chance based on consecutive swipes
        float currentBombChance = CalculateDynamicBombChance();
        bool shouldSpawn = UnityEngine.Random.value < currentBombChance;
        
        if (shouldSpawn)
        {
            Debug.Log($"💣 BOMB SPAWNING! Chance was {currentBombChance:P0} (Same couple: {consecutiveSameCoupleSwipes}, Total: {totalConsecutiveSwipes})");
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
            case CoupleType.Pig:
                return pigDownSprite;
            case CoupleType.CatSnake:
                return catSnakeDownSprite;
            case CoupleType.SantaDoll:
                return santaDollDownSprite;
            case CoupleType.SantaSockCat:
                return santaSockCatDownSprite;
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
            case CoupleType.Pig:
                return pigUpSprite;
            case CoupleType.CatSnake:
                return catSnakeUpSprite;
            case CoupleType.SantaDoll:
                return santaDollUpSprite;
            case CoupleType.SantaSockCat:
                return santaSockCatUpSprite;
            default:
                return catUpSprite;
        }
    }
    
    private void CheckForCoupleChange()
    {
        // Only check for change after minimum rounds
        if (currentCoupleRounds < minRoundsPerCouple)
        {
            Debug.Log($"⏳ Current couple: {currentCouple}, Rounds: {currentCoupleRounds}/{minRoundsPerCouple} - Need more rounds before switching");
            return;
        }
        
        // We've reached the minimum, now there's a random chance to change
        float roll = UnityEngine.Random.value;
        Debug.Log($"🎲 Couple change check: {currentCouple} has {currentCoupleRounds} rounds. Roll: {roll:F2} vs {coupleChangeChance:F2}");
        
        if (roll < coupleChangeChance)
        {
            ChangeToRandomCouple();
        }
        else
        {
            Debug.Log($"✋ Staying with {currentCouple} couple. Rounds: {currentCoupleRounds}");
        }
    }
    
    private void ChangeToRandomCouple()
    {
        CoupleType oldCouple = currentCouple;
        int oldRounds = currentCoupleRounds;
        
        // Get a different couple type (excluding Bomb type - bomb variants are handled separately)
        CoupleType[] allCouples = { CoupleType.Cat, CoupleType.Dog, CoupleType.Pig, CoupleType.CatSnake, 
                                     CoupleType.SantaDoll, CoupleType.SantaSockCat };
        CoupleType newCouple;
        
        do
        {
            newCouple = allCouples[UnityEngine.Random.Range(0, allCouples.Length)];
        }
        while (newCouple == currentCouple);
        
        currentCouple = newCouple;
        currentCoupleRounds = 0;
        
        Debug.Log($"🔄 COUPLE CHANGED! {oldCouple} → {newCouple} (completed {oldRounds} rounds with {oldCouple})");
    }
    
    public void SetCouple(CoupleType coupleType)
    {
        currentCouple = coupleType;
        currentCoupleRounds = 0;
    }
}
