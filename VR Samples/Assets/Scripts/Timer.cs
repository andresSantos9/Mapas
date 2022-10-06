using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Timer : MonoBehaviour {

	string timerText;
	private float startTime;
	private bool stop = true;



		

	// Use this for initialization
	void Start () {
	

		StartTimer ();

	}
	
	// Update is called once per frame
	void Update () {

		if (stop)
			return; 

		float t = Time.time - startTime;

		string minutes = ((int)t / 60).ToString ();
		string seconds = (t % 60).ToString ("f2");

		timerText = minutes + ":" + seconds;
	
	}


	public void StartTimer (){

		startTime = Time.time;
		stop = false;
	}


	public string StopTimer(){

		//stop = true;
		return  timerText;
	}




}
