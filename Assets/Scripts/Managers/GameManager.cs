using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using TMPro;

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
    public GameObject finalText;
    public GameObject gameStatisticsCanvas;
    public float timeShowMessage;
    CinemachineVirtualCamera mainCamera;
    CanvasGroup m_ActiveFinalCanvas;
    GameObject m_PauseCanvas;
    GameObject m_Alarm;
    bool m_GamePaused;
    bool m_GameStarted;
    bool m_AlarmPlaying;
    bool m_EndGame;
    bool m_ShowFinalText;
    bool m_WaitForInput;
    float m_TotalPlayingTime;
    float m_FinalTextTimer;
    float m_ReverseTimer;
    int m_PlayerDeaths;
    int m_KilledEnemies;
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
        if (!m_EndGame)
        {
                m_TotalPlayingTime += Time.deltaTime;

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
        else
        {
            if (m_ShowFinalText)
            {
                ShowFinalext();
            }
            else
            {
                if (!m_WaitForInput)
                {
                    ShowStatistics();
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        ReturnToMenu();
                    }
                }
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
        if (nextScene != "EndScene")
        {
            DoorsManager.Instance.DisableSceneObjects(SceneManager.GetActiveScene().name);
            DoorsManager.Instance.EnableNextSceneObjects(nextScene);
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
            SceneManager.MoveGameObjectToScene(PlayerController.Player.gameObject, SceneManager.GetSceneByName(nextScene));
        }
        else
        {
            m_EndGame = true;
            m_AlarmPlaying = false;
            Destroy(m_Alarm);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += EndGame;
            SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
        }
    }

    void MoveInScene()
    {
        StartCoroutine(CameraMove());
        DoorBehaviour otherDoor = DoorsManager.Instance.FindDoor(m_NextDoorLevel);
        PlayerController.Player.SetCurrentCheckpoint(otherDoor.gameObject);
        PlayerController.MoveToSpawnpoint(otherDoor.gameObject);
    }

    public IEnumerator CameraMove()
    {
        mainCamera.m_Follow = null;
        yield return new WaitForEndOfFrame();
        mainCamera.m_Follow = GameObject.Find("Player").transform;
    }

    public void TurnOnAlarm()
    {
        if (!m_AlarmPlaying && PlayerSkills.GetSkills().IsSkillUnlocked(PlayerSkills.SkillType.Jetpack))
        {
            m_Alarm = Instantiate(alarmObject, Vector3.zero, Quaternion.identity);
            m_AlarmPlaying = true;
        }
    }

    public void AddPlayerDeath()
    {
        m_PlayerDeaths += 1;
    }

    public void AddEnemyKill()
    {
        m_KilledEnemies += 1;
    }

    void EndGame(Scene scene, LoadSceneMode mode)
    {
        GameObject finalTextCanvas = Instantiate(finalText, Vector3.zero, Quaternion.identity);
        m_ActiveFinalCanvas = finalTextCanvas.GetComponent<CanvasGroup>();
        m_ActiveFinalCanvas.alpha = 0f;

        StartCoroutine(DestroyAllObjects());

        m_ShowFinalText = true;

        SceneManager.sceneLoaded -= EndGame;
    }

    void ShowFinalext()
    {
        if (m_FinalTextTimer < 5f)
        {
            m_FinalTextTimer += Time.unscaledDeltaTime;
            m_ActiveFinalCanvas.alpha = m_FinalTextTimer / 5f;
            m_ReverseTimer = m_FinalTextTimer;
        }
        else if (m_FinalTextTimer < 5f + timeShowMessage)
        {
            m_FinalTextTimer += Time.unscaledDeltaTime;
            m_ActiveFinalCanvas.alpha = 0.8f;
        }
        else
        {
            m_ReverseTimer -= 2f * Time.unscaledDeltaTime;
            m_ActiveFinalCanvas.alpha = m_ReverseTimer / 5f;
            if (m_ReverseTimer < 0)
            {
                Destroy(m_ActiveFinalCanvas.gameObject);
                m_ActiveFinalCanvas = null;
                m_ShowFinalText = false;
            }
        }
    }

    void ShowStatistics()
    {
        if (m_ActiveFinalCanvas == null)
        {
            GameObject statisticsCanvas = Instantiate(gameStatisticsCanvas, Vector3.zero, Quaternion.identity);
            m_ActiveFinalCanvas = statisticsCanvas.GetComponent<CanvasGroup>();
            m_ActiveFinalCanvas.alpha = 0f;
            m_FinalTextTimer = 0f;

            TMP_Text timeText = statisticsCanvas.transform.Find("CompletionTime").gameObject.GetComponent<TMP_Text>();

            float time = m_TotalPlayingTime;
            int hours = Mathf.FloorToInt(time / 3600);
            time = time - 3600*hours;
            int minutes = Mathf.FloorToInt(time / 60);
            time = time - 60*minutes;
            int seconds = Mathf.FloorToInt(time % 60);

            timeText.text += string.Format ("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

            TMP_Text deathsText = statisticsCanvas.transform.Find("Deaths").gameObject.GetComponent<TMP_Text>();
            deathsText.text += m_PlayerDeaths;

            TMP_Text killsText = statisticsCanvas.transform.Find("Kills").gameObject.GetComponent<TMP_Text>();
            killsText.text += m_KilledEnemies;
        }

        if (m_FinalTextTimer < 5f)
        {
            m_FinalTextTimer += Time.unscaledDeltaTime;
            m_ActiveFinalCanvas.alpha = m_FinalTextTimer / 5f;
        }
        else
        {
            m_WaitForInput = true;
        }
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Destroy(BackgroundMusicPlayer.MusicPlayer.gameObject);
        Destroy(gameObject);
    }

    public void ReturnToMenu(PauseMenu fromPause)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(DestroyAllObjects());
        ReturnToMenu();
    }

    IEnumerator DestroyAllObjects()
    {
        LevelManager.Instance.DestroyAllPersistentObjects();
        PlayerController.Player.DestroyPlayer();
        EnemySpawnerManager.Instance.DestroyManager();
        yield return null;
    }
}
