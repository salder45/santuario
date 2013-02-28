using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NITE;
using OpenNI;
using System;

[RequireComponent(typeof(AudioSource))]
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
	bool estaCorriendo=false;
	
	//Cnotador del click
	private float clickCount = 0;
	private float exitCount=100;
	private string siguienteNivel;
	
	public AudioClip clip1;
	public AudioClip clip2;
	public AudioClip clip3;
	public AudioClip clip4;
	public AudioClip clip5;
	public AudioClip clip6;
	public AudioClip clip7;
	private String playMemory;
	private bool isMenu=false;
	
	
	// Use this for initialization
	void Start(){
		player=GameObject.Find("Player");
		Debug.Log ("---------"+player.transform.localPosition);
		camaraMenu=GameObject.Find(CAMARA_MENU).camera;
		camaraPlayer=GameObject.Find(CAMARA_PLAYER).camera;	
		seleccionarCamara(CAMARA_PLAYER);
		playMemory="1";
		audio.clip = clip1;
		audio.Play();
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
		
		fadeCountCircle();
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
		
		if(!isMenu){
			if(!audio.isPlaying){
				playAudioNext(playMemory);
				
			}
		}
	}
	
	void playAudioNext(String memoria){
		Debug.Log ("----------"+memoria);
		
		switch (memoria){
			case "0": audio.clip =clip1; playMemory="1"; audio.Play(); avanzar();
			break;
			case "1": audio.clip =clip2; playMemory="2";  audio.Play(); avanzar();
			break;
			case "2": audio.clip =clip3;   playMemory="3";audio.Play(); avanzar();
			break;
			case "3":audio.clip =clip4;   playMemory="4";audio.Play(); avanzar();
			break;
			case "4": audio.clip =clip5;   playMemory="5";audio.Play(); avanzar();
			break;
			case "5": audio.clip =clip6;   playMemory="6";audio.Play(); avanzar();
			break;
			case "6": audio.clip =clip7;   playMemory="0";audio.Play(); avanzar();
			break;
			
		}
		
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
			Debug.Log("Pausa");
			isMenu=true;
			audio.Pause();
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
			isMenu=false;
		}else {
			camaraPlayer.enabled=false;
			camaraMenu.enabled=true;
		}
	}
	void OnCollisionStay(Collision collision){
		/*if(contador>50f){
			seleccionarCamara(CAMARA_PLAYER);
			contador=0f;
			if(posicionActual>5){
				retroceder();
			}else{
				avanzar();
			}
		}		
		contador++;*/
			Debug.Log(clickCount);
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
			
		}else{
			
			if(collision.gameObject.name=="botonAtras"){
				audio.Stop();
				switch (playMemory){
					case "0": audio.clip =clip1; playMemory="1"; audio.Play(); retroceder();
					break;
					case "1": audio.clip =clip2; playMemory="1";  audio.Play(); retroceder();
					break;
					case "2": audio.clip =clip3;   playMemory="2";audio.Play(); retroceder();
					break;
					case "3":audio.clip =clip4;   playMemory="3";audio.Play(); retroceder();
					break;
					case "4": audio.clip =clip5;   playMemory="4";audio.Play(); retroceder();
					break;
					case "5": audio.clip =clip6;   playMemory="5";audio.Play(); retroceder();
					break;
					case "6": audio.clip =clip7;   playMemory="6";audio.Play(); retroceder();
					break;
					
				}
				seleccionarCamara(CAMARA_PLAYER);
			}else if(collision.gameObject.name=="botonDelante"){
				audio.Stop();
				seleccionarCamara(CAMARA_PLAYER);
			}else if(collision.gameObject.name=="botonRegresar"){
				continuar();
			}else if(collision.gameObject.name=="botonSalir"){
				salir();
			}
			 
			//exitCount = 0;
			//collision.collider.enabled= false;
			
		}
			
		
		clickCount++;
	}
	
	void OnCollisionExit(Collision collision){
		contador=0f;
		fadeCountCircle();
		clickCount=0;
	}
	
	void iniciaPuntos(){
		orden=new List<List<Movimiento>>();		
		//Movimiento1
		List<Movimiento> movsUno=new List<Movimiento>();
		Movimiento mv1=new Movimiento();
		mv1.distancia=30f;
		mv1.eje=EJE_X;
		movsUno.Add(mv1);
		orden.Add(movsUno);
		//movimiento2
		List<Movimiento> movsDos=new List<Movimiento>();
		Movimiento mv2=new Movimiento();
		mv2.ejeRotacion=EJE_Y;
		mv2.rotacion=90f;		
		movsDos.Add(mv2);
		//
		Movimiento mv2Uno=new Movimiento();
		mv2Uno.eje=EJE_Z;
		mv2Uno.distancia=-7f;
		movsDos.Add(mv2Uno);
		//
		Movimiento mv2Dos=new Movimiento();
		mv2Dos.ejeRotacion=EJE_Y;
		mv2Dos.rotacion=-90f;
		movsDos.Add(mv2Dos);
		
		Movimiento mv2Tres=new Movimiento();
		mv2Tres.distancia=20f;
		mv2Tres.eje=EJE_X;
		movsDos.Add(mv2Tres);
		
		Movimiento mv2Cuatro=new Movimiento();
		mv2Cuatro.ejeRotacion=EJE_Y;
		mv2Cuatro.rotacion=-90f;		
		movsDos.Add(mv2Cuatro);
		
		Movimiento mv2Cinco=new Movimiento();
		mv2Cinco.eje=EJE_Z;
		mv2Cinco.distancia=7f;
		movsDos.Add(mv2Cinco);
		
		Movimiento mv2Seis=new Movimiento();
		mv2Seis.ejeRotacion=EJE_Y;
		mv2Seis.rotacion=90f;		
		movsDos.Add(mv2Seis);
		
		orden.Add(movsDos);
		//
		List<Movimiento> movsTres=new List<Movimiento>();
		Movimiento mv3Uno=new Movimiento();
		mv3Uno.ejeRotacion=EJE_Y;
		mv3Uno.rotacion=90f;		
		movsTres.Add(mv3Uno);
		
		Movimiento mv3Dos=new Movimiento();
		mv3Dos.eje=EJE_Z;
		mv3Dos.distancia=-5f;		
		movsTres.Add(mv3Dos);
		
		Movimiento mv3Tres=new Movimiento();
		mv3Tres.ejeRotacion=EJE_Y;
		mv3Tres.rotacion=-90f;		
		movsTres.Add(mv3Tres);
		
		Movimiento mv3Cuatro=new Movimiento();
		mv3Cuatro.eje=EJE_X;
		mv3Cuatro.distancia=15f;		
		movsTres.Add(mv3Cuatro);
		
		Movimiento mv3Cinco=new Movimiento();
		mv3Cinco.ejeRotacion=EJE_Y;
		mv3Cinco.rotacion=-90f;		
		movsTres.Add(mv3Cinco);
		
		Movimiento mv3Seis=new Movimiento();
		mv3Seis.eje=EJE_Z;
		mv3Seis.distancia=5f;		
		movsTres.Add(mv3Seis);
		
		Movimiento mv3Siete=new Movimiento();
		mv3Siete.ejeRotacion=EJE_Y;
		mv3Siete.rotacion=90f;		
		movsTres.Add(mv3Siete);
		
		Movimiento mv3Ocho=new Movimiento();
		mv3Ocho.eje=EJE_X;
		mv3Ocho.distancia=5f;		
		movsTres.Add(mv3Ocho);
		
		Movimiento mv3Nueve=new Movimiento();
		mv3Nueve.eje=EJE_X;
		mv3Nueve.distancia=10f;		
		movsTres.Add(mv3Nueve);
		
		orden.Add(movsTres);
		//
		List<Movimiento> movsCuatro=new List<Movimiento>();
		
		Movimiento mv4Uno=new Movimiento();
		mv4Uno.ejeRotacion=EJE_Y;
		mv4Uno.rotacion=90f;		
		movsCuatro.Add(mv4Uno);
		
		Movimiento mv4Dos=new Movimiento();
		mv4Dos.eje=EJE_Z;
		mv4Dos.distancia=2f;		
		movsCuatro.Add(mv4Dos);		
		
		orden.Add(movsCuatro);
		//
		List<Movimiento> movsCinco=new List<Movimiento>();
		Movimiento mv5Uno=new Movimiento();
		mv5Uno.ejeRotacion=EJE_Y;
		mv5Uno.rotacion=180f;		
		movsCinco.Add(mv5Uno);
		
		Movimiento mv5Dos=new Movimiento();
		mv5Dos.eje=EJE_Z;
		mv5Dos.distancia=-2f;		
		movsCinco.Add(mv5Dos);
		
		orden.Add(movsCinco);
		//
		List<Movimiento> movsSeis=new List<Movimiento>();
		
		Movimiento mv6Uno=new Movimiento();
		mv6Uno.ejeRotacion=EJE_Y;
		mv6Uno.rotacion=90f;		
		movsSeis.Add(mv6Uno);
		
		Movimiento mv6Dos=new Movimiento();
		mv6Dos.eje=EJE_X;
		mv6Dos.distancia=10f;		
		movsSeis.Add(mv6Dos);
		
		orden.Add(movsSeis);
		
	}
	
	IEnumerator ejecutaVariosMovimientos(int posAc){
		List<Movimiento> movs=orden[posAc];
		int i=0;
		while(i<movs.Count){
			if(!estaCorriendo){
				StartCoroutine(ejecutaMovimiento(movs[i]));
				i++;				
			}else{
				yield return new WaitForSeconds(0.0001f);
			}
		}
	}
	
	IEnumerator ejecutaVariosMovimientosRegresar(int posAc){
		//Debug.Log(posAc+" ** "+orden.Count);
		if(posAc>(orden.Count-1)){
			//Debug.Log("Es el ultimo");
			posAc=posAc-1;
			posicionActual=posicionActual-1;
		}
		
		List<Movimiento> movs=orden[posAc];
		Movimiento voltea=new Movimiento();
		voltea.ejeRotacion=EJE_Y;
		voltea.rotacion=180f;
		StartCoroutine(ejecutaMovimiento(voltea));
		int i=movs.Count-1;
		while(i>=0){
			if(!estaCorriendo){
				Movimiento m=movs[i];
				//Debug.Log(m.eje+" ** "+m.distancia);
				//Debug.Log(m.ejeRotacion+" ** "+m.rotacion);
				
				if(m.ejeRotacion.Equals("")){
					m.distancia=(m.distancia*-1.0f);
				}else {
					m.rotacion=(m.rotacion*-1.0f);					
				}
				//Debug.Log(m.eje+" ++ "+m.distancia);
				//Debug.Log(m.ejeRotacion+" ++ "+m.rotacion);
				StartCoroutine(ejecutaMovimiento(m));
				i--;				
			}else{
				yield return new WaitForSeconds(0.0001f);
			}
		}		
		StartCoroutine(ejecutaMovimiento(voltea));		
	}
	
	IEnumerator ejecutaMovimiento(Movimiento movimiento){
		estaCorriendo=true;
		float x,y,z;
		x=player.transform.position.x;
		y=player.transform.position.y;
		z=player.transform.position.z;
		if(movimiento.ejeRotacion.Equals("")){
			//Debug.Log("Mover");
			if(movimiento.eje==EJE_X){
				x+=movimiento.distancia;
			}else if(movimiento.eje==EJE_Y){
				y+=movimiento.distancia;
			}else if(movimiento.eje==EJE_Z){
				z+=movimiento.distancia;
			}
						
			float i=100f;
			float avance=movimiento.distancia/i;
			float tmp=0;  
			while(tmp<i){
				player.transform.position=Vector3.Lerp(player.transform.position,new Vector3(x,y,z),tmp*0.01f);
				tmp++;
				yield return null;	
			}
		}else{
			//Debug.Log("Rotar");
			float xR,yR,zR;
			xR=0f;
			yR=0f;
			zR=0f;
			float i=100f;
			float avance=movimiento.rotacion/i;
			float tmp=0;
			while(tmp<i){
				//Debug.Log("Esta rotando");
				if(movimiento.ejeRotacion==EJE_X){
					xR=avance;
				}else if(movimiento.ejeRotacion==EJE_Y){
					yR=avance;
				}else if(movimiento.ejeRotacion==EJE_Z){
					zR=avance;
				}
				tmp++;
				player.transform.Rotate(Vector3.Slerp(player.transform.eulerAngles,new Vector3(xR,yR,zR),1));
				yield return null;					
			}
		}			
		
		estaCorriendo=false;
	}

	//Movimientos
	void avanzar(){
		Debug.Log("avanzar");
		StartCoroutine(ejecutaVariosMovimientos(posicionActual));
		posicionActual++;
	}
	
	void retroceder(){
		Debug.Log("Retroceder");
		StartCoroutine(ejecutaVariosMovimientosRegresar(posicionActual));
		posicionActual--;
	}
	//Audio
	void continuar(){	
		Debug.Log("continuar");
		seleccionarCamara(CAMARA_PLAYER);
		audio.Play();
	}
	
	void detener(){
		Debug.Log("detener");
	}	
	
	void salir(){
		Debug.Log("salir");
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
}
