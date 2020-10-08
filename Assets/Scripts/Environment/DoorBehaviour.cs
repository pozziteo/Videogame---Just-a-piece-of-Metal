using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorBehaviour : MonoBehaviour
{
    public GameObject lockedDoor;
    public GameObject unlockedDoor;
    public GameObject openDoor;
    public GameObject unlockedDoorMessage;
    public GameObject openDoorMessage;
    public string connectedScene;
    public float messageTime;
    public string doorID;
    public string connectedDoor;
    DoorsManager m_DoorsManager;
    float m_ElapsedTime;
    bool m_IsUnlocked;
    bool m_IsOpen;

    public bool IsUnlocked()
    {
        return m_IsUnlocked;
    }

    public bool IsOpen()
    {
        return m_IsOpen;
    }
    
    void Awake()
    {
        m_DoorsManager = DoorsManager.Instance;

        if (m_DoorsManager.FindDoor(doorID) == null)
        {
            m_DoorsManager.AddDoor(gameObject.scene.name, this);
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

        if (unlockedDoor.gameObject.activeSelf)
        {
            m_IsUnlocked = true;
        }
        else if (openDoor.gameObject.activeSelf)
        {
            m_IsUnlocked = true;
            m_IsOpen = true;
        }
    }

    void Update()
    {
        if (m_ElapsedTime >= 0)
        {
            m_ElapsedTime -= Time.deltaTime;
            if (m_ElapsedTime < 0)
            {
                unlockedDoorMessage.SetActive(false);
                openDoorMessage.SetActive(false);
            }
        }
    }

    public void UnlockDoor()
    {
        m_IsUnlocked = true;
        unlockedDoor.SetActive(true);
        Destroy(lockedDoor);
    }

    public void OpenDoor()
    {
        if (!m_IsUnlocked || m_IsOpen)
        {
            return;
        }

        m_IsOpen = true;
        openDoor.SetActive(true);
        unlockedDoor.SetActive(false);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (m_IsUnlocked)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                DisplayMessage();
            }
        }
    }

    void DisplayMessage()
    {
        m_ElapsedTime = messageTime;

        if (!m_IsOpen)
        {
            unlockedDoorMessage.SetActive(true);
        }
        else
        {
            openDoorMessage.SetActive(true);
        }
    }

    public void ChangeLevel()
    {
        if (SceneManager.GetActiveScene().name != connectedScene)
        {
            m_DoorsManager.DisableSceneObjects(SceneManager.GetActiveScene().name);
            m_DoorsManager.EnableNextSceneObjects(connectedScene);
            SceneManager.LoadScene(connectedScene, LoadSceneMode.Single);
            SceneManager.MoveGameObjectToScene(PlayerController.Player.gameObject, SceneManager.GetSceneByName(connectedScene));
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            DoorBehaviour otherDoor = m_DoorsManager.FindDoor(connectedDoor);
            PlayerController.Player.SetCurrentCheckpoint(otherDoor.gameObject);
            PlayerController.MoveToSpawnpoint(otherDoor.gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DoorBehaviour otherDoor = m_DoorsManager.FindDoor(connectedDoor);
        PlayerController.Player.SetCurrentCheckpoint(otherDoor.gameObject);
        PlayerController.MoveToSpawnpoint(otherDoor.gameObject);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
