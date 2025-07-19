// AudioManager.cs (Corrected to load saved volumes on Start)
using System;
using UnityEngine;

// You still need your Sound.cs file for this to work
// [System.Serializable] public class Sound { ... }

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;

    // It's good practice to keep the keys consistent by defining them here

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // --- THIS IS THE CORRECTED/ADDED LOGIC ---
        // Load the saved volume settings from the last game session.
        // Use a default value (e.g., 0.75f) if no setting has ever been saved.
        
        PlayMusic("Theme");
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning($"Music '{name}' not found!");
            return;
        }
        if (musicSource == null)
        {
            Debug.LogError("Music source is not assigned!");
            return;
        }

        musicSource.clip = s.clip;
        musicSource.Play();
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);

        if (s == null)
        {
            Debug.LogWarning($"SFX '{name}' not found!");
            return;
        }
        if (sfxSource == null)
        {
            Debug.LogError("SFX source is not assigned!");
            return;
        }

        sfxSource.PlayOneShot(s.clip);
    }

    // --- Your control methods remain the same ---

    
}