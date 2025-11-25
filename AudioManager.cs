using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Game Sound Effects")]
    [SerializeField] private AudioClip catUpSound;
    [SerializeField] private AudioClip catDownSound;
    
    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonClickSound;
    
    [Header("Game Over Sound Effects")]
    [SerializeField] private AudioClip levelFailedSound; // Includes explosion sound
    [SerializeField] private AudioClip tooLateSound;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        Debug.Log("AudioManager: Awake called");
        
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.Log("AudioManager: No AudioSource found, creating one");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Debug.Log("AudioManager: AudioSource found on GameObject");
        }
        
        // Load audio clips from Resources folder if not assigned
        if (catUpSound == null)
        {
            Debug.Log("AudioManager: Attempting to load 'in' from Resources");
            catUpSound = Resources.Load<AudioClip>("in");
            if (catUpSound != null)
            {
                Debug.Log("AudioManager: Successfully loaded 'in.mp3' from Resources folder");
            }
            else
            {
                Debug.LogError("AudioManager: Could not find 'in.mp3' in Resources folder!");
            }
        }
        else
        {
            Debug.Log("AudioManager: Cat up sound already assigned in inspector");
        }
        
        if (catDownSound == null)
        {
            Debug.Log("AudioManager: Attempting to load 'out' from Resources");
            catDownSound = Resources.Load<AudioClip>("out");
            if (catDownSound != null)
            {
                Debug.Log("AudioManager: Successfully loaded 'out.mp3' from Resources folder");
            }
            else
            {
                Debug.LogError("AudioManager: Could not find 'out.mp3' in Resources folder!");
            }
        }
        else
        {
            Debug.Log("AudioManager: Cat down sound already assigned in inspector");
        }
        
        Debug.Log($"AudioManager setup complete - AudioSource: {audioSource != null}, CatUpSound: {catUpSound != null}, CatDownSound: {catDownSound != null}");
    }
    
    public void PlayCatUpSound()
    {
        Debug.Log("AudioManager: PlayCatUpSound called");
        
        if (audioSource == null)
        {
            Debug.LogError("AudioManager: AudioSource is NULL!");
            return;
        }
        
        if (catUpSound == null)
        {
            Debug.LogError("AudioManager: Cat Up Sound is NULL!");
            return;
        }
        
        Debug.Log($"AudioManager: Playing cat UP sound - Volume: {audioSource.volume}, Mute: {audioSource.mute}");
        audioSource.PlayOneShot(catUpSound);
    }
    
    public void PlayCatDownSound()
    {
        Debug.Log("AudioManager: PlayCatDownSound called");
        
        if (audioSource == null)
        {
            Debug.LogError("AudioManager: AudioSource is NULL!");
            return;
        }
        
        if (catDownSound == null)
        {
            Debug.LogError("AudioManager: Cat Down Sound is NULL!");
            return;
        }
        
        Debug.Log($"AudioManager: Playing cat DOWN sound - Volume: {audioSource.volume}, Mute: {audioSource.mute}");
        audioSource.PlayOneShot(catDownSound);
    }
    
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }
    
    /// <summary>
    /// Plays button click sound effect
    /// </summary>
    public void PlayButtonClickSound()
    {
        if (audioSource == null || buttonClickSound == null)
        {
            Debug.LogWarning("⚠️ AudioManager: Cannot play button click sound - AudioSource or clip is null");
            return;
        }
        
        audioSource.PlayOneShot(buttonClickSound);
        Debug.Log("🔊 Button click sound played");
    }
    
    /// <summary>
    /// Plays level failed sound (normal bomb explosion fail)
    /// </summary>
    public void PlayLevelFailedSound()
    {
        if (audioSource == null || levelFailedSound == null)
        {
            Debug.LogWarning("⚠️ AudioManager: Cannot play level failed sound - AudioSource or clip is null");
            return;
        }
        
        audioSource.PlayOneShot(levelFailedSound);
        Debug.Log("🔊 Level failed sound played");
    }
    
    /// <summary>
    /// Plays too late sound (timeout fail)
    /// </summary>
    public void PlayTooLateSound()
    {
        if (audioSource == null || tooLateSound == null)
        {
            Debug.LogWarning("⚠️ AudioManager: Cannot play too late sound - AudioSource or clip is null");
            return;
        }
        
        audioSource.PlayOneShot(tooLateSound);
        Debug.Log("🔊 Too late sound played");
    }
    
    /// <summary>
    /// Plays any audio clip as a one-shot sound
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    public void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("⚠️ AudioManager: Cannot play sound - AudioSource is null");
            return;
        }
        
        if (clip == null)
        {
            Debug.LogWarning("⚠️ AudioManager: Cannot play sound - AudioClip is null");
            return;
        }
        
        audioSource.PlayOneShot(clip);
    }
    
    /// <summary>
    /// Plays any audio clip as a one-shot sound with volume control
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volumeScale">Volume scale (0.0 to 1.0)</param>
    public void PlayOneShot(AudioClip clip, float volumeScale)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("⚠️ AudioManager: Cannot play sound - AudioSource is null");
            return;
        }
        
        if (clip == null)
        {
            Debug.LogWarning("⚠️ AudioManager: Cannot play sound - AudioClip is null");
            return;
        }
        
        audioSource.PlayOneShot(clip, volumeScale);
    }
}
