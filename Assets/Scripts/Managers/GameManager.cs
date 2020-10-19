﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }
    public bool GamePaused
    {
        get
        {
            return m_GamePaused;
        }
    }
    
    static GameManager instance;
    public GameObject pauseGameObject;
    public GameObject alarmObject;
    CinemachineVirtualCamera mainCamera;
    GameObject m_PauseCanvas;
    GameObject m_Alarm;
    bool m_GamePaused;
    bool m_GameStarted;
    bool m_AlarmPlaying;
    string m_NextDoorLevel;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            mainCamera = GameObject.Find("Cameras").GetComponentInChildren<CinemachineVirtualCamera>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!m_GamePaused)
            {
                PauseGame();
            }
            else
            {
                RestartGame();
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (m_GameStarted)
        {
            DoorBehaviour otherDoor = DoorsManager.Instance.FindDoor(m_NextDoorLevel);
            PlayerController.Player.SetCurrentCheckpoint(otherDoor.gameObject);
            PlayerController.MoveToSpawnpoint(otherDoor.gameObject);
        }
        else
        {
            m_GameStarted = true;
        }
    }

    void PauseGame()
    {
        m_GamePaused = true;
        m_PauseCanvas = Instantiate(pauseGameObject, Vector3.zero, Quaternion.identity);
        CanvasGroup canvas = m_PauseCanvas.GetComponent<CanvasGroup>();
        canvas.alpha = 0.7f;
        Time.timeScale = 0f;
    }

    void RestartGame()
    {
        m_GamePaused = false;
        Time.timeScale = 1f;
        Destroy(m_PauseCanvas);
    }

    public void EnterDoor(string nextScene, string nextDoor)
    {
        m_NextDoorLevel = nextDoor;
        if (SceneManager.GetActiveScene().name != nextScene)
        {
            ChangeLevel(nextScene);
        }
        else
        {
            MoveInScene();
        }
    }

    void ChangeLevel(string nextScene)
    {
        DoorsManager.Instance.DisableSceneObjects(SceneManager.GetActiveScene().name);
        DoorsManager.Instance.EnableNextSceneObjects(nextScene);
        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        SceneManager.MoveGameObjectToScene(PlayerController.Player.gameObject, SceneManager.GetSceneByName(nextScene));
    }

    void MoveInScene()
    {
        DoorBehaviour otherDoor = DoorsManager.Instance.FindDoor(m_NextDoorLevel);
        PlayerController.Player.SetCurrentCheckpoint(otherDoor.gameObject);
        PlayerController.MoveToSpawnpoint(otherDoor.gameObject);
        mainCamera.m_Follow = PlayerController.Player.gameObject.transform;
    }

    public void TurnOnAlarm()
    {
        if (!m_AlarmPlaying && PlayerSkills.GetSkills().IsSkillUnlocked(PlayerSkills.SkillType.Jetpack))
        {
            m_Alarm = Instantiate(alarmObject, Vector3.zero, Quaternion.identity);
            m_AlarmPlaying = true;
        }
    }
}
