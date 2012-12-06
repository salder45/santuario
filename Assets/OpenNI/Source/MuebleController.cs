using UnityEngine;
using System.Collections;
using NITE;
using OpenNI;
using System;

public class MuebleController : MonoBehaviour {
	//OpenNI
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	private const string NOMBRE_ANTERIOR="Santuario";
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	//Algoritmo
	private Point3D puntoTorso;
	//Constantews
	private const float MEDIDA_SEGURIDAD_FRENTE_Z=50;
	private const float MEDIDA_SEGURIDAD_CENTRAL=350;
	private const float MEDIDA_SEGURIDAD_MANOS=100;
	private const float LIMITE_ROTACION_VERTICAL=0.3f;
	
	//User Tracking
	private UserGenerator userGenerator;
	private SkeletonCapability skeletonCapability;
	private PoseDetectionCapability poseDetectionCapability;
	private string calibPose;
	private bool shouldRun;
	//
	public float valorRotation=1f;
	public float escala=1f;
	private const float TAMANO_SECTOR=100;
	private const int ESCALA_DISTANCIA=1;
	
	//Contadores para el fade
	private float fadeCount=0;
	private float exitCount=0;
	private bool salir = false;
	
	//Objetos para ocultar y Audio Source
	public GameObject fade;
	
	// Use this for initialization
	void Start () {
		Debug.Log("Start");
		this.context=Context.CreateFromXmlFile(XML_CONFIG, out scriptNode);
		this.depth=this.context.FindExistingNode(NodeType.Depth) as DepthGenerator;
		if(depth==null){
			throw new Exception("Nodo de Profundidad no encontrado");
		}
		this.userGenerator=new UserGenerator(this.context);
		this.skeletonCapability=this.userGenerator.SkeletonCapability;
		this.poseDetectionCapability=this.userGenerator.PoseDetectionCapability;
		this.calibPose=this.skeletonCapability.CalibrationPose;
		
		this.userGenerator.NewUser+=userGenerator_NewUser;
		this.userGenerator.LostUser+=userGenerator_LostUser;
		this.poseDetectionCapability.PoseDetected+=poseDetectionCapability_PoseDetected;
		this.skeletonCapability.CalibrationComplete+=skeletonCapability_CalibrationComplete;
		
		this.skeletonCapability.SetSkeletonProfile(SkeletonProfile.All);
		this.userGenerator.StartGenerating();
	
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("Update");
		if(fadeCount<100){
			fadeCount++;
			Color color = fade.renderer.material.color;
			color.a = 1f;
			color.a -= fadeCount/100;
			fade.renderer.material.color = color;
		}
		
		this.context.WaitOneUpdateAll (this.depth);
		int[] users=this.userGenerator.GetUsers();
			foreach(int user in users){
			if(this.skeletonCapability.IsTracking(user)){
				updatePuntoRef(skeletonCapability.GetSkeletonJointPosition(user,SkeletonJoint.Torso));
				SkeletonJointPosition posHandIz=skeletonCapability.GetSkeletonJointPosition(user,SkeletonJoint.RightHand);
				SkeletonJointPosition posHandDr=skeletonCapability.GetSkeletonJointPosition(user,SkeletonJoint.LeftHand);
				
				if(isDentroCuadroSeguridad(posHandDr.Position)&isDentroCuadroSeguridad(posHandIz.Position)){
					float dist=distanciaEntreDosPuntos(posHandDr.Position,posHandIz.Position)/100f;					
					float noNormalActual=(dist*1f)/ESCALA_DISTANCIA;
					int disNormal=(int)dist;
					float esc=(disNormal*1f)/ESCALA_DISTANCIA;
					if(Mathf.Abs(noNormalActual-esc)<0.4f&&Mathf.Abs(noNormalActual-esc)>-0.1f){
						esc=esc+escala;
						transform.localScale=Vector3.Lerp(transform.localScale,new Vector3(esc,esc,esc),Time.time);
					}					

					//Rotacion horizontal
					float x,y,z;
					x=y=z=0;									
					if((posHandDr.Position.Z<posHandIz.Position.Z&isDentroMargenY(posHandDr.Position.Y,posHandIz.Position.Y))&(!isDentroMargenZ(posHandDr.Position.Z,posHandIz.Position.Z))){
						y=-valorRotation;						
					}else if((posHandDr.Position.Z>posHandIz.Position.Z&isDentroMargenY(posHandDr.Position.Y,posHandIz.Position.Y))&(!isDentroMargenZ(posHandDr.Position.Z,posHandIz.Position.Z))){
						y=valorRotation;						
					}
					//rotacion vertical
					//Frente
					
					float tmp=transform.eulerAngles.x;
					if(tmp>35&tmp<318){
						tmp=35;
					}else if(tmp<320&tmp>36){
						tmp=320;
					}
					
					Debug.Log(transform.eulerAngles.x+" ==== "+tmp);
					
					if((posHandDr.Position.Y<posHandIz.Position.Y&isDentroMargenZ(posHandDr.Position.Z,posHandIz.Position.Z))&(!isDentroMargenY(posHandDr.Position.Y,posHandIz.Position.Y))){
						//|(transform.eulerAngles.x-2<=35||transform.eulerAngles.x+2>=320)
						if((transform.eulerAngles.x<=35||transform.eulerAngles.x>=320)|tmp==35){
							//Debug.Log("Frente"+transform.eulerAngles.x);
							x=-valorRotation;													
						}
							//y positivo x negativo
						//Atras
					}else if((posHandDr.Position.Y>posHandIz.Position.Y&isDentroMargenZ(posHandDr.Position.Z,posHandIz.Position.Z))&(!isDentroMargenY(posHandDr.Position.Y,posHandIz.Position.Y))){
						//|(transform.eulerAngles.x-2<=35||transform.eulerAngles.x+2>=320)
						if((transform.eulerAngles.x<=35||transform.eulerAngles.x>=320)|tmp==320){
							//Debug.Log("Atras"+transform.eulerAngles.x);
							x=valorRotation;							
						}
						//y negativo x positivo
					}
					
					//Debug.Log("EulerAngles"+transform.eulerAngles);
					//Debug.Log("LocalEulerAngles"+transform.localEulerAngles);
					//Debug.Log("X "+transform.rotation.x+" Y "+transform.rotation.y+" Z "+transform.rotation.z);
					//Debug.Log("XL "+transform.localRotation.x+" YL "+transform.localRotation.y+" ZL "+transform.localRotation.z);
					transform.Rotate(new Vector3(x,y,z));
					
				
					
				}else {
					
					transform.rotation=new Quaternion(0f,0f,0f,0f);
				}
				//x
				//positivo es hacia arriba
				//negativo hacia abajo
				SkeletonJointPosition cabeza=skeletonCapability.GetSkeletonJointPosition(user,SkeletonJoint.Head);
				if((posHandDr.Position.Y>cabeza.Position.Y)|(posHandIz.Position.Y>cabeza.Position.Y)){
					Debug.Log("Debe salir");
					salir=true;
				}
							
			}
		}
		if(salir){
			exitCount++;
			Color color = fade.renderer.material.color;
			color.a = 0f;
			color.a += exitCount/50;
			fade.renderer.material.color = color;
			if(exitCount==49){
				Debug.Log(exitCount);
				context.Release();
				Application.LoadLevel(NOMBRE_ANTERIOR);
			}
			
		}
		
	}
	
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		context.Release();
	}
	
	//User Generator Handlers
	void userGenerator_NewUser(object sender, NewUserEventArgs e){
          if (this.skeletonCapability.DoesNeedPoseForCalibration){
            	this.poseDetectionCapability.StartPoseDetection(this.calibPose, e.ID);
           }else{
            	this.skeletonCapability.RequestCalibration(e.ID, true);
            }
    }

	void poseDetectionCapability_PoseDetected(object sender, PoseDetectedEventArgs e){
            this.poseDetectionCapability.StopPoseDetection(e.ID);
            this.skeletonCapability.RequestCalibration(e.ID, true);
    }

	void skeletonCapability_CalibrationComplete(object sender, CalibrationProgressEventArgs e){
            if (e.Status == CalibrationStatus.OK){
                this.skeletonCapability.StartTracking(e.ID);
            }else if (e.Status != CalibrationStatus.ManualAbort){
                if (this.skeletonCapability.DoesNeedPoseForCalibration){
                    this.poseDetectionCapability.StartPoseDetection(calibPose, e.ID);
                }else{
                    this.skeletonCapability.RequestCalibration(e.ID, true);
                }
            }
    }

	void userGenerator_LostUser(object sender, UserLostEventArgs e){		
	}
	
	void updatePuntoRef(SkeletonJointPosition punto){
		this.puntoTorso=punto.Position;
	}
	
	bool isDentroCuadroSeguridad(Point3D puntoMano){
		bool retorno=false;
		float x,y,z;
		x=puntoTorso.X;
		y=puntoTorso.Y;
		z=puntoTorso.Z;

		if((x+MEDIDA_SEGURIDAD_CENTRAL>puntoMano.X&&x-MEDIDA_SEGURIDAD_CENTRAL<puntoMano.X)&&
			(y+MEDIDA_SEGURIDAD_CENTRAL>puntoMano.Y&&y-MEDIDA_SEGURIDAD_CENTRAL<puntoMano.Y)&&
			(z-((2*MEDIDA_SEGURIDAD_CENTRAL)+MEDIDA_SEGURIDAD_FRENTE_Z)<puntoMano.Z&&z-MEDIDA_SEGURIDAD_FRENTE_Z>puntoMano.Z)){
			retorno=true;
		}	

		return retorno;
	}

	bool isDentroMargenX(float xManoDr, float xManoIz){
		bool retorno=false;
		if(xManoDr+MEDIDA_SEGURIDAD_MANOS>=xManoIz&&xManoDr-MEDIDA_SEGURIDAD_MANOS<=xManoIz){
			retorno=true;
		}
		return retorno;
	}

	bool isDentroMargenY(float yManoDr, float yManoIz){
		bool retorno=false;
		if(yManoDr+MEDIDA_SEGURIDAD_MANOS>=yManoIz&&yManoDr-MEDIDA_SEGURIDAD_MANOS<=yManoIz){
			retorno=true;
		}
		return retorno;
	}

	bool isDentroMargenZ(float zManoDr, float zManoIz){
		bool retorno=false;
		if(zManoDr+MEDIDA_SEGURIDAD_MANOS>zManoIz&&zManoIz>zManoDr-MEDIDA_SEGURIDAD_MANOS){
			retorno=true;
		}
		return retorno;
	}

	float distanciaEntreDosPuntos(Point3D a,Point3D b){
		//return Mathf.Sqrt(elevaCuadrado(b.X-a.X)+elevaCuadrado(b.Y-a.Y)+elevaCuadrado(b.Z-a.Z));
		//return Mathf.Sqrt(elevaCuadrado(b.X-a.X)+elevaCuadrado(b.Y-a.Y));
		return Mathf.Sqrt(elevaCuadrado(b.X-a.X));
	}

	float elevaCuadrado(float numero){
		return (float)Math.Pow(numero,2);
	}	
	
}
