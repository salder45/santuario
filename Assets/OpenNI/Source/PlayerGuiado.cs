using UnityEngine;
using System.Collections;
using OpenNI;
using NITE;
using System;

[RequireComponent (typeof (CharacterController))]
public class PlayerGuiado : MonoBehaviour {
	//KINECT
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	public Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private HandsGenerator hands;
	private GestureGenerator gesture;
	
	
	//Constantes
	private const string WAVE="Wave";
	private const float MEDIDA_MANO_X=400f;
	private const float MEDIDA_MANO_Y=400f;
	private const float MEDIDA_MANO_Z=300f;
	private const float MEDIDA_UNITY_X=3f;
	private const float MEDIDA_UNITY_Y_INICIAL=2f;
	private const float MEDIDA_UNITY_Y_FINAL=3f;
	private const float MEDIDA_UNITY_Z=5f;
	//Botones Mano
	GameObject avanzar;
	GameObject retroceder;
	GameObject repetir;
	GameObject mano;
	//
	private float zInicial=0f;
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
	//punto de referencia para el kinect
	private float kinectRefX=0f;
	private float kinectRefY=0f;
	private float kinectRefZ=0f;
	//puntos mano
	private Point3D puntoMano;
	private float kinectManoX=0f;
	private float kinectManoY=0f;
	private float kinectManoZ=0f;
	//distancias reales kinect
	private float kinectDistanciaX=0f;
	private float kinectDistanciaY=0f;
	private float kinectDistanciaZ=0f;
	
	void Start () {
		//inicia objetos
		avanzar=GameObject.Find("BotonAvanzar");
		repetir=GameObject.Find("BotonRepetir");
		retroceder=GameObject.Find("BotonRetroceder");
		mano=GameObject.Find("Mano");
		determinaCentro();
		acomodaManoDerecha();
		
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
	
	void hands_HandCreate(object sender, HandCreateEventArgs e){
		determinaCentro();
		acomodaManoCentro();
	}
	
	void hands_HandUpdate(object sender, HandUpdateEventArgs e){
	}
	
	void hands_HandDestroy(object sender, HandDestroyEventArgs e){
		determinaCentro();
		acomodaManoDerecha();
	}
	void gesture_GestureRecognized(object sender, GestureRecognizedEventArgs e){
		if(e.Gesture==WAVE){
			hands.StartTracking(e.EndPosition);
		}
		
	}
	
	
	// Update is called once per frame
	void Update () {		
		this.context.WaitOneUpdateAll(this.depth);
	}
	
	//apagar sensor
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		context.Release();
	}
	
	void determinaCentro(){
		calculaPosicionInicialUnity();
		calculaDistanciaEje();
		calculaPuntoMedio();
	}
	
	void calculaPosicionInicialUnity(){
		this.xInit=this.repetir.transform.position.x-MEDIDA_UNITY_X;
		this.xEnd=this.repetir.transform.position.x+MEDIDA_UNITY_X;
		this.yInit=this.repetir.transform.position.y-MEDIDA_UNITY_Y_INICIAL;
		this.yEnd=this.repetir.transform.position.y+MEDIDA_UNITY_Y_FINAL;
		this.zInit=this.repetir.transform.position.z-MEDIDA_UNITY_Z;
		this.zEnd=this.repetir.transform.position.z+MEDIDA_UNITY_Z;		
	}
	
	void calculaDistanciaEje(){
		this.distanciaX=distanciaEntreDosPuntos(new Point3D(xInit,0f,0f),new Point3D(xEnd,0f,0f));
		Debug.Log(distanciaX+" Init "+xInit+" End "+xEnd);
		this.distanciaY=distanciaEntreDosPuntos(new Point3D(0f,yInit,0f),new Point3D(0f,yEnd,0f));
		this.distanciaZ=distanciaEntreDosPuntos(new Point3D(0f,0f,zInit),new Point3D(0f,0f,zEnd));
	}
	
	void calculaPuntoMedio(){
		this.xMedia=xInit+(distanciaX/2);
		Debug.Log("PosicionMitad "+xMedia);
		this.yMedia=yInit+(distanciaY/2);
		this.zMedia=zInit+(distanciaZ/2);
	}
	
	float distanciaEntreDosPuntos(Point3D a,Point3D b){
		return Mathf.Sqrt(elevaCuadrado(a.X-b.X)+elevaCuadrado(a.Y-b.Y)+elevaCuadrado(a.Z-b.Z));
	}

	float elevaCuadrado(float numero){
		return Mathf.Pow(numero,2f);
	}
	
	void acomodaManoDerecha(){
		mano.transform.position=new Vector3(xMedia,yMedia,avanzar.transform.position.z);
	}
	
	void acomodaManoCentro(){
		mano.transform.position=new Vector3(xMedia,yMedia,zMedia);
	}	
}
