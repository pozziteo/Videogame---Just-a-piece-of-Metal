using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour
{
    public List<AudioClip> tracks;
    AudioSource m_AudioSource;
    static int CurrentIndexPlaying = 0;

    void Awake()
    {
        DontDestroyOnLoad(this);
        m_AudioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayList());
    }

    IEnumerator PlayList()
    {
        yield return new WaitForSeconds(tracks[CurrentIndexPlaying].length);
        CurrentIndexPlaying = (CurrentIndexPlaying + 1) % tracks.Count;
        m_AudioSource.clip = tracks[CurrentIndexPlaying];
        m_AudioSource.Play();
    }


}
