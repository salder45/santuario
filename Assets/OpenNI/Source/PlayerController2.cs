using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using OpenNI;

//path
using System.IO;
public class PlayerController2 : MonoBehaviour {
	//path del archivo .// es el raiz del proyecto
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private UserGenerator userGenerator;
	private SkeletonCapability skeletonCapability;
	private PoseDetectionCapability poseDetectionCapability;
	private string calibPose;
	private float valorRotation=1f;
	private bool shouldRun;
	private int contador=0;
	//constante del angulo de rotacion
	private const float ANGULO_ROTACION_EN_X=0.30f;
	private const float ANGULO_ROTACION_EN_Y=0.20f;
	
	private const float CONSTANTE_ROTACION_VERTICAL=25f;
	private const float ANGULO_FRONTERA_Y_REGRESAR=0.05f;
	private const float DISTANCIA_MOVER=200f;
	
	
	//punto de refrencia para avanzar/retroceder
	private Point3D puntoInicial;

	// Use this for initialization
	void Start () {
		Debug.Log("Start");
		this.context=Context.CreateFromXmlFile(XML_CONFIG,out scriptNode);
		this.depth=context.FindExistingNode(NodeType.Depth) as DepthGenerator;
		if(depth==null){
			throw new Exception("Nodo de Profundidad no encontrado");
		}
		this.userGenerator=new UserGenerator(this.context);
		this.skeletonCapability=this.userGenerator.SkeletonCapability;
		this.poseDetectionCapability=this.userGenerator.PoseDetectionCapability;
		this.calibPose=this.skeletonCapability.CalibrationPose;
		
		this.userGenerator.NewUser+=userGenerator_NewUser;
		//this.userGenerator.LostUser+=userGenerator_LostUser;
		this.poseDetectionCapability.PoseDetected+=poseDetectionCapability_PoseDetected;
		this.skeletonCapability.CalibrationComplete+=skeletonCapability_CalibrationComplete;
		
		this.skeletonCapability.SetSkeletonProfile(SkeletonProfile.All);
				
		this.userGenerator.StartGenerating();
		this.shouldRun=true;
	
	}
	
	// Update is called once per frame
	void Update () {
		
		//Debug.Log("Update");
		if(this.shouldRun){
			try{
				this.context.WaitOneUpdateAll(this.depth);
			}catch(Exception){
				Debug.Log("No paso");
			}			
			
			int[] users=this.userGenerator.GetUsers();
			foreach(int user in users){
				if(this.skeletonCapability.IsTracking(user)){
					//Debug.Log("Trackeando");
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
	}
	
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		//context.Release();
	}
	
	//handlers
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
	
	public static Quaternion SkeletonJointOrientationToQuaternion(SkeletonJointOrientation m) {float tr = m.X1 + m.Y2 + m.Z3;

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

	void Mover(Point3D puntoActual){
		//Avanzar
		if(puntoActual.Z<puntoInicial.Z-DISTANCIA_MOVER){
			transform.Translate(Vector3.forward*Time.deltaTime*3f);
		}else //Retroceder 
		if(puntoActual.Z>puntoInicial.Z+DISTANCIA_MOVER){
			transform.Translate(Vector3.back*Time.deltaTime*3f);
		}
	}
}
