using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI levelFailedText;
    [SerializeField] private TextMeshProUGUI scoreText;
    
    private void Start()
    {
        HideCountdown();
        HideLevelFailed();
    }
    
    public void ShowCountdown(string text)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = text;
        }
    }
    
    public void HideCountdown()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }
    
    public void ShowLevelFailed()
    {
        if (levelFailedText != null)
        {
            levelFailedText.gameObject.SetActive(true);
            levelFailedText.text = "LEVEL FAILED!";
        }
    }
    
    public void HideLevelFailed()
    {
        if (levelFailedText != null)
        {
            levelFailedText.gameObject.SetActive(false);
        }
    }
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
}
