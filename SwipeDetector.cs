using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class SwipeDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float swipeThreshold = 50f; // Minimum distance for a swipe
    
    public event Action OnSwipeUp;
    public event Action OnSwipeDown;
    public event Action OnTouchEnded; // New event for when touch/click ends
    
    private Vector2 touchStartPosition; // Position where touch/click started
    private Vector2 lastSwipePosition; // Last position where a swipe was detected
    private bool isTouching = false;
    private bool isEnabled = false;
    
    public void EnableSwipeDetection(bool enable)
    {
        isEnabled = enable;
        if (!enable)
        {
            isTouching = false;
        }
    }
    
    void Update()
    {
        if (!isEnabled) return;
        
        HandleTouchInput();
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
                touchStartPosition = touch.position.ReadValue();
                lastSwipePosition = touchStartPosition;
                isTouching = true;
            }
            
            // Touch is being held - detect continuous swipes
            if (touch.press.isPressed && isTouching)
            {
                Vector2 currentTouchPosition = touch.position.ReadValue();
                DetectContinuousSwipe(currentTouchPosition);
            }
            
            // Touch ended
            if (touch.press.wasReleasedThisFrame)
            {
                isTouching = false;
                OnTouchEnded?.Invoke(); // Notify that touch has ended
            }
        }
        // Fallback to mouse for testing in editor
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                touchStartPosition = Mouse.current.position.ReadValue();
                lastSwipePosition = touchStartPosition;
                isTouching = true;
            }
            
            if (Mouse.current.leftButton.isPressed && isTouching)
            {
                Vector2 currentTouchPosition = Mouse.current.position.ReadValue();
                DetectContinuousSwipe(currentTouchPosition);
            }
            
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isTouching = false;
                OnTouchEnded?.Invoke(); // Notify that touch has ended
            }
        }
    }
    
    private void DetectContinuousSwipe(Vector2 currentPos)
    {
        // Calculate delta from the last swipe detection point
        Vector2 swipeDelta = currentPos - lastSwipePosition;
        
        // Check if movement is significant enough
        if (swipeDelta.magnitude < swipeThreshold)
        {
            return;
        }
        
        // Determine if it's a vertical swipe
        if (Mathf.Abs(swipeDelta.y) > Mathf.Abs(swipeDelta.x))
        {
            if (swipeDelta.y > 0)
            {
                // Swipe up detected
                OnSwipeUp?.Invoke();
                // Update last swipe position to current position
                lastSwipePosition = currentPos;
            }
            else
            {
                // Swipe down detected
                OnSwipeDown?.Invoke();
                // Update last swipe position to current position
                lastSwipePosition = currentPos;
            }
        }
    }
}
