using UnityEngine;
using System.Collections;

public class RandomRangeTransform : MonoBehaviour {

	
	public float speed ;
	public Vector3 position;
	public float frameForUpdate;
	
	private float count;
	private Vector3 range;
	
	
	// Use this for initialization
	void Start () {
		count = Random.Range(0f,frameForUpdate);
		position = transform.position;
		range =new Vector3(6.0f,6.0f,6.0f);
	}
	
	// Update is called once per frame
	void Update () {
			
		if(count<frameForUpdate){
			transform.position = position + Vector3.Scale(SmoothRandom.GetVector3(speed), range);
			count=Random.Range(0f,frameForUpdate);
		}else{
			count++;
		}
		
		
	}
}
