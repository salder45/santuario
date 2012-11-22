using UnityEngine;
using System.Collections;

public class Jugar : MonoBehaviour {

	// Use this for initialization
	private float xs;

	public float range;

	void Start () {
		xs = transform.position.x;
		Debug.Log("EL valor de xs = "+xs);
	}

	void OnTriggerEnter(){
		Debug.Log("trigger On");
		//transform.Translate(new Vector3( xs+range,transform.position.y,transform.position.z));	

	}

	void OnTriggerExit(){
		Debug.Log("trigger Off");
		//transform.Translate(new Vector3( xs-range,transform.position.y,transform.position.z));

	}

	// Update is called once per frame
	void Update () {

	}
}