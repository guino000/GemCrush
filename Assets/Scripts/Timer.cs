using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    public Text timerText;
    private float startTime;

    public bool isOver
    {
        get;
        private set;
    }

    public float gameDuration
    {
        get;
        set;
    }

	// Use this for initialization
	void Start () {
        startTime = Time.time;
        gameDuration = 30f;
	}
	
	// Update is called once per frame
	void Update () {
        float t = (startTime + gameDuration) - Time.time;
        string minutes = ((int) (t / 60)).ToString();
        string seconds = ((int) (t % 60)).ToString("f0");

        timerText.text = string.Format("{0}:{1}", minutes, seconds);

        if(Time.time >= (startTime + gameDuration))
        {
            this.isOver = true;
        }
	}
}
