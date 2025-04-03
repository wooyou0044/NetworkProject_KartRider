using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    AudioSource audioSource;
    [SerializeField] AudioClip[] audioClips;

    public static SoundManager Instance { get => instance; }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        audioSource.loop = true;
    }

    public void PlayBGM(int sceneIndex)
    {
        if(sceneIndex >= 0 && sceneIndex <audioClips.Length)
        {
            audioSource.clip = audioClips[sceneIndex];
            audioSource.Play();
        }
    }
}
