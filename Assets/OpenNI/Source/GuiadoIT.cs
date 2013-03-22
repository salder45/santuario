using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NITE;
using OpenNI;
using System;

[RequireComponent(typeof(AudioSource))]
public class GuiadoIT : MonoBehaviour {
	//CONSTANTES NOMBRES DE CAMINOS
	private const string RECORRIDO="recorrido";
	private string NOMBRE_CAMINO="1";
	
	private const string NOMBRE_CAMINO_UNO="1";
	private const string NOMBRE_CAMINO_DOS="2";
	private const string NOMBRE_CAMINO_TRES="3";
	private const string NOMBRE_CAMINO_CUATRO="4";
	private const string NOMBRE_CAMINO_CINCO="5";
	private const string NOMBRE_CAMINO_SEIS="6";
	private const string NOMBRE_CAMINO_SIETE="7";
	private Vector3 VECTOR_ACTUAL=Vector3.zero;
	GameObject player;
	//Posiciones a Mirar
	private Vector3 uno=Vector3.zero;
	private Vector3 dos=new Vector3(-25.07944f,1.86326f,-0.07589531f);
	private Vector3 tres=new Vector3(-10.47053f,0.7997839f,0.2008085f);
	private Vector3 cuatro=new Vector3(18.13391f,1.62014f,0.05215108f);
	private Vector3 cinco=new Vector3(10.02586f,1.4096f,-3.418796f);
	private Vector3 seis=new Vector3(10f,2.038217f,3.5f);
	private Vector3 siete=new Vector3(24.97611f,1.429089f,0.01680589f);
	
	//Audios
	public AudioClip clip1;
	public AudioClip clip2;
	public AudioClip clip3;
	public AudioClip clip4;
	public AudioClip clip5;
	public AudioClip clip6;
	public AudioClip clip7;
	//Varios
	private bool isMenu=false;
	private String playMemory;
	// Use this for initialization
	void Start () {
		player=GameObject.Find("Player");
		Debug.Log("Start");
		playMemory="1";
		playAudioNext(playMemory);
		//iTween.MoveTo(player,iTween.Hash("path",iTweenPath.GetPath(RECORRIDO),"time",60,"orientToPath",true));		
		
	}
	
	// Update is called once per frame
	void Update () {
		if(playMemory=="0"){
			//cambiarNivel
		}
		if(!isMenu){
			if(audio.isPlaying){
				recorreCamino(audio.time,audio.clip.length,NOMBRE_CAMINO,VECTOR_ACTUAL);
			}else{
				playAudioNext(playMemory);
			}
		}
		/*
		tiempoAc=audio.time;
		porcentaje=(1.0f*tiempoAc)/duracion;
		Debug.Log(tiempoAc);
		iTween.PutOnPath(player,iTweenPath.GetPath(RECORRIDO),porcentaje);
		*/
		
	}
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		//context.Release();
	}
	
	void recorreCamino(float tiempoActual,float tamanoTotal,string nombreCamino,Vector3 mirarA){
		float tmp=(1.0f*tiempoActual)/tamanoTotal;
		iTween.PutOnPath(player,iTweenPath.GetPath(nombreCamino),tmp);
		player.transform.LookAt(mirarA);
	}
	
	void playAudioNext(String memoria){
		Debug.Log ("----------"+memoria);		
		switch (memoria){
			case "1": audio.clip =clip1; playMemory="2"; audio.Play();
			VECTOR_ACTUAL=uno;
			NOMBRE_CAMINO=NOMBRE_CAMINO_UNO;
			break;
			case "2": audio.clip =clip2; playMemory="3";  audio.Play();
			VECTOR_ACTUAL=dos;
			NOMBRE_CAMINO=NOMBRE_CAMINO_DOS;
			break;
			case "3": audio.clip =clip3;   playMemory="4";audio.Play();
			VECTOR_ACTUAL=tres;
			NOMBRE_CAMINO=NOMBRE_CAMINO_TRES;
			break;
			case "4":audio.clip =clip4;   playMemory="5";audio.Play();
			VECTOR_ACTUAL=cuatro;
			NOMBRE_CAMINO=NOMBRE_CAMINO_CUATRO;
			break;
			case "5": audio.clip =clip5;   playMemory="6";audio.Play();
			VECTOR_ACTUAL=cinco;
			NOMBRE_CAMINO=NOMBRE_CAMINO_CINCO;
			break;
			case "6": audio.clip =clip6;   playMemory="7";audio.Play();
			VECTOR_ACTUAL=seis;
			NOMBRE_CAMINO=NOMBRE_CAMINO_SEIS;
			break;
			case "7": audio.clip =clip7;   playMemory="0";audio.Play();
			VECTOR_ACTUAL=siete;
			NOMBRE_CAMINO=NOMBRE_CAMINO_SIETE;
			break;
			
		}
		
	}
}
