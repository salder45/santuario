using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NITE;
using OpenNI;
using System;

public class MenuRecorridoGuiado : MonoBehaviour {
	//KINECT
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	public Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private HandsGenerator hands;
	private GestureGenerator gesture;
	
	//Constantes
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
	private const float MOVIMIENTO=1f;
	
	private float zInicial=0f;
	Camera camaraMenu;
	Camera camaraPlayer;
	GameObject puntoInicial;
	GameObject puntoFinal;
	GameObject player;
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
	//variables prueba
	private float contador=0f;
	//Movimiento
	//variable que guarda en que posicion de la lista se encuentra
	private int posicionActual=0;
	//Lista del orden 
	public List<List<Movimiento>> orden;
	public List<Movimiento> movimientos;
	
	// Use this for initialization
	void Start(){
		player=GameObject.Find("Player");
		camaraPlayer=GameObject.Find(CAMARA_PLAYER).camera;		
		camaraMenu=GameObject.Find(CAMARA_MENU).camera;
		seleccionarCamara(camaraMenu.ToString());
		puntoInicial=GameObject.Find("puntoInicial");
		puntoFinal=GameObject.Find("puntoFinal");
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
		
		//handdlers
		this.hands.HandCreate+=hands_HandCreate;
		this.hands.HandUpdate+=hands_HandUpdate;
		this.hands.HandDestroy+=hands_HandDestroy;

		this.gesture.AddGesture(WAVE);
		this.gesture.AddGesture(CLICK);
		this.gesture.GestureRecognized+=gesture_GestureRecognized;
		this.gesture.StartGenerating();
		//iniciaPuntos
		iniciaPuntos();
}
	
	// Update is called once per frame
	void Update (){
		this.context.WaitOneUpdateAll(this.depth);
	}
	
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		context.Release();
	}
	
	void hands_HandCreate(object sender, HandCreateEventArgs e){
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
		Debug.Log(e.Gesture);
		if(e.Gesture==WAVE){
			this.hands.StartTracking(e.EndPosition);
		}else if(e.Gesture==CLICK){
			seleccionarCamara(CAMARA_MENU);
		}
	}
	
	void determinaEspacioUnity(){
		xInit=camaraMenu.transform.position.x+UNITY_MEDIDA_X_INIT;
		yInit=camaraMenu.transform.position.y-UNITY_MEDIDA_Y;
		zInit=camaraMenu.transform.position.z+UNITY_MEDIDA_Z;
		xEnd=camaraMenu.transform.position.x+UNITY_MEDIDA_X_END;
		yEnd=camaraMenu.transform.position.y+UNITY_MEDIDA_Y;
		zEnd=camaraMenu.transform.position.z-UNITY_MEDIDA_Z;
		puntoInicial.transform.position=new Vector3(xInit,yInit,zInit);
		puntoFinal.transform.position=new Vector3(xEnd,yEnd,zEnd);
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
	
	bool isActiveMainCamera(){
		return camaraPlayer.enabled;
	}
	
	void seleccionarCamara(string nombre){
		//checar el cambiar de camaras el error es que las do quedan desactivadas el detalle es que solo regresa las camaras enabled en ese momento
		if(nombre==CAMARA_PLAYER){
			camaraPlayer.enabled=true;
			camaraMenu.enabled=false;
		}else {
			camaraPlayer.enabled=false;
			camaraMenu.enabled=true;
		}
	}
	void OnCollisionStay(Collision collision){
		if(contador>50f){
			seleccionarCamara(CAMARA_PLAYER);
			contador=0f;
			avanzar();
		}		
		contador++;
	}
	
	void OnCollisionExit(Collision collision){
		contador=0f;
	}
	
	void iniciaPuntos(){
		orden=new List<List<Movimiento>>();		
		//Movimiento1
		List<Movimiento> movsUno=new List<Movimiento>();
		Movimiento m01=new Movimiento();
		m01.distancia=30f;
		m01.eje=EJE_X;
		movsUno.Add(m01);
		orden.Add(movsUno);
		//movimiento2
		List<Movimiento> movsDos=new List<Movimiento>();
		Movimiento m10=new Movimiento();
		m10.ejeRotacion=EJE_Y;
		//m10.rotacion=0.001f;
		m10.rotacion=90f;
		movsDos.Add(m10);
		orden.Add(movsDos);
	}
	
	void ejecutaVariosMovimientos(int posAc,bool isAvanzar){
		List<Movimiento> movs=orden[posAc];
		foreach(Movimiento m in movs){
			StartCoroutine(ejecutaMovimiento(m,isAvanzar));
		}
	}
	
	IEnumerator ejecutaMovimiento(Movimiento movimiento,bool isAvanzar){
		float x,y,z;
		x=player.transform.position.x;
		y=player.transform.position.y;
		z=player.transform.position.z;
		Debug.Log("*"+movimiento.ejeRotacion+"*");
		if(isAvanzar){
			if(movimiento.ejeRotacion.Equals("")){
				Debug.Log("Mover");
				if(movimiento.eje==EJE_X){
					x+=movimiento.distancia;
				}else if(movimiento.eje==EJE_Y){
					y+=movimiento.distancia;
				}else if(movimiento.eje==EJE_Z){
					z+=movimiento.distancia;
				}
			 	float i=0f;
				float tiempo=10f;
				float ratio=1f/tiempo;
				while(i<1.0f){
					i+=Time.deltaTime*ratio;
					player.transform.position=Vector3.Lerp(player.transform.position,new Vector3(x,y,z),i);
					yield return null;
				}
			}else{
				Debug.Log("Rotar");
				float xR,yR,zR;
				xR=player.transform.rotation.x;
				yR=player.transform.rotation.y;
				zR=player.transform.rotation.z;
				
				if(movimiento.ejeRotacion==EJE_X){
					xR+=movimiento.rotacion;
				}else if(movimiento.ejeRotacion==EJE_Y){
					yR+=movimiento.rotacion;
				}else if(movimiento.ejeRotacion==EJE_Z){
					zR+=movimiento.rotacion;
				}
				
				float i=0.5f;
				float tiempo=2f;
				float ratio=1f/tiempo;
				while(i<1.0f){
					i+=0.2f;
					player.transform.Rotate(Vector3.Slerp(player.transform.rotation.eulerAngles,new Vector3(xR,yR,zR),i));
					yield return null;
					/*
					i+=Time.deltaTime*ratio;
					player.transform.rotation=Quaternion.Slerp(player.transform.rotation,new Quaternion(xR,yR,zR,0f),i);
					yield return null;
					*/
				}
			}			
		}
	}
	//Movimientos
	void avanzar(){
		ejecutaVariosMovimientos(posicionActual,true);
		posicionActual++;
	}
	
	void retroceder(){
		ejecutaVariosMovimientos(posicionActual,false);
		posicionActual--;
	}
	//Audio
	void repetir(){		
	}
	
	void detener(){		
	}	
	
	void salir(){
	}
}
