using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int currentScore = 0;
    
    public int CurrentScore => currentScore;
    
    public void AddPoint()
    {
        currentScore++;
        Debug.Log("Score: " + currentScore);
    }
    
    public void ResetScore()
    {
        currentScore = 0;
    }
}
