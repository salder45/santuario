using UnityEngine;
using UnityEditor;
using System.Collections;

public class ExplosionAnimator : SpritePackerAnimation {

	
	public Transform[] streamers;

	public float streamerUpAmount = 0f;
	public float streamerRadius = 0f;
	public int seed = 8734793;
	
	public float  minSpeed = 0f;
	public float  maxSpeed = 0f;
	public float  drag = 0f;
	public float  randomForce = 0f;
	public float  drop = 0f;
	public float  collisionDamping = 0f;

	public float  emitTimeOut = 0f;
	public float  shrinkTime = 0f;
	public float  timeRandom = 0f;

	public float  minSize = 0f;
	
	Vector3[] velocities;
	Vector3[] offsets;
	
	float lastSampledTime = 0;
	
	struct ExplosionStreamer {
		Transform trans;
		ParticleSystem[] emitters;
		Vector3 position;
		Vector3 velocity;
	}
	
	Vector3[] directions = {new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, -1, 0)};
	
	public override void Init () {
		var i = 0;
		velocities = new Vector3[streamers.Length];
		while(i < streamers.Length) {
			ParticleSystem[] PSes = streamers[i].GetComponentsInChildren<ParticleSystem>();
			foreach(ParticleSystem p in PSes) {
				SerializedObject so = new SerializedObject(p);
			}
			velocities[i] = Vector3.zero;
			i++;
		}
	}
	
	public override void Sample (float time, Vector3 cameraPos) {
		int origSeed = Random.seed;
		Random.seed = seed;
		
		var toCam = (cameraPos - transform.position).normalized * 1.35f;
		var i = 0;
		var dirIndex = 0;
		while(i < streamers.Length){
			Vector3 random = Random.onUnitSphere;
			Vector3 randomForceVec = Random.insideUnitSphere * randomForce;
			float initialSpeed = Random.Range(minSpeed, maxSpeed);
			
			if(dirIndex >= directions.Length) dirIndex = 0;
			Vector3 dir = directions[dirIndex];
			
			var randomv = ( (dir + random + toCam) + (transform.TransformDirection(Vector3.forward) * streamerUpAmount) ).normalized * streamerRadius;
			streamers[i].position = transform.position + randomv;
			
			if(velocities[i] == Vector3.zero) {
				velocities[i] = randomv.normalized * initialSpeed;
			}
			
			float deltaTime = lastSampledTime - time;
			
			velocities[i] -= Vector3.up * drop * deltaTime;
			velocities[i] += randomForceVec * deltaTime;
			velocities[i] -= velocities[i] * drag * deltaTime;
			
			/*
			for(e in emitters)
			{
				if(e.minSize > 0.02)
				{
					e.minSize = e.minSize * 1 - (timer / (shrinkTime));
					e.maxSize = e.maxSize * 1 - (timer / (shrinkTime));
				}
				
				e.rndVelocity *= 1 - (timer / (emitTimeOut * 2));
				
				if(timer > emitTimeOut || e.maxSize < minSize) e.emit = false;
			}
			
			var hit : RaycastHit;
			if(Physics.Raycast(transform.position, velocity, hit, velocity.magnitude))
			{
				velocity = Vector3.Reflect(velocity, hit.normal) * collisionDamping;
			}
			
			transform.position += velocity * Time.fixedDeltaTime;
			
			if(timer > emitTimeOut)
			{
				transform.DetachChildren();
				Destroy(gameObject);
			}
			*/
			
			
			dirIndex ++;
			i ++;
		}
		Random.seed = origSeed;
	}
}
