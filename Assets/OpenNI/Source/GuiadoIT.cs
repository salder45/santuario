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
	private const string NOMBRE_BOTON_PLAY="botonPlay";
	private const string NOMBRE_BOTON_REBOBINAR="botonRegresar";
	private const string NOMBRE_BOTON_SALIR="botonSalir";
	GameObject player;
	//Posiciones a Mirar
	private Vector3 uno=Vector3.zero;
	private Vector3 dos=new Vector3(-25.07944f,1.86326f,-0.07589531f);
	private Vector3 tres=new Vector3(-10.47053f,0.7997839f,0.2008085f);
	private Vector3 cuatro=new Vector3(18.13391f,.5f,0.05215108f);
	private Vector3 cinco=new Vector3(10.02586f,.3f,-3.418796f);
	private Vector3 seis=new Vector3(10f,.5f,3.5f);
	private Vector3 siete=new Vector3(24.97611f,.5f,0.01680589f);
	
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
	//OPENNI
		private readonly string XML_CONFIG=@".//OpenNI.xml";
	public Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private HandsGenerator hands;
	private GestureGenerator gesture;
	//constantes mano
	private const string WAVE="Wave";
	private const string CLICK="Click";
	private const string CAMARA_PLAYER="CameraPlayer";
	private const string CAMARA_MENU="CameraMenu";
	private const float UNITY_MEDIDA_X_INIT=0.7f;
	private const float UNITY_MEDIDA_X_END=1f;
	private const float UNITY_MEDIDA_Y=0.6f;
	private const float UNITY_MEDIDA_Z=1.1f;
	private const float MEDIDA_MANO_X=400f;
	private const float MEDIDA_MANO_Y=400f;
	private const float MEDIDA_MANO_Z=300f;
	private const string EJE_X="X";
	private const string EJE_Y="Y";
	private const string EJE_Z="Z";
	private float zInicial=0f;
	Camera camaraMenu;
	Camera camaraPlayer;
	private const string NOMBRE_ANTERIOR="menu";
	
	//puntos inicial y final
	private float xInit=0f;
	private float yInit=0f;
	private float zInit=0f;
	private float xEnd=0f;
	private float yEnd=0f;
	private float zEnd=0f;
	//distancias entre los puntos
	private float distanciaX=0f;
	private float distanciaY=0f;
	private float distanciaZ=0f;
	//puntos medios
	private Point3D puntoMitad;
	private float xMedia=0f;
	private float yMedia=0f;
	private float zMedia=0f;
	//puntos mano
	private Point3D puntoMano;
	private float kinectManoX=0f;
	private float kinectManoY=0f;
	private float kinectManoZ=0f;
	//punto de referencia para el kinect
	private float kinectRefX=0f;
	private float kinectRefY=0f;
	private float kinectRefZ=0f;
	//distancias reales kinect
	private float kinectDistanciaX=0f;
	private float kinectDistanciaY=0f;
	private float kinectDistanciaZ=0f;
	// Use this for initialization
	private float contador=0f;
	//Cnotador del click
	private float clickCount = 0;
	private float exitCount=100;
	private string siguienteNivel;

	
	void Start () {
		player=GameObject.Find("Player");
		camaraMenu=GameObject.Find(CAMARA_MENU).camera;
		camaraPlayer=GameObject.Find(CAMARA_PLAYER).camera;	
		seleccionarCamara(CAMARA_PLAYER);
		Debug.Log("Start");
		playMemory="1";
		playAudioNext(playMemory);
		//iTween.MoveTo(player,iTween.Hash("path",iTweenPath.GetPath(RECORRIDO),"time",60,"orientToPath",true));
		determinaEspacioUnity();
		determinaCentro();
		transform.position=new Vector3(xMedia,yMedia,zMedia);
		//
		this.context=Context.CreateFromXmlFile(XML_CONFIG, out scriptNode);
		this.depth=this.context.FindExistingNode(NodeType.Depth) as DepthGenerator;
		if(depth==null){
			throw new Exception("Nodo de Profundidad no encontrado");
		}
		this.hands=this.context.FindExistingNode(NodeType.Hands) as HandsGenerator;
		if(this.hands==null){
			throw new Exception("Nodo de Manos no encontrado");
		}
		this.gesture=this.context.FindExistingNode(NodeType.Gesture) as GestureGenerator;
		if(this.gesture==null){
			throw new Exception("Nodo de Gestos no encontrado");
		}
		fadeCountCircle();
		//handdlers
		this.hands.HandCreate+=hands_HandCreate;
		this.hands.HandUpdate+=hands_HandUpdate;
		this.hands.HandDestroy+=hands_HandDestroy;

		this.gesture.AddGesture(WAVE);
		this.gesture.AddGesture(CLICK);
		this.gesture.GestureRecognized+=gesture_GestureRecognized;
		this.gesture.StartGenerating();
	}
	
	// Update is called once per frame
	void Update () {
		this.context.WaitOneUpdateAll(this.depth);
		if(playMemory=="0"){
			Application.LoadLevel("Menu");
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
		context.Release();
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
			case "7": audio.clip =clip7;   playMemory="8";audio.Play();
			VECTOR_ACTUAL=siete;
			NOMBRE_CAMINO=NOMBRE_CAMINO_SIETE;
			break;
			case "8": audio.clip =clip7;   playMemory="0";audio.Play();
			break;
			
		}
		
	}
	//Algoritmos interaccion mano
	
	void seleccionarCamara(string nombre){
		//checar el cambiar de camaras el error es que las do quedan desactivadas el detalle es que solo regresa las camaras enabled en ese momento
		if(nombre==CAMARA_PLAYER){
			camaraPlayer.enabled=true;
			camaraMenu.enabled=false;
			isMenu=false;
		}else {
			camaraPlayer.enabled=false;
			camaraMenu.enabled=true;
		}
	}
	
	void determinaEspacioUnity(){
		xInit=camaraMenu.transform.position.x+UNITY_MEDIDA_X_INIT;
		yInit=camaraMenu.transform.position.y-UNITY_MEDIDA_Y;
		zInit=camaraMenu.transform.position.z+UNITY_MEDIDA_Z;
		xEnd=camaraMenu.transform.position.x+UNITY_MEDIDA_X_END;
		yEnd=camaraMenu.transform.position.y+UNITY_MEDIDA_Y;
		zEnd=camaraMenu.transform.position.z-UNITY_MEDIDA_Z;
		//puntoInicial.transform.position=new Vector3(xInit,yInit,zInit);
		//puntoFinal.transform.position=new Vector3(xEnd,yEnd,zEnd);
	}
	
	void determinaCentro(){
		calculaDistanciaEje();
		calculaPuntoMedio();
	}
	
	void calculaDistanciaEje(){
		this.distanciaX=distanciaEntreDosPuntos(new Point3D(xInit,0f,0f),new Point3D(xEnd,0f,0f));
		this.distanciaY=distanciaEntreDosPuntos(new Point3D(0f,yInit,0f),new Point3D(0f,yEnd,0f));
		this.distanciaZ=distanciaEntreDosPuntos(new Point3D(0f,0f,zInit),new Point3D(0f,0f,zEnd));
	}
	
	void calculaPuntoMedio(){
		//checar signos se cambio el signo de z que en este caso viene a ser el eje x para la camara
		this.xMedia=xInit+(distanciaX/2);
		this.yMedia=yInit+(distanciaY/2);
		this.zMedia=zInit-(distanciaZ/2);
	}
	
	float distanciaEntreDosPuntos(Point3D a,Point3D b){
		return Mathf.Sqrt(elevaCuadrado(a.X-b.X)+elevaCuadrado(a.Y-b.Y)+elevaCuadrado(a.Z-b.Z));
	}

	float elevaCuadrado(float numero){
		return Mathf.Pow(numero,2f);
	}
	
	void hands_HandCreate(object sender, HandCreateEventArgs e){
		Debug.Log("Create Hand");
		this.puntoMano=e.Position;
		setPuntosManoActuales(this.puntoMano);
		iniciaPuntoReferencia(this.puntoMano);
	}

	void hands_HandUpdate(object sender, HandUpdateEventArgs e){
		setPuntosManoActuales(e.Position);
		distanciaKinectReales();
		if(!isActiveMainCamera()){
			calculaMovimiento();
		}else{
			transform.position=new Vector3(xMedia,yMedia,zMedia);
		}
	}

	void hands_HandDestroy(object sender, HandDestroyEventArgs e){
		transform.position=new Vector3(xMedia,yMedia,zMedia);
	}
	
	void gesture_GestureRecognized(object sender, GestureRecognizedEventArgs e){
		if(e.Gesture==WAVE){
			this.hands.StartTracking(e.EndPosition);
		}else if(e.Gesture==CLICK){
			Debug.Log("Pausa");
			isMenu=true;
			audio.Pause();
			seleccionarCamara(CAMARA_MENU);
		}
	}
	
	void setPuntosManoActuales(Point3D a){
		this.kinectManoX=a.X;
		this.kinectManoY=a.Y;
		this.kinectManoZ=a.Z;
	}
	
	void iniciaPuntoReferencia(Point3D puntoCentral){
		this.kinectRefX=puntoCentral.X+(MEDIDA_MANO_X/2f);
		this.kinectRefY=puntoCentral.Y-(MEDIDA_MANO_Y/2f);
		this.kinectRefZ=puntoCentral.Z+(MEDIDA_MANO_Z/2f);
	}
	
	void distanciaKinectReales(){
		this.kinectDistanciaX=distanciaEntreDosPuntos(new Point3D(kinectRefX,0f,0f),new Point3D(kinectManoX,0f,0f));
		this.kinectDistanciaY=distanciaEntreDosPuntos(new Point3D(0f,kinectRefY,0f),new Point3D(0f,kinectManoY,0f));
		this.kinectDistanciaZ=distanciaEntreDosPuntos(new Point3D(0f,0f,kinectRefZ),new Point3D(0f,0f,kinectManoZ));
	}
	bool isActiveMainCamera(){
		return camaraPlayer.enabled;
	}
	void calculaMovimiento(){
		float x=transform.position.x;
		float y=transform.position.y;
		//float z=0f;
		float z=transform.position.z;
		//los ejes estan cambiados
		if(kinectRefX>kinectManoX){
			float zNormal=((kinectDistanciaX/10f)*100f)/(MEDIDA_MANO_X/10f);		
			z=(zNormal*distanciaZ)/100;
			z=zInit-z;
			/*
			float xNormal=((kinectDistanciaX/10f)*100f)/(MEDIDA_MANO_X/10f);
			x=(xNormal*distanciaX)/100;
			x=(xInit+x)-4f;
			*/
		}
		
		if(kinectRefY<kinectManoY){
			float yNormal=((kinectDistanciaY/10f)*100f)/(MEDIDA_MANO_Y/10f);
			y=(yNormal*distanciaY)/100;
			y=yInit+y;
		}
		/*
		if(kinectRefZ>kinectManoZ){
			float zNormal=((kinectDistanciaZ/10f)*100f)/(MEDIDA_MANO_Z/10f);		
			z=(zNormal*distanciaZ)/100;
		}
		*/
		
		transform.position=new Vector3(x,y,z);
		//codigo a ver donde se acomoda
		if(kinectManoZ<(zInicial-250f)){
			Debug.Log("Click");
			//clickAudio.Play();
			gameObject.transform.Translate(0f,0f,6f);
			
		}
		
	}
	
	void fadeCountCircle(){
		int number=1;
		while(number<=8){
			string nameC = "c"+number;
			GameObject goC = GameObject.Find(nameC);
			Color color = goC.renderer.material.color;
			color.a = 0f;
			goC.renderer.material.color = color;
			number++;
		}
	}
	
	void OnCollisionExit(Collision collision){
		contador=0f;
		fadeCountCircle();
		clickCount=0;
	}
	
	void OnCollisionStay(Collision collision){
		if(clickCount<100){			
			if(clickCount>10){
				GameObject goC = GameObject.Find("c1");
				Color color = goC.renderer.material.color;
				color.a = 1f;
				goC.renderer.material.color = color;
			}
			if(clickCount>20){
				GameObject goC = GameObject.Find("c2");
				Color color = goC.renderer.material.color;
				color.a = 1f;
				goC.renderer.material.color = color;
			}
			if(clickCount>30){
				GameObject goC = GameObject.Find("c3");
				Color color = goC.renderer.material.color;
				color.a = 1f;
				goC.renderer.material.color = color;
			}
			if(clickCount>40){
				GameObject goC = GameObject.Find("c4");
				Color color = goC.renderer.material.color;
				color.a = 1f;
				goC.renderer.material.color = color;
			}
			if(clickCount>50){
				GameObject goC = GameObject.Find("c5");
				Color color = goC.renderer.material.color;
				color.a = 1f;
				goC.renderer.material.color = color;
			}
			if(clickCount>60){
				GameObject goC = GameObject.Find("c6");
				Color color = goC.renderer.material.color;
				color.a = 1f;
				goC.renderer.material.color = color;
			}
			if(clickCount>80){
				GameObject goC = GameObject.Find("c7");
				Color color = goC.renderer.material.color;
				color.a = 1f;
				goC.renderer.material.color = color;
			}
			if(clickCount>95){
				GameObject goC = GameObject.Find("c8");
				Color color = goC.renderer.material.color;
				color.a = 1f;
				goC.renderer.material.color = color;
			}
			
		}else {
			if(collision.gameObject.name==NOMBRE_BOTON_PLAY){
				seleccionarCamara(CAMARA_PLAYER);
				audio.Play();
			}else if(collision.gameObject.name==NOMBRE_BOTON_REBOBINAR){
				seleccionarCamara(CAMARA_PLAYER);
				playAudioNext("1");
			}else if(collision.gameObject.name==NOMBRE_BOTON_SALIR){
				Application.LoadLevel(NOMBRE_ANTERIOR);
			}
		}
		clickCount++;
	}

}