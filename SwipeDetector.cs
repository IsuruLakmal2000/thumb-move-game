using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class SwipeDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float swipeThreshold = 50f;
    
    public event Action OnSwipeUp;
    public event Action OnSwipeDown;
    
    private Vector2 lastTouchPosition;
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
            if (swipeDelta.y > 0)
            {
                // Swipe up
                OnSwipeUp?.Invoke();
            }
            else
            {
                // Swipe down
                OnSwipeDown?.Invoke();
            }
        }
    }
}
