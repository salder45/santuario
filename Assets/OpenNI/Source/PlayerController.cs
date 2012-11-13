using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using OpenNI;

//path
using System.IO;

public class PlayerController : MonoBehaviour {
	//path del archivo .// es el raiz del proyecto
	private readonly string XML_CONFIG=@".//Assets/OpenNI/OpenNI.xml";
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private UserGenerator userGenerator;
	private SkeletonCapability skeletonCapability;
	private PoseDetectionCapability poseDetectionCapability;
	private string calibPose;
	private float valorRotation=1f;
	private bool shouldRun;
	//constante del angulo de rotacion
	private const float ANGULO_ROTACION_EN_X=0.30f;
	private const float ANGULO_ROTACION_EN_Y=0.20f;
	private const float ANGULO_BLOQUEAR_EN_Y=0.30f;
	
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
					Quaternion q=SkeletonJointOrientationToQuaternion(ori);
					RotaEnX(q.y);
					RotaEnY(q.x);
				}
			}
		}
	}
	
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		context.Release();
	}
	
	//handlers
	void userGenerator_NewUser(object sender, NewUserEventArgs e){
          if (this.skeletonCapability.DoesNeedPoseForCalibration){
            	this.poseDetectionCapability.StartPoseDetection(this.calibPose, e.ID);
           }else{
            	this.skeletonCapability.RequestCalibration(e.ID, true);
            }
    }
	/*
	void userGenerator_LostUser(object sender, UserLostEventArgs e){
		//this.joints.Remove(e.ID);
	}*/
	
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
	//rota en x en base al y del cuaternion
	void RotaEnX(float y){
		if(y>ANGULO_ROTACION_EN_X){
			transform.Rotate(new Vector3(0f,valorRotation,0f));
		}else if(y<-ANGULO_ROTACION_EN_X){
			transform.Rotate(new Vector3(0f,-valorRotation,0f));
		}
	}
	
	//rota en y en base al x del cuaternion
	void RotaEnY(float x){
		GameObject camara=GameObject.Find("CameraPlayer");
		float rotacionCamaraX=camara.transform.rotation.x;
		Debug.Log((x>-ANGULO_ROTACION_EN_Y));
		if(x>-ANGULO_ROTACION_EN_Y&x<ANGULO_ROTACION_EN_Y){
			camara.transform.rotation=new Quaternion(0f,0f,0f,0f);	
		}else //esta parte del codigo hace la rotacion
		if(x<-(ANGULO_ROTACION_EN_Y-0.1f)&rotacionCamaraX<ANGULO_BLOQUEAR_EN_Y){
			camara.transform.Rotate(new Vector3(valorRotation,0f,0f));
		}else if(x>ANGULO_ROTACION_EN_Y&rotacionCamaraX>-ANGULO_BLOQUEAR_EN_Y){
			camara.transform.Rotate(new Vector3(-valorRotation,0f,0f));
		}
		
		
		
		
		
		/*
		if(q.x>.20){
			if(GameObject.Find("CuboCamara").transform.rotation.x<.25){
				GameObject.Find("CuboCamara").transform.Rotate(new Vector3(valorPositivoRotacion,0f,0f));
			}
		}else if(q.x<-.20){
			if(GameObject.Find("CuboCamara").transform.rotation.x>-.25){
				GameObject.Find("CuboCamara").transform.Rotate(new Vector3(-valorNegativoRotacion,0f,0f));
			}
		}else{
			Quaternion fromX =new Quaternion(GameObject.Find("CuboCamara").transform.rotation.x,0f,0f,GameObject.Find("CuboCamara").transform.rotation.w);
			Quaternion toX =new Quaternion(0f,GameObject.Find("CuboCamara").transform.rotation.y,0f,transform.rotation.w);
			//GameObject.Find("CuboCamara").transform.Rotate(Quaternion.Lerp(fromX, toX, Time.time * .001f));
			//GameObject.Find("CuboCamara").transform.Rotate(Vector3((GameObject.Find("CuboCamara").rotation.x *-1)*2,0f,0f));				
		}
		*/
		
	}
}
