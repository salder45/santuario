using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]
public class FPS : MonoBehaviour {
	public float gravedad = 20.0f;
	
	//
	private Vector3 moveDirection = Vector3.zero;
    private bool grounded = false;
	private CharacterController controller;
    private Transform myTransform;
	private float fallStartLevel;
	private bool falling;
	
	
	// Use this for initialization
	void Start () {
		controller = GetComponent<CharacterController>();
        myTransform = transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (grounded) {
		}else{
			if (!falling) {
                falling = true;
                fallStartLevel = myTransform.position.y;
            }
		}
		
		
		moveDirection.y -= gravedad * Time.deltaTime;
		
		
		 grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
	}
}
