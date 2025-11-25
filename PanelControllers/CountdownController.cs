using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CountdownController : MonoBehaviour
{
    [Header("Countdown Sprites")]
    [SerializeField] private Sprite countdown3Sprite;
    [SerializeField] private Sprite countdown2Sprite;
    [SerializeField] private Sprite countdown1Sprite;
    [SerializeField] private Sprite countdownGoSprite;
    
    [Header("UI References")]
    [SerializeField] private Image countdownImage;
    [SerializeField] private GameObject countdownPanel;
    
    [Header("Animation Settings")]
    [SerializeField] private AnimationType animationType = AnimationType.ScalePulse;
    [SerializeField] private float animationDuration = 0.8f;
    [SerializeField] private float scaleAmount = 1.5f; // How much to scale up
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip countdownBeepSound; // Optional: beep sound for 3, 2, 1
    [SerializeField] private AudioClip countdownGoSound; // Optional: different sound for GO
    
    public enum AnimationType
    {
        ScalePulse,      // Scale up then down
        FadeIn,          // Fade from transparent to opaque
        ScaleAndFade,    // Both scale and fade
        Bounce,          // Bouncy scale effect
        Rotate           // Rotate while scaling
    }
    
    private Vector3 originalScale;
    private Coroutine currentAnimation;
    
    private void Start()
    {
        // Store original scale and setup image
        if (countdownImage != null)
        {
            originalScale = countdownImage.transform.localScale;
            // Preserve aspect ratio to prevent stretching
            countdownImage.preserveAspect = true;
        }
        
        // Hide at start
        HideCountdown();
    }
    
    /// <summary>
    /// Shows countdown with number (3, 2, 1) or "GO"
    /// </summary>
    /// <param name="number">3, 2, 1, or 0 for "GO"</param>
    public void ShowCountdown(int number)
    {
        if (countdownImage == null) return;
        
        // Select appropriate sprite
        Sprite spriteToShow = null;
        AudioClip soundToPlay = null;
        
        switch (number)
        {
            case 3:
                spriteToShow = countdown3Sprite;
                soundToPlay = countdownBeepSound;
                break;
            case 2:
                spriteToShow = countdown2Sprite;
                soundToPlay = countdownBeepSound;
                break;
            case 1:
                spriteToShow = countdown1Sprite;
                soundToPlay = countdownBeepSound;
                break;
            case 0: // GO!
                spriteToShow = countdownGoSprite;
                soundToPlay = countdownGoSound ?? countdownBeepSound;
                break;
            default:
                Debug.LogWarning($"Invalid countdown number: {number}");
                return;
        }
        
        // Stop any existing animation first
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        
        // Show panel immediately
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }
        
        // Set sprite and ensure it's active
        if (spriteToShow != null)
        {
            countdownImage.sprite = spriteToShow;
            countdownImage.gameObject.SetActive(true);
            countdownImage.enabled = true;
            
            // Preserve aspect ratio to prevent stretching
            countdownImage.preserveAspect = true;
            
            // Set to native size to maintain original proportions
            // This ensures different sized sprites display correctly
            countdownImage.SetNativeSize();
            
            // Apply the original scale to maintain consistent sizing
            countdownImage.transform.localScale = originalScale;
        }
        else
        {
            Debug.LogWarning($"Missing sprite for countdown number: {number}");
        }
        
        // Play sound
        PlayCountdownSound(soundToPlay);
        
        // Start animation
        currentAnimation = StartCoroutine(AnimateCountdown());
        
        Debug.Log($"🔢 Countdown showing: {(number == 0 ? "GO" : number.ToString())} - Sprite: {(spriteToShow != null ? spriteToShow.name : "NULL")}");
    }
    
    /// <summary>
    /// Hides the countdown display
    /// </summary>
    public void HideCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
        
        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(false);
        }
        
        // Stop animation if running
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }
    
    private IEnumerator AnimateCountdown()
    {
        if (countdownImage == null) yield break;
        
        float elapsedTime = 0f;
        Transform imageTransform = countdownImage.transform;
        
        // Reset to original state
        imageTransform.localScale = originalScale;
        imageTransform.localRotation = Quaternion.identity;
        
        // Preserve aspect ratio by setting native size
        countdownImage.preserveAspect = true;
        
        // Get or add CanvasGroup for fade effects
        CanvasGroup canvasGroup = countdownImage.GetComponent<CanvasGroup>();
        if (countdownImage.TryGetComponent<CanvasGroup>(out canvasGroup))
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            // Add CanvasGroup if needed for fade effects
            if (animationType == AnimationType.FadeIn || animationType == AnimationType.ScaleAndFade)
            {
                canvasGroup = countdownImage.gameObject.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
            }
        }
        
        // Animate based on type
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / animationDuration;
            float curveValue = animationCurve.Evaluate(normalizedTime);
            
            switch (animationType)
            {
                case AnimationType.ScalePulse:
                    AnimateScalePulse(imageTransform, normalizedTime, curveValue);
                    break;
                    
                case AnimationType.FadeIn:
                    AnimateFadeIn(canvasGroup, normalizedTime);
                    break;
                    
                case AnimationType.ScaleAndFade:
                    AnimateScaleAndFade(imageTransform, canvasGroup, normalizedTime, curveValue);
                    break;
                    
                case AnimationType.Bounce:
                    AnimateBounce(imageTransform, normalizedTime);
                    break;
                    
                case AnimationType.Rotate:
                    AnimateRotate(imageTransform, normalizedTime, curveValue);
                    break;
            }
            
            yield return null;
        }
        
        // Ensure final state
        imageTransform.localScale = originalScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
    
    private void AnimateScalePulse(Transform transform, float time, float curveValue)
    {
        // Scale up then back down
        float scale = 1f + (scaleAmount - 1f) * Mathf.Sin(curveValue * Mathf.PI);
        transform.localScale = originalScale * scale;
    }
    
    private void AnimateFadeIn(CanvasGroup canvasGroup, float time)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = time;
        }
    }
    
    private void AnimateScaleAndFade(Transform transform, CanvasGroup canvasGroup, float time, float curveValue)
    {
        // Start small and transparent, grow to full size and opaque
        float scale = curveValue * scaleAmount;
        transform.localScale = originalScale * scale;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = curveValue;
        }
    }
    
    private void AnimateBounce(Transform transform, float time)
    {
        // Bouncy effect using sine waves
        float bounce = Mathf.Abs(Mathf.Sin(time * Mathf.PI * 2f));
        float scale = 1f + bounce * (scaleAmount - 1f);
        transform.localScale = originalScale * scale;
    }
    
    private void AnimateRotate(Transform transform, float time, float curveValue)
    {
        // Rotate while scaling
        float scale = 1f + (scaleAmount - 1f) * Mathf.Sin(curveValue * Mathf.PI);
        transform.localScale = originalScale * scale;
        
        // Rotate 360 degrees
        float rotation = time * 360f;
        transform.localRotation = Quaternion.Euler(0, 0, rotation);
    }
    
    private void PlayCountdownSound(AudioClip clip)
    {
        if (audioManager != null && clip != null)
        {
            // Use a helper method if AudioManager supports it
            audioManager.PlayOneShot(clip);
        }
    }
}
