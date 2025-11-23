using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip catUpSound;
    [SerializeField] private AudioClip catDownSound;
    
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
}
