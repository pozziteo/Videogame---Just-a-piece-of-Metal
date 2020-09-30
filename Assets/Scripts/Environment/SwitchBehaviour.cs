using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBehaviour : MonoBehaviour
{
    static List<string> SwitchIDs;
    public GameObject messageBox;
    public GameObject activatedSwitch;
    public DoorBehaviour linkedDoor;
    public float messageTime;
    public string switchID;
    float m_ElapsedTime;
    [SerializeField] bool m_IsActivated;

    void Awake()
    {
        if (SwitchIDs == null)
        {
            SwitchIDs = new List<string>();
        }

        if (!SwitchIDs.Contains(switchID))
        {
            SwitchIDs.Add(switchID);
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        messageBox.SetActive(false);
        activatedSwitch.SetActive(false);
    }

    public bool IsActivated()
    {
        return m_IsActivated;
    }

    // Update is called once per frame
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

    public void DisplayMessage()
    {
        m_ElapsedTime = messageTime;
        messageBox.SetActive(true);
    }

    public void ActivateSwitch() 
    {
        activatedSwitch.SetActive(true);
        messageBox.SetActive(false);
        m_IsActivated = true;
        linkedDoor.UnlockDoor();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        if (player != null && !m_IsActivated)
        {
            DisplayMessage();
        }
    }
}
