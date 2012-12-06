using UnityEngine;
using System.Collections;

public class ClickMenu : MonoBehaviour {
	
	//Variables internas
	private float frameDeEspera = 100;
	private float exitCount = 100;
	
	//Variables publicas
	public string siguienteNivel;
	public GameObject fade;	
	
	MenuController go;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		
	}
	
	void OnCollisionEnter(Collision other){
		Debug.Log("Collision");
		go = (MenuController)other.gameObject.GetComponent("MenuController");
		go.context.Release();
		Application.LoadLevel(siguienteNivel);
		
	}
}
