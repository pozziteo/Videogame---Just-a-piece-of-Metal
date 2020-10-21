using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour
{
    public static BackgroundMusicPlayer MusicPlayer
    {
        get
        {
            return musicPlayer;
        }
    }
    static BackgroundMusicPlayer musicPlayer;
    public List<AudioClip> tracks;
    AudioSource m_AudioSource;
    static int CurrentIndexPlaying = 0;

    void Awake()
    {
        if (musicPlayer == null)
        {
            musicPlayer = this;
            DontDestroyOnLoad(this);
            m_AudioSource = GetComponent<AudioSource>();
            StartCoroutine(PlayList());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator PlayList()
    {
        while (true)
        {
            yield return new WaitForSeconds(tracks[CurrentIndexPlaying].length);
            CurrentIndexPlaying = (CurrentIndexPlaying + 1) % tracks.Count;
            m_AudioSource.clip = tracks[CurrentIndexPlaying];
            m_AudioSource.Play();
        }
    }


}
