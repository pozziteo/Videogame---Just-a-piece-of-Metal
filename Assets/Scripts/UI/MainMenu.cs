using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public float introTextFadeInTimer;
    public float introTextShowTimer;
    float m_Timer;
    
    void Start()
    {
        StartCoroutine(Presentation());
    }
    public void PlayGame()
    {
        StartCoroutine(StartGame());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator Presentation()
    {
        yield return new WaitForSeconds(4f);
        transform.Find("Presents").gameObject.SetActive(false);
        transform.Find("Title").gameObject.SetActive(true);
        transform.Find("MainMenu").gameObject.SetActive(true);
    }

    IEnumerator StartGame()
    {
        transform.Find("Title").gameObject.SetActive(false);
        transform.Find("MainMenu").gameObject.SetActive(false);
        GameObject intro = transform.Find("Intro").gameObject;
        intro.SetActive(true);
        CanvasGroup canvas = intro.GetComponent<CanvasGroup>();

        while (m_Timer < introTextFadeInTimer)
        {
            m_Timer += Time.deltaTime;
            canvas.alpha = m_Timer / introTextFadeInTimer;
            yield return null;
        }

        yield return new WaitForSeconds(introTextShowTimer);

        while (m_Timer > 0)
        {
            m_Timer -= Time.deltaTime;
            canvas.alpha = m_Timer / introTextFadeInTimer;
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        yield return null;
    }
}
