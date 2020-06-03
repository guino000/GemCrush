using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour {

    public Text pointsText;

    public long points
    {
        get;
        set;
    }

	// Use this for initialization
	void Start () {
        this.points = 0;
	}
	
	// Update is called once per frame
	void Update () {
        pointsText.text = string.Format("{0}", this.points);
	}
}
