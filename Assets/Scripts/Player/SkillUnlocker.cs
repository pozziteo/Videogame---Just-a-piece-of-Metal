using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using TMPro;

public class SkillUnlocker : MonoBehaviour
{
    public PlayerSkills.SkillType skillUnlock;
    public ParticleSystem particle;
    public GameObject unlockSkillMessage;
    public CanvasGroup faderCanvas;
    public float timeFadeScreen;
    public float timeShowMessage = 3f;
    SkillManager m_SkillManager;
    bool m_InRange;
    bool m_Unlocked;
    bool m_FadeScreen;
    float m_FadingTimer;
    float m_ReverseTimer;

    void Awake()
    {
        m_SkillManager = SkillManager.Instance;

        if (m_SkillManager.FindSkill(skillUnlock) == null)
        {
            m_SkillManager.AddSkillUnlocker(gameObject.scene.name, this);
            DontDestroyOnLoad(this);
            unlockSkillMessage.SetActive(false);
            faderCanvas.alpha = 0f;
            GameObject fadeImage = faderCanvas.transform.Find("Black").gameObject;
            TMP_Text nameText = fadeImage.transform.Find("SkillName").gameObject.GetComponent<TMP_Text>();
            nameText.text = "You unlocked\n" + PlayerSkills.GetSkillDescription(skillUnlock).ToUpper() + "\n\n\n\n";
            TMP_Text descrText = fadeImage.transform.Find("SkillDescription").gameObject.GetComponent<TMP_Text>();
            descrText.text = PlayerSkills.GetSkillExplanation(skillUnlock);
        }
        else
        {
            Destroy(gameObject);
        }                     
    }

    void Update()
    {
        if (m_Unlocked)
        {
            if (m_FadeScreen)
            {
                m_InRange = false;
                Time.timeScale = 0f;
                ShowUnlockedMessage();
            }
        }

        else if (m_InRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayerSkills.GetSkills().UnlockSkill(skillUnlock);
                Destroy(particle);
                Destroy(unlockSkillMessage);
                m_Unlocked = true;
                m_FadeScreen = true;
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!m_Unlocked)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                unlockSkillMessage.SetActive(true);
                m_InRange = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!m_Unlocked)
            {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                m_InRange = false;
                unlockSkillMessage.SetActive(false);
            }
        }
    }

    void ShowUnlockedMessage()
    {
        if (m_FadingTimer < timeFadeScreen)
        {
            m_FadingTimer += Time.unscaledDeltaTime;
            faderCanvas.alpha = m_FadingTimer / timeFadeScreen * 0.8f;
            m_ReverseTimer = m_FadingTimer;
        }
        else if (m_FadingTimer < timeFadeScreen + timeShowMessage)
        {
            m_FadingTimer += Time.unscaledDeltaTime;
            faderCanvas.alpha = 0.8f;
        }
        else
        {
            m_ReverseTimer -= 2f * Time.unscaledDeltaTime;
            faderCanvas.alpha = m_ReverseTimer / timeFadeScreen * 0.8f;
            if (m_ReverseTimer < 0)
            {
                Time.timeScale = 1f;
                m_FadeScreen = false;
            }
        }
    }
}
