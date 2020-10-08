using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBehaviour : MonoBehaviour
{
    public GameObject messageBox;
    public GameObject activatedSwitch;
    public DoorBehaviour linkedDoor;
    public float messageTime;
    public string switchID;
    float m_ElapsedTime;
    [SerializeField] bool m_IsActivated;
    SwitchManager m_SwitchManager;

    void Awake()
    {
        m_SwitchManager = SwitchManager.Instance;

        if (m_SwitchManager.FindSwitch(switchID) == null)
        {
            m_SwitchManager.AddSwitch(gameObject.scene.name, this);
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
