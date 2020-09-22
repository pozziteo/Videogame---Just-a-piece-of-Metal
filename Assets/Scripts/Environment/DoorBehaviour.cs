using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    public GameObject lockedDoor;
    public GameObject unlockedDoor;
    public GameObject openDoor;
    public GameObject messageBox;
    public float messageTime;
    float m_ElapsedTime;
    bool m_IsUnlocked;
    bool m_IsOpen;
    
    void Awake()
    {
        unlockedDoor.SetActive(false);
        openDoor.SetActive(false);
    }

    void Update()
    {
        if (m_ElapsedTime >= 0)
        {
            m_ElapsedTime -= Time.deltaTime;
            if (m_ElapsedTime < 0)
            {
                messageBox.SetActive(false);
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
        if (m_IsOpen)
        {
            return;
        }

        m_IsOpen = true;
        openDoor.SetActive(true);
        unlockedDoor.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (m_IsUnlocked)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();

            if (player != null && !m_IsOpen)
            {
                DisplayMessage();
            }
        }
    }

    void DisplayMessage()
    {
        m_ElapsedTime = messageTime;
        messageBox.SetActive(true);
    }
}
