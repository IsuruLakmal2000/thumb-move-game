using UnityEngine;
using System.Collections;
using System;

public class ThumbVisualizer : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float downRotation = 0f;
    [SerializeField] private float upRotation = 50f;
    [SerializeField] private float rotationSpeed = 5f; // Speed of rotation animation
    
    [Header("Rhythm Settings")]
    [SerializeField] private float rhythmInterval = 0.7f; // Time between rotations
    [SerializeField] private bool autoRotate = false; // Enable/disable automatic rotation
    
    [Header("Timeout Settings")]
    [SerializeField] private float userResponseTimeout = 1.0f; // Max time for user to respond to thumb movement
    
    // Event for timeout
    public event Action OnUserTooSlow;
    
    private bool isUp = false;
    private float targetRotation = 0f;
    private Coroutine rhythmCoroutine;
    private bool isPaused = false;
    
    // Timeout tracking - tracks if cat sprite appeared in time
    private float lastThumbMoveTime = 0f;
    private float lastUserResponseTime = 0f;
    private bool waitingForCatToAppear = false;
    private bool expectingCatUp = false;
    private bool timeoutPaused = false; // Pause timeout during bomb scenarios
    
    void Start()
    {
        // Initialize at down position
        SetRotationImmediate(downRotation);
        targetRotation = downRotation;
    }
    
    void Update()
    {
        // Smoothly rotate towards target rotation
        float currentZ = transform.localEulerAngles.z;
        
        // Handle angle wrapping (Unity uses 0-360)
        if (currentZ > 180f) currentZ -= 360f;
        
        float newZ = Mathf.Lerp(currentZ, targetRotation, Time.deltaTime * rotationSpeed);
        transform.localEulerAngles = new Vector3(0, 0, newZ);

        // Check if user has been inactive for too long
        if (waitingForCatToAppear && !timeoutPaused) // Don't check timeout during bomb
        {
            float timeSinceLastResponse = Time.time - lastUserResponseTime;
            
            // Log every 0.2 seconds to show we're checking
            if (Mathf.FloorToInt(timeSinceLastResponse / 0.2f) != Mathf.FloorToInt((timeSinceLastResponse - Time.deltaTime) / 0.2f))
            {
                Debug.Log($"⏱️ Waiting for response... {timeSinceLastResponse:F2}s / {userResponseTimeout}s");
            }
            
            if (timeSinceLastResponse > userResponseTimeout)
            {
                waitingForCatToAppear = false;
                
                Debug.Log($"❌ TIMEOUT TRIGGERED! No response for {timeSinceLastResponse:F2}s > {userResponseTimeout}s");
                Debug.Log($"Expected cat to {(expectingCatUp ? "appear UP" : "appear DOWN")}");
                Debug.Log($"OnUserTooSlow null? {(OnUserTooSlow == null)}");
                
                OnUserTooSlow?.Invoke();
            }
        }
    }
    
    public void StartRhythm()
    {
        autoRotate = true;
        lastUserResponseTime = Time.time; // Reset the response timer when game starts
        Debug.Log("🟢 StartRhythm() called - autoRotate = true");
        if (rhythmCoroutine != null)
        {
            StopCoroutine(rhythmCoroutine);
        }
        rhythmCoroutine = StartCoroutine(RhythmCoroutine());
    }
    
    public void StopRhythm()
    {
        autoRotate = false;
        waitingForCatToAppear = false; // Cancel any pending timeout
        
        if (rhythmCoroutine != null)
        {
            StopCoroutine(rhythmCoroutine);
            rhythmCoroutine = null;
        }
        
        // Return to down position (without triggering timeout since autoRotate is now false)
        targetRotation = downRotation;
        isUp = false;
    }
    
    private IEnumerator RhythmCoroutine()
    {
        while (autoRotate)
        {
            // Wait if paused
            while (isPaused)
            {
                yield return null;
            }
            
            // Rotate up
            RotateUp();
            yield return new WaitForSeconds(rhythmInterval);
            
            if (!autoRotate) break;
            
            // Wait if paused
            while (isPaused)
            {
                yield return null;
            }
            
            // Rotate down
            RotateDown();
            yield return new WaitForSeconds(rhythmInterval);
        }
    }
    
    public void RotateUp()
    {
        targetRotation = upRotation;
        isUp = true;
        Debug.Log($"⬆️ RotateUp() called - autoRotate = {autoRotate}");
        StartTimeoutTracking(true);
    }
    
    public void RotateDown()
    {
        targetRotation = downRotation;
        isUp = false;
        Debug.Log($"⬇️ RotateDown() called - autoRotate = {autoRotate}");
        StartTimeoutTracking(false);
    }
    
    public void RotateToPosition(bool up)
    {
        if (up)
        {
            RotateUp();
        }
        else
        {
            RotateDown();
        }
    }
    
    private void SetRotationImmediate(float rotation)
    {
        transform.localEulerAngles = new Vector3(0, 0, rotation);
    }
    
    public void Reset()
    {
        StopRhythm();
        SetRotationImmediate(downRotation);
        targetRotation = downRotation;
        isUp = false;
        isPaused = false;
        waitingForCatToAppear = false;
    }
    
    public void PauseRhythm()
    {
        isPaused = true;
    }
    
    public void ResumeRhythm()
    {
        isPaused = false;
    }
    
    public void FreezeForDuration(float duration)
    {
        StartCoroutine(FreezeCoroutine(duration));
    }
    
    private IEnumerator FreezeCoroutine(float duration)
    {
        PauseRhythm();
        timeoutPaused = true; // Pause timeout tracking during bomb
        Debug.Log("ThumbVisualizer: Timeout PAUSED (bomb appeared)");
        
        yield return new WaitForSeconds(duration);
        
        timeoutPaused = false; // Resume timeout tracking after bomb
        lastUserResponseTime = Time.time; // Reset timer so user isn't immediately penalized
        Debug.Log("ThumbVisualizer: Timeout RESUMED (bomb cleared)");
        ResumeRhythm();
    }
    
    public bool IsUp => isUp;

    // Called by GameManager when cat sprite appears (user successfully swiped and sprite changed)
    public void OnCatSpriteAppeared(bool catIsUp)
    {
        // Cat sprite appeared, update last response time
        lastUserResponseTime = Time.time;
        Debug.Log($"✅ User responded! Reset timeout timer.");
        
        // Cancel timeout tracking if in correct direction
        if (waitingForCatToAppear)
        {
            bool correctDirection = (catIsUp && expectingCatUp) || (!catIsUp && !expectingCatUp);
            if (correctDirection)
            {
                waitingForCatToAppear = false;
            }
        }
    }

    private void StartTimeoutTracking(bool expectUp)
    {
        Debug.Log($"🔵 StartTimeoutTracking() called - autoRotate = {autoRotate}, expectUp = {expectUp}");
        
        if (autoRotate) // Only track timeout when in auto rhythm mode
        {
            // Start tracking on first move, then keep tracking continuously
            if (!waitingForCatToAppear)
            {
                waitingForCatToAppear = true;
                Debug.Log($"✅ Timeout tracking STARTED - timeout after {userResponseTimeout}s of inactivity");
            }
            
            // Update what we're expecting
            lastThumbMoveTime = Time.time;
            expectingCatUp = expectUp;
        }
        else
        {
            Debug.Log("⚠️ Timeout tracking NOT started - autoRotate is false");
        }
    }
}
