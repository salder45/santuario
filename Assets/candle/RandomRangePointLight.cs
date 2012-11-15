using UnityEngine;
using System.Collections;

public class RandomRangePointLight : MonoBehaviour {
	
	public float min;
	public float max;
	public float frameForUpdate;
	
	private float count;
	
	
	// Use this for initialization
	void Start () {
		count = Random.Range(0f,frameForUpdate);
	}
	
	// Update is called once per frame
	void Update () {
		
		if(count<frameForUpdate){
			gameObject.light.range = Random.Range(min,max);	
			count=Random.Range(0f,frameForUpdate);
		}else{
			count++;
		}
		
		
	}
}
