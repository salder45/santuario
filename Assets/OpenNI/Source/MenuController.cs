using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using NITE;
using OpenNI;

public class MenuController : MonoBehaviour {
	//kinect
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private HandsGenerator hands;
	private GestureGenerator gesture;
	//puntos inicial y final
	private float xInit=-14f;
	private float yInit=-2f;
	private float zInit=0f;
	private float xEnd=2.52f;
	private float yEnd=6.18f;
	private float zEnd=6f;
	//algoritmo
	
	//constantes
	private const string WAVE="Wave";
	private const float MEDIDA_MANO_X=400f;
	private const float MEDIDA_MANO_Y=400f;
	private const float MEDIDA_MANO_Z=300f;
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
	//distancias entre los puntos
	private float distanciaX=0f;
	private float distanciaY=0f;
	private float distanciaZ=0f;
	//puntos medios
	private Point3D puntoMitad;
	private float xMedia=0f;
	private float yMedia=0f;
	private float zMedia=0f;
	
	//Seleccion de opciones en el menu
	public string nivel;
	public float frameDeEspera ;
	private float contador;
	
	// Use this for initialization
	void Start () {
		Debug.Log("Start");
		
		//Debug.Log("START");
		determinaCentro();

		//Debug.Log("X "+xMedia+" Y "+yMedia+" Z "+zMedia);
		transform.position=new Vector3(xMedia,yMedia,zMedia);
		
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
		
		//handdlers
		this.hands.HandCreate+=hands_HandCreate;
		this.hands.HandUpdate+=hands_HandUpdate;
		this.hands.HandDestroy+=hands_HandDestroy;

		this.gesture.AddGesture(WAVE);
		this.gesture.GestureRecognized+=gesture_GestureRecognized;
		this.gesture.StartGenerating();
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("Update");
		this.context.WaitOneUpdateAll(this.depth);
	}
	
	//apagar sensor
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		context.Release();
	}
	
	//handlers
	void hands_HandCreate(object sender, HandCreateEventArgs e){
		//Debug.Log("Mano Creada");
		this.puntoMano=e.Position;
		setPuntosManoActuales(this.puntoMano);
		iniciaPuntoReferencia(this.puntoMano);
		//Debug.Log("X "+puntoMano.X+" Y "+puntoMano.Y+" Z "+puntoMano.Z);
		//Debug.Log("**** X "+kinectRefX+" Y "+kinectRefY+" Z "+kinectRefZ);

	}

	void hands_HandUpdate(object sender, HandUpdateEventArgs e){
		//Debug.Log("Mano Update");
		setPuntosManoActuales(e.Position);
		distanciaKinectReales();
		calculaMovimiento();
	}

	void hands_HandDestroy(object sender, HandDestroyEventArgs e){
		//Debug.Log("Mano destroy");
		transform.position=new Vector3(xMedia,yMedia,zMedia);
	}
	
	void gesture_GestureRecognized(object sender, GestureRecognizedEventArgs e){
		if(e.Gesture==WAVE){
			this.hands.StartTracking(e.EndPosition);
		}
	}
	
	//algoritmos
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
	
	void calculaMovimiento(){
		float x=transform.position.x;
		float y=transform.position.y;
		float z=transform.position.z;
		
		if(kinectRefX>kinectManoX){
			float xNormal=((kinectDistanciaX/10f)*100f)/(MEDIDA_MANO_X/10f);
			x=(xNormal*distanciaX)/100;
			x=(xInit+x)-4f;
		}
		
		if(kinectRefY<kinectManoY){
			float yNormal=((kinectDistanciaY/10f)*100f)/(MEDIDA_MANO_Y/10f);
			y=(yNormal*distanciaY)/100;
			y=yInit+y;
		}
		
		if(kinectRefZ>kinectManoZ){
			float zNormal=((kinectDistanciaZ/10f)*100f)/(MEDIDA_MANO_Z/10f);		
			z=(zNormal*distanciaZ)/100;
		}
		
		transform.position=new Vector3(x,y,z);
	}
	
	float distanciaEntreDosPuntos(Point3D a,Point3D b){
		return Mathf.Sqrt(elevaCuadrado(a.X-b.X)+elevaCuadrado(a.Y-b.Y)+elevaCuadrado(a.Z-b.Z));
	}

	float elevaCuadrado(float numero){
		return Mathf.Pow(numero,2f);
	}
	
	//determina centro
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
		this.xMedia=xInit+(distanciaX/2);
		this.yMedia=yInit+(distanciaY/2);
		this.zMedia=zInit+(distanciaZ/2);
	}

	
	//Codigo para seleccion con espera en seleccion (espera de frames no segundos)
	
	void OnCollisionStay(Collision collisionInfo) {    
		Debug.Log ("Stay "+contador);
		contador ++;
		if(frameDeEspera<contador ){
			context.Release();
			Application.LoadLevel(nivel);
			
		}
    }
	
	 void OnCollisionExit(Collision collision) {
		Debug.Log("CollisionExit");
		contador=0;
        
    }
	
}
