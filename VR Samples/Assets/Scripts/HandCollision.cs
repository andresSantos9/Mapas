using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using Leap.Unity;

public class HandCollision : MonoBehaviour {

	//private List<string> objColliding = new List<string> ();
	private List<ObjInfo> objColliding = new List<ObjInfo> ();
	// TZ
	public GameObject map;
	private MapController controller;
	//
	public Transform gmap;
	//original private int moveCounter = 0, maxWaitTicks = 100;
	public  int maxWaitTicks = 25;
	private int moveCounter = 0;

	//private Material[] mats;


	TextMesh textm;

	class ObjInfo {
		public string name;
		public GameObject obj;
		public ObjInfo(string name, GameObject obj){
			this.name = name;
			this.obj = obj;
		}
	}


	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Renderer>().material.shader = Shader.Find( "Transparent/Diffuse" );
		Color color = Color.white;
		color.a = 0.5f;
		gameObject.GetComponent<Renderer>().material.color = color;

		controller = map.GetComponent<MapController>();
	}

	//retuns 0 (closed), 1 (extended), 2 (something in between)
	int handExtended(GameObject pobj){
		// averiguams si es un dedo (en cuyo caso hay que obtener la mano) o la mano en si
		GameObject mano = null;
		Hand       hand = null;
		int extendedFingers = 0;
		int gesture = -1;

		Debug.LogError (pobj.name);

		if (pobj.transform.parent && pobj.transform.parent.GetComponent<RigidHand> ()) {
			// estamos en palm o forearm
			mano = pobj.transform.parent.gameObject;
		} else if (pobj.transform.parent.parent && pobj.transform.parent.parent.GetComponent<RigidHand> ()) {
			// estamos en un bone de dedo	
			mano = pobj.transform.parent.parent.gameObject;			
		} else {
			// esto no debería de darse
			Debug.LogError ("Error en handClosed: colision con parte de mano no identificada.");
		}

		if (mano != null){
			hand = ((HandModel)mano.GetComponent<RigidHand> ()).GetLeapHand();
			extendedFingers = 0;
			//List<Finger> extendedFigerList = hand.Fingers;
			for (int f = 0; f < hand.Fingers.Count; f++) {
				Finger digit = hand.Fingers [f];
				if (digit.IsExtended)
					extendedFingers++;
			}
		}

		if (extendedFingers == 5) {
			gesture = 1;
		} else if (extendedFingers == 0) { 
			gesture = 0;
		} else {
			gesture = 2;
		}

		return gesture;
	}


	// Update is called once per frame
	void Update () {
		GameObject obj;
		int handGesture = -1;
		// comprobamos si las manos están activas, si no vaciamos la lista y apagamos para que se quede pillada
		foreach (ObjInfo objInfo in objColliding) {
			if (!objInfo.obj.activeInHierarchy) {
				//no lo encuentra por lo que no está activo
				objColliding.Remove (objInfo);
			}
		}
		
		if(objColliding.Count != 0){
			//handGesture = handExtended (objColliding [0].obj);
			if (moveCounter == 0) {
				switch (gameObject.name) {
				case "Up":
				case "UpMore":
					//gmap.GetComponent<GoogleMap> ().MoveUp ();
					//if (handGesture == 1)
						//controller.MoveUp (GlobalVars.incMovHands);
						controller.MoveUp ();
						break;
				case "Down":
				case "DownMore":
					//gmap.GetComponent<GoogleMap> ().MoveDown ();
					//if (handGesture == 1)
						//controller.MoveDown (GlobalVars.incMovHands);
						controller.MoveDown ();
					break;
				case "Left":
				case "LeftMore":
					//gmap.GetComponent<GoogleMap> ().MoveLeft ();
					//if (handGesture == 1)
						//controller.MoveLeft (GlobalVars.incMovHands);
						controller.MoveLeft ();
					break;
				case "Right":
				case "RightMore":					
					//gmap.GetComponent<GoogleMap> ().MoveRight ();
					//if (handGesture == 1)
						//controller.MoveRight (GlobalVars.incMovHands);
						controller.MoveRight ();
					break;
				case "Zoom In":
					//gmap.GetComponent<GoogleMap> ().ZoomIn ();
					//if (handGesture == 0)
						controller.ZoomIn ();
					break;
				case "Zoom Out":
					//gmap.GetComponent<GoogleMap> ().ZoomOut ();
					//if (handGesture == 0)
						controller.ZoomOut ();
					break;
				case "AddMarker":
					//if (handGesture == 0)
						controller.AddMarker();
					break;
				case "RemoveMarker":
					//if (handGesture == 0)
					controller.RemoveMarker();
					break;
				}
			}
			moveCounter = (moveCounter + 1) % maxWaitTicks;
		}else
			moveCounter = 0;
	}

	private string isHand(Collider other)
	{
		string parent1 = "";
		string parent2 = "";
		GameObject mano = null;
		bool rdo = false;

		if (other.transform.parent && other.transform.parent.GetComponent<RigidHand> ()) {
			return "parte"; //mano o forearm
		} else if (other.transform.parent.parent && other.transform.parent.parent.GetComponent<RigidHand> ()) {
			return "dedo";
		}
		return "";

		/*
		if (other.transform.parent && other.transform.parent.parent && other.transform.parent.parent.GetComponent<RigidHand>())
			//return other.transform.parent.parent.GetComponent<HandModel>();
			return other.transform.parent.parent.GetComponent<RigidHand>();
		else
			return null;
		*/
	}

	void OnEnable(){
		objColliding.Clear();
	}

	void OnTriggerEnter(Collider other) {

		//HandModel hand_model = GetHand(other);
		ObjInfo objInfo;

		string cacho = isHand(other);
		string sName = "";
		if (cacho != ""){

			if (cacho == "dedo") {
				sName = other.transform.parent.parent.name + "/" +other.transform.parent.name + "/" + other.name;
			} else {
				sName = other.transform.parent.name + "/" + other.name;
			}

			if (objColliding.Count == 0) {
				Color color = Color.green;
				color.a = 0.2f;
				gameObject.GetComponent<Renderer>().material.color = color;
			}

			bool found = false;
			foreach (ObjInfo objInfoAux in objColliding) {
				if (objInfoAux.name == sName){
					found = true;
				}
			}

			if (!found){
				objInfo = new ObjInfo (sName, other.gameObject);
				objColliding.Add (objInfo);
			}

		}

	}

	void OnTriggerExit(Collider other) {

		string cacho = isHand(other);
		string sName = "";
		if (cacho != "") {

			if (cacho == "dedo") {
				sName = other.transform.parent.parent.name + '/' + other.transform.parent.name  +'/' +  other.name;
			} else {
				sName = other.transform.parent.name +  '/' + other.name;
			}

			foreach (ObjInfo objInfo in objColliding) {
				if (objInfo.name == sName){
					objColliding.Remove (objInfo);
					break;
				}
			}

			if (objColliding.Count == 0) {
				Color color = Color.white;
				color.a = 0.5f;
				gameObject.GetComponent<Renderer>().material.color = color;
			}
		}
	}

}
