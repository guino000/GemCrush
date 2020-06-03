using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class StartMenuController : MonoBehaviour {

    public Button btLogin;

    public void StartGame()
    {
        Scene curScene = SceneManager.GetActiveScene();

        if(curScene.name != "main_scene")
        {
            SceneManager.LoadScene("main_scene");
            SceneManager.UnloadSceneAsync("start_menu");
        }
    }

    public void UserLogin()
    {       
        GPS playServices = gameObject.GetComponent<GPS>();
        playServices.LogIn();
        if (playServices.CheckIfLoggedOn())
            btLogin.gameObject.SetActive(false);
        else
            btLogin.gameObject.SetActive(false);
    }

    public void ViewLeaderboards()
    {
        gameObject.GetComponent<GPS>().leaderboard = "CgkI24vU4a8IEAIQAA";
        gameObject.GetComponent<GPS>().OnShowLeaderBoard();
    }
}
