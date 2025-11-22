using UnityEngine;
using System.Collections;

public class ThumbVisualizer : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float downRotation = 0f;
    [SerializeField] private float upRotation = 50f;
    [SerializeField] private float rotationSpeed = 5f; // Speed of rotation animation
    
    [Header("Rhythm Settings")]
    [SerializeField] private float rhythmInterval = 0.7f; // Time between rotations
    [SerializeField] private bool autoRotate = false; // Enable/disable automatic rotation
    
    private bool isUp = false;
    private float targetRotation = 0f;
    private Coroutine rhythmCoroutine;
    private bool isPaused = false;
    
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
    }
    
    public void StartRhythm()
    {
        autoRotate = true;
        if (rhythmCoroutine != null)
        {
            StopCoroutine(rhythmCoroutine);
        }
        rhythmCoroutine = StartCoroutine(RhythmCoroutine());
    }
    
    public void StopRhythm()
    {
        autoRotate = false;
        if (rhythmCoroutine != null)
        {
            StopCoroutine(rhythmCoroutine);
            rhythmCoroutine = null;
        }
        // Return to down position
        RotateDown();
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
        Debug.Log("Thumb rotating UP to " + upRotation + " degrees");
    }
    
    public void RotateDown()
    {
        targetRotation = downRotation;
        isUp = false;
        Debug.Log("Thumb rotating DOWN to " + downRotation + " degrees");
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
    }
    
    public void PauseRhythm()
    {
        isPaused = true;
        Debug.Log("Thumb rhythm PAUSED");
    }
    
    public void ResumeRhythm()
    {
        isPaused = false;
        Debug.Log("Thumb rhythm RESUMED");
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
}
