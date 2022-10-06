using UnityEngine;
using System.Collections;

public class GlobalVars : MonoBehaviour {

	public static bool nextCityControl = true; // variable que cuando esta a true hace que el control de pasar de una ciudad a otra
											   // la tenga el investigador pulsando la barra de espacio
											   // si esta a false, se pasa a la siguiente ciudad automáticamente cuando el sujeto pone 
											   // un marcador sobre el mapa (incluso si se ha equivocado al pulsar y lo ha puesto en otra parte)

	// Variables to activate/deactivate controls
	// Leap motion
	public static bool handsActive        = true; // Leap Motion Hands active
	public static bool handDragActive	  = false; //Drag the map using the hands. It doesn't work very well

	// Variables to hide/show the buttons to use with the hands (leap motion)
	public static bool navButtonsActive   = true;
	public static bool zoomButtonsActive  = true;

	// Variables for the XBox gamepad
	public static bool   gamePadActive    = false; // joystick
	// Para no liarnos al final:
	// A - poner marcador				public static string gpAddMarker  	  = "A";
	// X - quitar marcador				public static string gpDeleteMarker   = "X";
	// B - drag 						public static string gpDrag        	  = "B"; // Button to drag
	public static string gpZoomIn     	  = "R2";
	public static string gpZoomOut    	  = "L2";


	// Variables for the Rift Remote (the small control)
	public static bool riftRemoteActive   = true;

	// Oculus Touch
	public static bool touchActive  	  = true;  //Oculus Touch active. It should be active to use the right or left Touch
	public static bool touchActiveR  	  = true; // Oculus Touch right active
	public static bool touchActiveL  	  = true; // Oculus Touch left active

	//Other controls
	public static bool mouseActive        = false;
	public static bool keyboardActive     = false;


	// Variables to specify the increment of the movement triggered by the hands and the zoom
	public static double incMovHands  = 0.5d; // incremento del desplazamiento izq, der, arr, abj 
	public static int    incZoom      = 1;    // incremento del zoom

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

}
