using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Sprites;

public class GameController : MonoBehaviour {

    public CanvasGroup gameOverPanelCanvasGroup;
    public Text gameOverText;
    public Button restartButton;
    public Image pauseScreen;

	// Use this for initialization
	void Start () {
        StartGame();
        restartButton.onClick.AddListener(OnReloadClick);               
	}
	
	// Update is called once per frame
	void Update () {
		if(this.gameObject.GetComponent<Timer>().isOver)
        {
            StopGame();
        }
    }

    public void OnReloadClick()
    {
        Scene mainScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(mainScene.name);
    }

    public void StartGame()
    {
        Scene curScene = SceneManager.GetActiveScene();
        if (curScene.name != "main_scene")
        {
            SceneManager.LoadScene("main_scene");
            SceneManager.UnloadSceneAsync("start_menu");
        }
      
        //Unfreezes time
        Time.timeScale = 1;

        //Hides GameOverPanel
        ChangeGameOverPanelVisibility(false);
    }

    void StopGame()
    {
        //Stops Time
        Time.timeScale = 0;

        //Shows GameOver Panel
        ChangeGameOverPanelVisibility(true);

        if(OnGameOverEventHandler != null)
        {
            OnGameOverEventHandler(this);
        }
    }

    public void PauseGame()
    {
        //Stops Time
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
            gameObject.GetComponent<GameGrid>().canPlay = false;
            pauseScreen.enabled = true;
        }
        else
        {
            Time.timeScale = 1;
            gameObject.GetComponent<GameGrid>().canPlay = true;
            pauseScreen.enabled = false;
        }
    }

    public void HomeScreen()
    {
        //Stops Time
        SceneManager.LoadScene("start_menu");
        SceneManager.UnloadSceneAsync("main_scene");
    }

    void ChangeGameOverPanelVisibility(bool visible)
    {
        gameOverPanelCanvasGroup.blocksRaycasts = visible; //this prevents the UI element to receive input events
        if(visible)
            gameOverPanelCanvasGroup.alpha = 1f; //this makes everything transparent
        else
            gameOverPanelCanvasGroup.alpha = 0f; //this makes everything transparent
    }

    public void SetGameOverText(string text)
    {
        gameOverText.text = text;
    }

    public delegate void OnGameOver(GameController controller);
    public static event OnGameOver OnGameOverEventHandler;
}
