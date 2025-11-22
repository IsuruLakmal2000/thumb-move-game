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
    
    [Header("Bomb Settings")]
    [SerializeField] private float bombAppearChance = 0.3f; // 30% chance
    [SerializeField] private float bombTimeLimit = 1f; // 1 second to react
    
    public event Action OnBombTimeout;
    
    private bool isUp = false;
    private bool isBomb = false;
    private bool isBombActive = false;
    private float bombTimer = 0f;
    private bool bombSystemEnabled = false;
    
    public bool IsUp => isUp;
    public bool IsBomb => isBomb;
    public bool IsBombActive => isBombActive;
    
    private void Start()
    {
        // Initialize with cat down
        ShowCatDown();
    }
    
    void Update()
    {
        if (!bombSystemEnabled || !isBombActive) return;
        
        bombTimer += Time.deltaTime;
        if (bombTimer >= bombTimeLimit)
        {
            // Time's up! Bomb timeout
            DeactivateBomb();
            OnBombTimeout?.Invoke();
        }
    }
    
    public void ShowCatDown()
    {
        if (spriteRenderer != null && catDownSprite != null)
        {
            spriteRenderer.sprite = catDownSprite;
            isUp = false;
            isBomb = false;
        }
    }
    
    public void ShowCatUp()
    {
        if (spriteRenderer != null && catUpSprite != null)
        {
            spriteRenderer.sprite = catUpSprite;
            isUp = true;
            isBomb = false;
        }
    }
    
    public void ShowBombDown()
    {
        if (spriteRenderer != null && bombDownSprite != null)
        {
            spriteRenderer.sprite = bombDownSprite;
            isUp = false;
            isBomb = true;
        }
    }
    
    public void ShowBombUp()
    {
        if (spriteRenderer != null && bombUpSprite != null)
        {
            spriteRenderer.sprite = bombUpSprite;
            isUp = true;
            isBomb = true;
        }
    }
    
    public void Reset()
    {
        ShowCatDown();
        DeactivateBomb();
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
        return UnityEngine.Random.value < bombAppearChance;
    }
    
    public void ActivateBomb()
    {
        isBombActive = true;
        bombTimer = 0f;
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
}
