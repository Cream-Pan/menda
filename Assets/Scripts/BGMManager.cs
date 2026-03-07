using System;
using UnityEngine;
using UnityEngine.Audio;

public class BGMManager : MonoBehaviour
{
    public AudioSource bgmAudioSource;
    private Boolean isMute = false;
    [SerializeField] GameObject sura;

    public AudioClip StartSound;
    public AudioClip TypingSound;
    public AudioClip CorrectSound;
    public AudioClip ResultSound;
    public AudioClip RestartSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StopBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }
    }

    public void Mute()
    {
        if (isMute)
        {
            sura.SetActive(false);
            AudioListener.volume = 1f;
            isMute = false;
        }
        else
        {
            sura.SetActive(true);
            AudioListener.volume = 0f;
            isMute = true;
        }
    }

    public void PressStart()
    {
        PlaySound(StartSound);
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
