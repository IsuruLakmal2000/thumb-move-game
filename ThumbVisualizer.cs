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
    [SerializeField] private float userResponseTimeout = 0.85f; // Max time for user to respond to thumb movement
    
    // Event for timeout
    public event Action OnUserTooSlow;
    
    private bool isUp = false;
    private float targetRotation = 0f;
    private Coroutine rhythmCoroutine;
    private bool isPaused = false;
    
    // Timeout tracking - tracks if cat sprite appeared in time
    private float lastThumbMoveTime = 0f;
    private bool waitingForCatToAppear = false;
    private bool expectingCatUp = false;
    
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

        // Check if cat sprite appeared in time after thumb moved
        if (waitingForCatToAppear)
        {
            float timeElapsed = Time.time - lastThumbMoveTime;
            
            // Log every 0.2 seconds to show we're checking
            if (Mathf.FloorToInt(timeElapsed / 0.2f) != Mathf.FloorToInt((timeElapsed - Time.deltaTime) / 0.2f))
            {
                Debug.Log($"⏱️ Waiting... {timeElapsed:F2}s / {userResponseTimeout}s");
            }
            
            if (timeElapsed > userResponseTimeout)
            {
                waitingForCatToAppear = false;
                
                Debug.Log($"❌ TIMEOUT TRIGGERED! Time: {timeElapsed:F2}s > {userResponseTimeout}s");
                Debug.Log($"Expected cat to {(expectingCatUp ? "appear UP" : "appear DOWN")}");
                Debug.Log($"OnUserTooSlow null? {(OnUserTooSlow == null)}");
                
                OnUserTooSlow?.Invoke();
            }
        }
    }
    
    public void StartRhythm()
    {
        autoRotate = true;
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
        yield return new WaitForSeconds(duration);
        ResumeRhythm();
    }
    
    public bool IsUp => isUp;

    // Called by GameManager when cat sprite appears (user successfully swiped and sprite changed)
    public void OnCatSpriteAppeared(bool catIsUp)
    {
        // Cat sprite appeared, cancel timeout tracking
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
            // If already waiting for a response, trigger timeout for previous movement first
            if (waitingForCatToAppear)
            {
                Debug.Log($"⚠️ Previous timeout still active! Triggering timeout for previous movement.");
                OnUserTooSlow?.Invoke();
            }
            
            lastThumbMoveTime = Time.time;
            waitingForCatToAppear = true;
            expectingCatUp = expectUp;
            Debug.Log($"✅ Timeout tracking STARTED - waitingForCatToAppear = {waitingForCatToAppear}, timeout = {userResponseTimeout}s");
        }
        else
        {
            Debug.Log("⚠️ Timeout tracking NOT started - autoRotate is false");
        }
    }
}
