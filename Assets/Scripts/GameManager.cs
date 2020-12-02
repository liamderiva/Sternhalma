using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { set; get; }

	void Start ()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
	}

    public void PvPButton()
    {
        Debug.Log("PvP");
        SceneManager.LoadScene("PlayerVsPlayer");
    }
    public void PvEButton()
    {
        Debug.Log("PvE");
        SceneManager.LoadScene("PlayerVsComputer");
    }
    public void ReturnButton()
    {
        Debug.Log("Return");
        SceneManager.LoadScene("MainMenu");
    }
    public void ExitButton()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
