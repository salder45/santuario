#pragma strict
#pragma implicit
#pragma downcast

var time = 0.00;
private var done = false;

function Update ()
{
	if(!done)
	{
		done = true;
		if(time != 0) Wait();
		else particleEmitter.emit = true;
	}
}

function Wait()
{
	yield WaitForSeconds (time);
	particleEmitter.emit = true;
}