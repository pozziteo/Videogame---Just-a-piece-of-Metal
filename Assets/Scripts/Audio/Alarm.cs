using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm : MonoBehaviour
{
    public static Alarm Instance
    {
        get 
        {
            return instance;
        }
    }
    static Alarm instance;
    static int CurrentPlayingIndex = 0;
    public List<AudioClip> alarmVoiceClips;
    AudioSource m_AudioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            m_AudioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
            StartCoroutine(PlayAlarm());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator PlayAlarm()
    {
        while (true)
        {
            yield return new WaitForSeconds(alarmVoiceClips[CurrentPlayingIndex].length + 0.5f);
            if (CurrentPlayingIndex == alarmVoiceClips.Count - 1)
            {
                yield return new WaitForSeconds(3.0f);
            }
            CurrentPlayingIndex = (CurrentPlayingIndex + 1) % alarmVoiceClips.Count;
            m_AudioSource.clip = alarmVoiceClips[CurrentPlayingIndex];
            m_AudioSource.Play();
        }
    }
    
}
