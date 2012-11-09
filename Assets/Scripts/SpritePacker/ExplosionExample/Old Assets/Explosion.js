//#pragma strict
var deleteAfter = 0.00;
var streamer : GameObject;var streamerUpAmount = 0.00;var streamerCountMin = 0;var streamerCountMax = 0;var streamerRadius = 0.00;

private var timer = 0.00;

private var directions = Array(Vector3(0, 0, 1), Vector3(0, 0, -1), Vector3(1, 0, 0), Vector3(-1, 0, 0), Vector3(0, 1, 0), Vector3(0, -1, 0));
private var hit : RaycastHit;
function Start (){	if(streamer)	{		var streamerCount = Random.Range(streamerCountMin, streamerCountMax);
		var toCam = (Camera.main.transform.position - transform.position).normalized * 1.35;		var i = 0;
		var dirIndex = 0;		while(i < streamerCount)		{
			var random = Random.onUnitSphere;
			
			if(dirIndex >= directions.length) dirIndex = 0;
			var dir : Vector3 = directions[dirIndex];
						var randomv = ( (dir + random + toCam) + (transform.TransformDirection(Vector3.forward) * streamerUpAmount) ).normalized * streamerRadius;			Instantiate(streamer, transform.position + randomv, Quaternion.LookRotation(randomv));
			dirIndex ++;			i ++;		}	}
}

function Update (){		if(timer > deleteAfter)	{		transform.DetachChildren();		Destroy(gameObject);	}		timer += Time.deltaTime;}