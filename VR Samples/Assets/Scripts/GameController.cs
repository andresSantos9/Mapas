// -----------------------------------------------------------------------------
// GameController.cs:
// -----------------------------------------------------------------------------
//
// Este script gestiona los cambios en el mapa que se desencadenan con la
// entrada del usuario, ya sea por medio del Xbox One Controller o por medio
// del teclado. Tambien incluye un control adicional util solo cuando se esta
// desarrollando el videojuego en el editor Unity.
//
// Autor:
//		Alvaro Caceres Mu~noz (100303602@alumnos.uc3m.es)
// Fecha:
// 		Septiembre de 2016
// -----------------------------------------------------------------------------

namespace VRTK.Examples
{
using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	public GameObject map;
	public GameObject leftController;
	public GameObject rightController;

	//public Transform gmap, buttons;
	public Transform buttons;
	public bool inverted = true;

	private MapController controller;
	private VRTK_SimplePointer leftPointer;
	private VRTK_SimplePointer rightPointer;
	private VRTK_ControllerPointerEvents_ListenerExample  leftListener;
	private VRTK_ControllerPointerEvents_ListenerExample  rightListener;

	// Estas variables booleanas determinan si un determinado control del Xbox
	// One Wireless Controller ha sido pulsado o no.
	private bool zoomInIsPressed = false;
	private bool zoomOutIsPressed = false;
	private bool dragIsPressed = false;
	private bool dragIsHeld = false;
	private bool dragIsReleased = false;
	private bool addMarkerIsPressed = false; //TZ
	private bool rmvMarkerIsPressed = false; //TZ
	private bool addMarkerTouchRIsPressed = false; //TZ
	private bool rmvMarkerTouchRIsPressed = false; //TZ	
	private bool addMarkerTouchLIsPressed = false; //TZ
	private bool rmvMarkerTouchLIsPressed = false; //TZ	
	private bool moveUpIsPressed = false;
	private bool moveDownIsPressed = false;
	private bool moveRightIsPressed = false;
	private bool moveLeftIsPressed = false;
	private bool mouseIsPressed = false;


	// Estas variables definen el numero de ticks (equivalente al tiempo) que
	// lleva pulsado un determinado control del Xbox One Controller. Es
	// recomendable comprobar el uso que se hace de estos contadores en el
	// metodo Update() de esta misma clase, antes de continuar leyendo este
	// comentario. El motivo por el cual he inicializado los contadores a 1 es
	// que, si los hubiera inicializado a 0, el codigo que aparece en Update()
	// entenderia que se ha pulsado algun boton nada mas pulsar el boton de Play
	// en el editor de Unity, lo cual no es cierto. He utilizado 1 en vez de
	// cualquier otro valor entre 0 y maxWaitTicks porque, la primera vez que se
	// pulse cada boton al iniciar el nivel, se tendra que esperar casi tantos
	// ticks como maxWaitTicks, sin que la diferencia sea perceptible.
	private int zoomInCounter = 1;
	private int zoomOutCounter = 1;
	private int moveUpCounter = 1;
	private int moveDownCounter = 1;
	private int moveLeftCounter = 1;
	private int moveRightCounter = 1;

	// Esta variable define el numero de ticks (equivalente al tiempo) que debe
	// mantenerse pulsado un control del Xbox One Controller hasta que se
	// registre una nueva pulsacion de dicho control. He definido dicho valor
	// como 100 porque se traducia a esperar suficiente poco tiempo como para
	// que merezca la pena mantener pulsado el boton (con respecto a soltar el
	// boton y volverlo a pulsar varias veces), y suficiente tiempo como para
	// que las funciones de zoom y desplazamiento no se activaran demasiadas
	// veces por unidad de tiempo.
	// TZ: Desglosado en maxWaitTicksDesp y Zoom porque la precisión del mando
	// para el zoom necesita ser muy alta y para los desplazamientos más baja
	private int maxWaitTicksDesp = 25;
	private int maxWaitTicksZoom = 100;
	private int maxWaitDragTicks = 1;

	// Esta variables definen el valor de inclinacion (horizontal y vertical,
	// respectivamente) en el tick actual del joystick derecho en el Xbox One
	// Wireless Controller.
	private float currentRightJoystickX, currentRightJoystickY;

	// Esta variable define hasta que punto hay que presionar el joystick
	// izquierdo del Xbox One Wireless Controller para que se registre el
	// movimiento en una determinada direccion de dicho joystick. He definido
	// dicho punto cercano a 1 de modo que haya que inclinar bastante el
	// joystick para registrar el movimiento, aunque no sea necesario inclinarlo
	// del todo (en cuyo caso habria que apretar el joystick bastante, lo que
	// resulta incomodo).
	private float rightJoystickActivationThreshold = 0.8f;

	// Esta variable define hasta que punto hay que presionar el trigger del
	// Xbox One Wireless Controller para que se registre la pulsacion de dicho
	// trigger. He definido dicho punto como 1 de modo que haya que pulsar el
	// trigger hasta el fondo. Me parecio que recuerda asi al funcionamiento de
	// los botones R y L del mando de Play Station 2.
	private float leftTriggerActivationThreshold = -1.0f;
	private float rightTriggerActivationThreshold = 1.0f;

	private Vector3 cameraPosition, lastCameraPosition, difference, mousePosition, lastMousePosition, differenceMouse;
	
	void Start(){
		cameraPosition = new Vector3(0,0,0);
		mousePosition = new Vector3(0,0,0);

		controller = map.GetComponent<MapController>();

		if (leftController != null) {
			leftPointer  = leftController.GetComponent<VRTK_SimplePointer> ();
			leftListener = leftController.GetComponent<VRTK_ControllerPointerEvents_ListenerExample> ();
		}
		if (rightController != null) {
			rightPointer  = rightController.GetComponent<VRTK_SimplePointer> ();
			rightListener = rightController.GetComponent<VRTK_ControllerPointerEvents_ListenerExample> ();
		}

		// Miramos a ver si hay que desactivar manos o botones virtuales
		GameObject go;
		if (!GlobalVars.handsActive) {
			go = GameObject.Find ("Debugger");
			if (go != null) {
				go.GetComponent<HandGrabber> ().enabled = false;
			}			
			go = GameObject.Find ("LeapHandController");
			if (go != null) {
				go.SetActive (false);
			}
		} else if (!GlobalVars.handDragActive) {
			go = GameObject.Find ("Debugger");
			if (go != null) {
				go.GetComponent<HandGrabber> ().enabled = false;
			}			
		}

		if (!GlobalVars.navButtonsActive) {
			go = GameObject.Find ("Left");
			if (go != null) {
				go.SetActive (false);
			}
			go = GameObject.Find ("LeftMore");
			if (go != null) {
				go.SetActive (false);
			}
			go = GameObject.Find ("Right");
			if (go != null) {
				go.SetActive (false);
			}
			go = GameObject.Find ("RightMore");
			if (go != null) {
				go.SetActive (false);
			}
			go = GameObject.Find ("Up");
			if (go != null) {
				go.SetActive (false);
			}
			go = GameObject.Find ("UpMore");
			if (go != null) {
				go.SetActive (false);
			}
			go = GameObject.Find ("Down");
			if (go != null) {
				go.SetActive (false);
			}
			go = GameObject.Find ("DownMore");
			if (go != null) {
				go.SetActive (false);
			}
		}

		if (!GlobalVars.zoomButtonsActive) {
			go = GameObject.Find ("Zoom In");
			if (go != null) {
				go.SetActive (false);
			}
			go = GameObject.Find ("Zoom Out");
			if (go != null) {
				go.SetActive (false);
			}
		}
	}


	void Update ()
	{

		zoomInIsPressed    = false;
		zoomOutIsPressed   = false;
		addMarkerIsPressed = false;
		rmvMarkerIsPressed = false;
		addMarkerTouchRIsPressed = false;
		rmvMarkerTouchRIsPressed = false;	
		addMarkerTouchLIsPressed = false;
		rmvMarkerTouchLIsPressed = false;	

		if (GlobalVars.mouseActive) {

			if (Input.mouseScrollDelta.y > 0)
				for (int i = 0; i < Input.mouseScrollDelta.y; i++)
				//gmap.GetComponent<GoogleMap> ().ZoomIn ();
					controller.ZoomIn ();
			else if (Input.mouseScrollDelta.y < 0)
				for (int i = 0; i > Input.mouseScrollDelta.y; i--)
				//gmap.GetComponent<GoogleMap> ().ZoomOut ();
					controller.ZoomOut ();

			/**** QUITADO EL DRAG AND DROP DESDE RATON
			 * 
			mouseIsPressed = Input.GetMouseButtonDown (0) ? true : mouseIsPressed;
			mouseIsPressed = Input.GetMouseButtonUp (0) ? false : mouseIsPressed;
			if (Input.GetMouseButtonDown (0)) {
				mousePosition = Input.mousePosition;
				buttons.gameObject.active = false;
			} else if (Input.GetMouseButtonUp (0)) {
				gmap.GetComponent<GoogleMap> ().DragAngle (gmap.transform.rotation.eulerAngles - new Vector3 (0, 0, 180), gmap.transform.position - new Vector3 (0, 4, 5.503f));
				buttons.gameObject.active = true;
				foreach (Transform button in buttons) {
					Color color = Color.white;
					color.a = 0.2f;
					button.GetComponent<Renderer> ().material.color = color;
				}
			}

			if (Input.mouseScrollDelta.y > 0)
				for (int i = 0; i < Input.mouseScrollDelta.y; i++)
					//gmap.GetComponent<GoogleMap> ().ZoomIn ();
					controller.ZoomIn ();
			else if (Input.mouseScrollDelta.y < 0)
				for (int i = 0; i > Input.mouseScrollDelta.y; i--)
					//gmap.GetComponent<GoogleMap> ().ZoomOut ();
					controller.ZoomOut ();
			*******/
		}
						
		if ((GlobalVars.gamePadActive) || (GlobalVars.touchActive) || (GlobalVars.riftRemoteActive)) {
						
				if ((GlobalVars.gamePadActive) || (GlobalVars.touchActive)) {
						
					if (GlobalVars.touchActive) {

						if (GlobalVars.touchActiveL) {
							currentRightJoystickX = OVRInput.Get (OVRInput.Axis2D.PrimaryThumbstick) [0];  
							currentRightJoystickY = (-1) * OVRInput.Get (OVRInput.Axis2D.PrimaryThumbstick) [1];

							//if (OVRInput.GetDown(OVRInput.Axis1D.PrimaryIndexTrigger) >= 0.8f){
							if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)){
								if ((leftPointer != null) && (leftPointer.IsActive()))
									addMarkerTouchLIsPressed = true;
								else
									zoomInIsPressed = true;											
										
							}

							//if (OVRInput.GetDown (OVRInput.Axis1D.PrimaryHandTrigger) >= 0.8f){
							if (OVRInput.GetDown (OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)){										
								if ((leftPointer != null) && (leftPointer.IsActive()))
									rmvMarkerTouchRIsPressed = true;
								else
									zoomOutIsPressed = true;
							}

						}

						if (GlobalVars.touchActiveR) {
							if ((currentRightJoystickX >= -rightJoystickActivationThreshold) && (currentRightJoystickX <= rightJoystickActivationThreshold)) {
								currentRightJoystickX = OVRInput.Get (OVRInput.Axis2D.SecondaryThumbstick) [0];  
							}
							if ((currentRightJoystickY >= -rightJoystickActivationThreshold) && (currentRightJoystickY <= rightJoystickActivationThreshold)) {
								currentRightJoystickY = (-1) * OVRInput.Get (OVRInput.Axis2D.SecondaryThumbstick) [1];
							}

							if (OVRInput.GetDown (OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)){ // >= 0.8f){
								if ((rightPointer != null) && (rightPointer.IsActive ())) {
									addMarkerTouchRIsPressed = true;
								} else {
									zoomInIsPressed = true;			
								}
							}

						if (OVRInput.GetDown (OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)){ // >= 0.8f){
								if ((rightPointer != null) && (rightPointer.IsActive()))
									rmvMarkerTouchRIsPressed = true;
								else
									zoomOutIsPressed = true;			
							}
						}

					}

					if (GlobalVars.gamePadActive) {
						// La idea es usar los triggers y el joystick derecho del Xbox One
						// Wireless Controller como si fueran botones, a pesar de que estos dos
						// componentes del mando lancen valores continuos. De esta forma, se
						// emula la interaccion con un mando de Play Station 2: pulsar los
						// triggers derecho e izquierdo seria analogo a pulsar los botones R1 y
						// L1 en el mando de PS2; y mover el joystick derecho arriba / abajo /
						// izquierda / derecha seria analogo a pulsar las flechas de direccion.

												 	
						//TZ Para que use el joystick de la izquierda o de la derecha
						if ((currentRightJoystickX >= -rightJoystickActivationThreshold) && (currentRightJoystickX <= rightJoystickActivationThreshold)) {
							currentRightJoystickX = Input.GetAxisRaw ("RightJoystickX");
						}

						if ((currentRightJoystickY >= -rightJoystickActivationThreshold) && (currentRightJoystickY <= rightJoystickActivationThreshold)) {
							currentRightJoystickY = Input.GetAxisRaw ("RightJoystickY");
						}

						if ((currentRightJoystickY >= -rightJoystickActivationThreshold) && (currentRightJoystickY <= rightJoystickActivationThreshold)) {
							currentRightJoystickY = (-1) * Input.GetAxisRaw ("Vertical");
						}

						if ((currentRightJoystickX >= -rightJoystickActivationThreshold) && (currentRightJoystickX <= rightJoystickActivationThreshold)) {
							currentRightJoystickX = Input.GetAxisRaw ("Horizontal");
						}
						//

						/**
						// TZ
						CAMBIADO. Ahora hacemos el drag del mando en el script
						ChangePositionByCenterPoint

						//
						//if (Input.GetButtonDown (GlobalVars.gpDrag))
						if (OVRInput.GetDown (OVRInput.Button.Two))  // "B"
							dragIsPressed = true;
						//if (Input.GetButtonDown (GlobalVars.gpDrag))
						if (OVRInput.GetDown (OVRInput.Button.Two))  // "B"
							dragIsHeld = true;
						//if (Input.GetButtonDown (GlobalVars.gpDrag))
						if (OVRInput.GetDown (OVRInput.Button.Two))  // "B"
							dragIsReleased = true;

						***/
						//if (Input.GetButtonDown (GlobalVars.gpAddMarker))
						if (OVRInput.GetDown (OVRInput.Button.One))  // "A"
							addMarkerIsPressed = true;
						//if (Input.GetButtonDown (GlobalVars.gpDeleteMarker))
						if (OVRInput.GetDown (OVRInput.Button.Three))  // "A"
							rmvMarkerIsPressed = true;				
						if (Input.GetAxisRaw (GlobalVars.gpZoomIn) == rightTriggerActivationThreshold)
							zoomInIsPressed = true;
						if (Input.GetAxisRaw (GlobalVars.gpZoomOut) == leftTriggerActivationThreshold)
							zoomOutIsPressed = true;
					}

					// Como clarificacion, al mover el joystick derecho hacia la derecha, se
					// obtiene un valor positivo de la componente X de dicho eje, y al
					// moverlo hacia la izquierda se obtiene un valor negativo. Al moverlo
					// hacia arriba, se obtiene un valor negativo de la componente Y de
					// dicho eje, y al moverlo hacia abajo se obtiene un valor positivo.
					moveUpIsPressed    = currentRightJoystickY < -rightJoystickActivationThreshold;
					moveDownIsPressed  = currentRightJoystickY > rightJoystickActivationThreshold;
					moveRightIsPressed = currentRightJoystickX > rightJoystickActivationThreshold;
					moveLeftIsPressed  = currentRightJoystickX < -rightJoystickActivationThreshold;
					
					currentRightJoystickX = 0;
					currentRightJoystickY = 0;
				}
					
				// Los botones + y - son reservados y no se pueden utilizar
				if (GlobalVars.riftRemoteActive) {
					if (OVRInput.Get (OVRInput.Button.DpadUp))
						moveUpIsPressed = true;
					if (OVRInput.Get (OVRInput.Button.DpadDown))
						moveDownIsPressed = true;
					if (OVRInput.Get (OVRInput.Button.DpadLeft))
						moveLeftIsPressed = true;
					if (OVRInput.Get (OVRInput.Button.DpadRight))
						moveRightIsPressed = true;
					if (OVRInput.Get (OVRInput.Button.One))
						zoomInIsPressed = true;
					if (OVRInput.Get (OVRInput.Button.Two))
						zoomOutIsPressed = true;
				}
			}

			// Una vez se convierten los datos continuos de los controles anteriores
			// se convierten a variables booleanas, podemos operar con ellos como si
			// fueran botones. No obstante, si solo se comprueba si el control esta
			// pulsado, Unity dispara los metodos de cambio de zoom y de movimiento
			// continuamente hasta que levantemos dicho control. Esto no solo hace
			// que el usuario cambie el zoom o la posicion del mapa de forma
			// abrupta, sino que ademas provoca tantas peticiones a los servidores
			// de Google que en pocos segundos el ordenador en el que se esta usando
			// el programa alcanza su cuota maxima de peticiones y no se reciben mas
			// fotogramas del mapa. Por eso, he creado un contador para cada "boton"
			// de modo que, si se ha pulsado un boton, solo se vuelva a registrar
			// pulsacion una vez han transcurrido varios segundos, o lo que es lo
			// mismo, una vez han transcurrido tantos ticks como maxWaitTicks.
			if (zoomInIsPressed) {
				if (zoomInCounter == 0)//{
					controller.ZoomIn();
					//gmap.GetComponent<GoogleMap> ().ZoomIn ();
					//Debug.Log (gmap.GetComponent<GoogleMap> ().getLatitude (255));
				//GameObject.Find("Map (2)").GetComponent<GoogleMap> ().ZoomIn ();
				//GameObject.Find("Map (4)").GetComponent<GoogleMap> ().ZoomIn ();
				//GameObject.Find("Map (5)").GetComponent<GoogleMap> ().ZoomIn ();
				//GameObject.Find("Map (7)").GetComponent<GoogleMap> ().ZoomIn ();}
				zoomInCounter = (zoomInCounter + 1) % maxWaitTicksZoom;
			}
			if (!zoomInIsPressed)
				zoomInCounter = 0;

			if (zoomOutIsPressed) {
				if (zoomOutCounter == 0)//{
					controller.ZoomOut();
					//gmap.GetComponent<GoogleMap> ().ZoomOut ();
				//GameObject.Find("Map (2)").GetComponent<GoogleMap> ().ZoomOut ();
				//GameObject.Find("Map (4)").GetComponent<GoogleMap> ().ZoomOut ();
				//GameObject.Find("Map (5)").GetComponent<GoogleMap> ().ZoomOut ();
				//GameObject.Find("Map (7)").GetComponent<GoogleMap> ().ZoomOut ();}
				zoomOutCounter = (zoomOutCounter + 1) % maxWaitTicksZoom;
			}
			if (!zoomOutIsPressed) {
				zoomOutCounter = 0;
			}

		/*if(dragIsPressed){
			cameraPosition = Camera.main.transform.eulerAngles;
			//for (int i = 1; i < 9; i++)
			//	GameObject.Find("Map (" + i + ")").GetComponent<GoogleMap> ().Surround (i);
			Debug.Log("Empieza");
		}

		if(dragIsHeld){
			lastCameraPosition = cameraPosition;
			cameraPosition = Camera.main.transform.eulerAngles;
			difference = cameraPosition - lastCameraPosition;
			difference.x %= 360;
			difference.y %= 360;
			if(difference.x > 180)
				difference.x -= 360;
			if(difference.y > 180)
				difference.y -= 360;
			difference.x /= 180;
			difference.y /= 180;
			difference.x = (difference.x > 1 || difference.x < -1) ? 0 : difference.x;
			difference.y = (difference.y > 1 || difference.y < -1) ? 0 : difference.y;
			difference.x = inverted ? -difference.x : difference.x;
			difference.y = inverted ? -difference.y : difference.y;
			Vector3 temp = gmap.transform.position;
			temp.x -= 10*difference.y;
			if (temp.x > 3)
				temp.x = 3;
			else if (temp.x < -3)
				temp.x = -3;
			temp.y += 10*difference.x;
			if (temp.y > 6)
				temp.y = 6;
			else if (temp.y < 4)
				temp.y = 4;
			gmap.transform.position = temp;
			//gmap.GetComponent<GoogleMap>().Drag(difference);
		}

		if(dragIsReleased){
			gmap.GetComponent<GoogleMap> ().Drag(gmap.transform.position - new Vector3(0f,5f,14.73f));
			//StartCoroutine(DragWaiter());
			Debug.Log(gmap.transform.position - new Vector3(0f,5f,14.73f));
			Debug.Log("Acaba");
		}*/

			/****** AQUI ******
		if(dragIsPressed){
			cameraPosition = Camera.main.transform.eulerAngles;
			buttons.gameObject.active = false;
		}
			***/

		/** OLD! De acuando utilizabamos gmap
		if(dragIsHeld){
			lastCameraPosition = cameraPosition;
			cameraPosition = Camera.main.transform.eulerAngles;
			difference = cameraPosition - lastCameraPosition;
			difference.x %= 360;
			difference.y %= 360;
			if(difference.x > 180)
				difference.x -= 360;
			if(difference.y > 180)
				difference.y -= 360;
			difference.x = inverted ? difference.x : -difference.x;
			difference.y = inverted ? -difference.y : difference.y;
			Vector3 tempPosition = gmap.transform.position;
			Vector3 tempRotation = gmap.transform.rotation.eulerAngles;
			tempPosition.y += 0.1f * difference.x;
			tempPosition.y = tempPosition.y > 5 ? 5 : tempPosition.y;
			tempPosition.y = tempPosition.y < 3 ? 3 : tempPosition.y;
			tempRotation.y += difference.y;
			tempRotation.y = tempRotation.y > 180 ? tempRotation.y - 360 : tempRotation.y;
			tempRotation.y = tempRotation.y < -180 ? tempRotation.y + 360 : tempRotation.y;
			gmap.transform.position = tempPosition;
			gmap.transform.rotation = Quaternion.Euler(tempRotation);
			//gmap.GetComponent<GoogleMap>().Drag(difference);
		}
		**/

		if(dragIsReleased){
		//TZ quitado por ahora	gmap.GetComponent<GoogleMap> ().DragAngle(gmap.transform.rotation.eulerAngles - new Vector3(0, 0, 180), gmap.transform.position - new Vector3(0, 4, 5.503f));
			buttons.gameObject.active = true;
			foreach (Transform button in buttons)
			{
				Color color = Color.white;
				color.a = 0.2f;
				button.GetComponent<Renderer>().material.color = color;
			}
		}

		if (addMarkerIsPressed) {
			controller.AddMarker ();
		}

		if (rmvMarkerIsPressed) {
			controller.RemoveMarker ();
		}
		
		if (addMarkerTouchRIsPressed) {
			rightListener.AddMarker ();
		}

		if (rmvMarkerTouchRIsPressed) {
			controller.RemoveMarker ();
		}

		if (addMarkerTouchLIsPressed) {
			leftListener.AddMarker ();
		}

		if (rmvMarkerTouchLIsPressed) {
			controller.RemoveMarker ();
		}

		/** OLD! De cuando utilizabamos google maps
		if(mouseIsPressed){
			lastMousePosition = mousePosition;
			mousePosition = Input.mousePosition;
			differenceMouse = mousePosition - lastMousePosition;
			difference.x = inverted ? -difference.x : difference.x;
			difference.y = inverted ? -difference.y : difference.y;
			Vector3 tempPosition = gmap.transform.position;
			Vector3 tempRotation = gmap.transform.rotation.eulerAngles;
			tempPosition.y += 0.01f * differenceMouse.y;
			tempPosition.y = tempPosition.y > 5 ? 5 : tempPosition.y;
			tempPosition.y = tempPosition.y < 3 ? 3 : tempPosition.y;
			tempRotation.y += 0.1f *differenceMouse.x;
			tempRotation.y = tempRotation.y > 180 ? tempRotation.y - 360 : tempRotation.y;
			tempRotation.y = tempRotation.y < -180 ? tempRotation.y + 360 : tempRotation.y;
			gmap.transform.position = tempPosition;
			gmap.transform.rotation = Quaternion.Euler(tempRotation);
		}
		*/

		if (moveUpIsPressed) {
			if (moveUpCounter == 0)
				controller.MoveUp ();
				//gmap.GetComponent<GoogleMap> ().MoveUp ();
				//GameObject.Find("Map (2)").GetComponent<GoogleMap> ().MoveUp ();
				moveUpCounter = (moveUpCounter + 1) % maxWaitTicksDesp;
		}
		if (!moveUpIsPressed)
			moveUpCounter = 0;

		if (moveDownIsPressed) {
			if (moveDownCounter == 0)
				controller.MoveDown ();
				//gmap.GetComponent<GoogleMap> ().MoveDown ();
				//GameObject.Find("Map (7)").GetComponent<GoogleMap> ().MoveDown ();
			moveDownCounter = (moveDownCounter + 1) % maxWaitTicksDesp;
		}
		if (!moveDownIsPressed)
			moveDownCounter = 0;

		if (moveLeftIsPressed) {
			if (moveLeftCounter == 0)
				controller.MoveLeft ();
				//gmap.GetComponent<GoogleMap> ().MoveLeft ();
				//GameObject.Find("Map (4)").GetComponent<GoogleMap> ().MoveLeft ();
			moveLeftCounter = (moveLeftCounter + 1) % maxWaitTicksDesp;
		}
		if (!moveLeftIsPressed)
			moveLeftCounter = 0;

		if (moveRightIsPressed) {
			if (moveRightCounter == 0)
				controller.MoveRight ();
				//gmap.GetComponent<GoogleMap> ().MoveRight ();
				//GameObject.Find("Map (5)").GetComponent<GoogleMap> ().MoveRight ();
			moveRightCounter = (moveRightCounter + 1) % maxWaitTicksDesp;
		}
		if (!moveRightIsPressed)
			moveRightCounter = 0;

		// Me parecio util incluir este atajo de teclado para poder salir del
		// Game View facilmente. No obstante, es valido solo para desarrollar el
		// juego, dentro de Unity (o sea, hay que comentar esta seccion de
		// codigo si se va a hacer build del proyecto para probar el
		// ejecutable). Como sugerencia, se podria incluir un atajo de teclado
		// para salir del Game View con un boton del Xbox One Wireless
		// Controller.
		/*
		if (Input.GetKeyDown (KeyCode.Q)) {
			UnityEditor.EditorApplication.isPlaying = false;
			Application.Quit();
		}
		*/

		// He incluido estos controles de teclado para poder manejar el mapa
		// si no se dispone del Xbox One Wireless Controller. No se si deberia
		// comentarse esta seccion a la hora de generar el ejecutable, de modo
		// que el usuario solo pueda trabajar con el Xbox One Wireless
		// Controller, controlando mas la experiencia de usuario; o si deberia
		// dejarse sin comentar, para que el usuario tenga alguna forma
		// alternativa de interaccion en caso de no tener a mano el Xbox One
		// Wireless Controller.
		if (GlobalVars.keyboardActive) {
			if (Input.GetKeyDown (KeyCode.UpArrow))
				controller.MoveUp ();
				//gmap.GetComponent<GoogleMap> ().MoveUp ();
			if (Input.GetKeyDown (KeyCode.DownArrow))
				controller.MoveDown ();			
				//gmap.GetComponent<GoogleMap> ().MoveDown ();
			if (Input.GetKeyDown (KeyCode.LeftArrow))
				controller.MoveLeft ();			
				//gmap.GetComponent<GoogleMap> ().MoveRight ();
			if (Input.GetKeyDown (KeyCode.RightArrow))
				controller.MoveRight ();
				//gmap.GetComponent<GoogleMap> ().MoveLeft ();
			if (Input.GetKeyDown (KeyCode.KeypadPlus))
				controller.ZoomIn ();
				//gmap.GetComponent<GoogleMap> ().ZoomIn ();
			if (Input.GetKeyDown (KeyCode.KeypadMinus))
				controller.ZoomOut ();
				//gmap.GetComponent<GoogleMap> ().ZoomOut ();
		}

	}

	//IEnumerator DragWaiter() {
        //yield return new WaitForSeconds(0.21f);
		//gmap.transform.position = new Vector3(0f,5f,14.73f);
    //}


}
}
