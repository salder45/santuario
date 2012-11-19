using UnityEngine;
using System.Collections;
using System;
using OpenNI;
using NITE;

[RequireComponent (typeof (CharacterController))]
public class PlayerController : MonoBehaviour {
	//variables Kinect
	//path del archivo .// es el raiz del proyecto es mejor ponerlo en el raiz por el asunto de cuando se hace ejecutable
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private UserGenerator userGenerator;
	private SkeletonCapability skeletonCapability;
	private PoseDetectionCapability poseDetectionCapability;
	private string calibPose;
	private Point3D puntoInicial;
	private bool shouldRun;
	//variables Unity CharacterController
	public float gravedad = 20.0f;
	private Vector3 moveDirection = Vector3.zero;
	private bool grounded = false;
	private float fallStartLevel;
    private bool falling;
	private CharacterController controller;
	private Transform miTransform;
	//valores del algoritmo
	private float valorRotation=1f;
	private int contador=0;
	private const float ANGULO_ROTACION_EN_X=0.30f;
	private const float ANGULO_ROTACION_EN_Y=0.20f;	
	private const float CONSTANTE_ROTACION_VERTICAL=25f;
	private const float ANGULO_FRONTERA_Y_REGRESAR=0.05f;
	private const float DISTANCIA_MOVER=200f;
	
	
	// Use this for initialization
	void Start () {
		Debug.Log("Start");
		//Init Kinect
		this.context=Context.CreateFromXmlFile(XML_CONFIG,out scriptNode);
		this.depth=context.FindExistingNode(NodeType.Depth) as DepthGenerator;
		if(depth==null){
			throw new Exception("Nodo de Profundidad no encontrado");
		}
		this.userGenerator=new UserGenerator(this.context);
		this.skeletonCapability=this.userGenerator.SkeletonCapability;
		this.poseDetectionCapability=this.userGenerator.PoseDetectionCapability;
		this.calibPose=this.skeletonCapability.CalibrationPose;
		
		//Init CharacterController
		controller=GetComponent<CharacterController>();
		miTransform=transform;
		//asignando handlers
		this.userGenerator.NewUser+=userGenerator_NewUser;
		this.userGenerator.LostUser+=userGenerator_LostUser;
		this.poseDetectionCapability.PoseDetected+=poseDetectionCapability_PoseDetected;
		this.skeletonCapability.CalibrationComplete+=skeletonCapability_CalibrationComplete;		
		this.skeletonCapability.SetSkeletonProfile(SkeletonProfile.All);
		//start generador
		this.userGenerator.StartGenerating();
		this.shouldRun=true;	
	}
	
	// Update is called once per frame
	void Update () {
		//solo se usa para ejecutar el GetButtonDown solo jala en el update no en el FixedUpdate
	}
	//Usado por el CharacterController
	void FixedUpdate() {
		//Debug.Log("FixedUpdate");
		if(controller.isGrounded){
			if(this.shouldRun){
				try{
					this.context.WaitOneUpdateAll(this.depth);
				}catch(Exception){
					Debug.Log("No paso");
				}
				int[] users=this.userGenerator.GetUsers();
				foreach(int user in users){
					if(this.skeletonCapability.IsTracking(user)){
						//Debug.Log("Track");
						SkeletonJointOrientation ori=this.skeletonCapability.GetSkeletonJointOrientation(user,SkeletonJoint.Torso);
						SkeletonJointPosition posicion=this.skeletonCapability.GetSkeletonJointPosition(user,SkeletonJoint.Torso);
						Quaternion q=SkeletonJointOrientationToQuaternion(ori);
						Rotar(q);
						if(contador==0){
							this.puntoInicial=posicion.Position;
						}else{
							Mover(posicion.Position);
						}
						contador=contador+1;
					}
				}
			}
		}else {
			if (!falling) {
                falling = true;
                fallStartLevel = miTransform.position.y;
            }			
		}
		
		//aplicando gravedad
		moveDirection.y -= gravedad * Time.deltaTime;
		
		//mueve el controlador y guarda si esta en el piso o no
		//grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
		controller.SimpleMove(moveDirection);
		
	}
	//apagar el sensor
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		context.Release();
	}
	
	//handlers de openni
	void userGenerator_NewUser(object sender, NewUserEventArgs e){
          if (this.skeletonCapability.DoesNeedPoseForCalibration){
            	this.poseDetectionCapability.StartPoseDetection(this.calibPose, e.ID);
           }else{
            	this.skeletonCapability.RequestCalibration(e.ID, true);
            }
    }
	
	void userGenerator_LostUser(object sender, UserLostEventArgs e){
		this.contador=0;
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
	
	public static Quaternion SkeletonJointOrientationToQuaternion(SkeletonJointOrientation m) {
		float tr = m.X1 + m.Y2 + m.Z3;
        float S = 0f;
		float qw = 0f;
		float qx = 0f;
		float qy = 0f;
		float qz = 0f;

        if(tr > 0) {
			S = Mathf.Sqrt(tr + 1.0f) * 2f;
			qw = 0.25f * S;
			qx = (m.Y3 - m.Z2) / S;
			qy = (m.Z1 - m.X3) / S;
			qz = (m.X2 - m.Y1) / S;

        } else if((m.X1 > m.Y2) && (m.X1 > m.Z3)) {
			S = Mathf.Sqrt(1.0f + m.X1 - m.Y2 - m.Z3) * 2f;
			qw = (m.Y3 - m.Z2) / S;
			qx = 0.25f * S;
			qy = (m.Y1 + m.X2) / S;
			qz = (m.Z1 + m.X3) / S;

        } else if(m.Y2 > m.Z3) {
			S = Mathf.Sqrt(1.0f + m.Y2 - m.X1 - m.Z3) * 2f;
			qw = (m.Z1 - m.X3) / S;
			qx = (m.Y1 + m.X2) / S;
			qy = 0.25f * S;
			qz = (m.Z2 + m.Y3) / S;

        } else {
			S = Mathf.Sqrt(1.0f + m.Z3 - m.X1 - m.Y2) * 2f;
			qw = (m.X2 - m.Y1) / S;
			qx = (m.Z1 + m.X3) / S;
			qy = (m.Z2 + m.Y3) / S;
			qz = 0.25f * S;
		}
		return new Quaternion(qx, qy, qz, qw);

    }
	
	//algoritmo
	void Mover(Point3D puntoActual){
		//Debug.Log("Mover");
		Debug.Log("****X "+moveDirection.x+" Y "+moveDirection.y+" Z "+moveDirection.z);
		//Avanzar
		if(puntoActual.Z<puntoInicial.Z-DISTANCIA_MOVER){
			Debug.Log("Avanzar");
			moveDirection=miTransform.TransformDirection(Vector3.forward)*10f;
		}else //Retroceder 
		if(puntoActual.Z>puntoInicial.Z+DISTANCIA_MOVER){
			Debug.Log("Retroceder");
			moveDirection=miTransform.TransformDirection(Vector3.back)*10f;
		}else{
			Debug.Log("Estar");
			moveDirection=Vector3.zero;
		}
		
		Debug.Log("X "+moveDirection.x+" Y "+moveDirection.y+" Z "+moveDirection.z);
	}
	
	//Rotacion del jugador y de la camara
	void Rotar(Quaternion rotacionKinect){
		GameObject camara=GameObject.Find("CuboCamara");
		float xPlayer=0f;
		float yPlayer=0f;
		float zPlayer=0f;
		float xCamara=camara.transform.localEulerAngles.x;
		float yCamara=0f;
		float zCamara=0f;
		bool isRotacionHorizontal=false;
		
		if(rotacionKinect.y>ANGULO_ROTACION_EN_X){
			isRotacionHorizontal=true;
			yPlayer=valorRotation;
		}else if(rotacionKinect.y<-ANGULO_ROTACION_EN_X){
			isRotacionHorizontal=true;
			yPlayer=-valorRotation;
		}
		/*
		if(rotacionKinect.x<-(ANGULO_ROTACION_EN_Y-0.1f)){
			xCamara=(xCamara+valorRotation*CONSTANTE_ROTACION_VERTICAL)/2f;
		}else if(rotacionKinect.x>ANGULO_ROTACION_EN_Y){
			if(xCamara==0|xCamara>335f){
				xCamara=xCamara-valorRotation;
			}
		}else if(rotacionKinect.x<ANGULO_ROTACION_EN_Y&&rotacionKinect.x>-(ANGULO_ROTACION_EN_Y-0.1f)){
			if(xCamara<26f&&xCamara>1){
				xCamara=xCamara-valorRotation;
			}else if(xCamara>334f&xCamara<360f){
				xCamara=xCamara+valorRotation;
			}else{
				xCamara=0f;
			}
		}
		*/
		if(isRotacionHorizontal){
			transform.Rotate(new Vector3(xPlayer,yPlayer,zPlayer));
		}else{
			camara.transform.localEulerAngles=new Vector3(xCamara,yCamara,zCamara);
		}
	}
}
