using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour {
	
	//Contadores para el fade
	private float fadeCount=0;
	private float exitCount=0;
	
	
	//Objetos para ocultar y Audio Source
	public GameObject fade;
	public AudioSource introduccionAudio;
	public string nextLevel;
	
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		
		if(fadeCount<1000){
			fadeCount++;
			Color color = fade.renderer.material.color;
			color.a = 1f;
			color.a -= fadeCount/1000;
			fade.renderer.material.color = color;
		}else if(!introduccionAudio.isPlaying){
				if(exitCount<100){
				exitCount++;
				Color color = fade.renderer.material.color;
				color.a = 0f;
				color.a += exitCount/100;
				fade.renderer.material.color = color;
			}else{
				Application.LoadLevel(nextLevel);
			}
		}
		
		if(fadeCount==50)
			introduccionAudio.Play();

	}

	
}