using UnityEngine;
using System.Collections;
using System.IO;


//Script que contiene las acciones a realizar sobre el mapa: desplazamiento, zoom...
//Por ahora la interacción con los marcadores no está incluida
public class MapController : MonoBehaviour {




	private OnlineMaps map;
	private OnlineMapsControlBase control;
	private OnlineMapsProjection projection;

	// Estas dos variables definen los incrementos de posicion horizontal y
	// vertical en el mapa, respectivamente, para cada uno de los niveles de
	// zoom que es posible obtener en Google Maps.
	// El motivo por el cual hacen falta valores especificos de desplazamiento
	// para niveles especificos de zoom es que, si el nivel de zoom es muy alto
	// convendra desplazarse una distancia (en metros, por ejemplo) peque~na de
	// modo que no se pierdan detalles de las calles que se estan observando,
	// mientras que si el nivel de zoom es muy bajo, convendra desplazarse una
	// distancia grande de modo que se avance agilmente de un punto a otro del
	// mapa.
	// El motivo por el cual he considerado los incrementos de posicion
	// horizontal y vertical por separado es un motivo empirico: al realizar
	// pruebas, descubri que sumar una cantidad "x" a la posicion de mira
	// vertical en el mapa hacia que este se desplazara en mayor medida que al
	// sumar la misma cantidad a la posicion de mira horizontal.
	// El motivo por el cual he introducido los valores de desplazamiento de
	// forma manual, empirica, es que no he conseguido encontrar una funcion que
	// aproximara bien los valores.
	private float [] horizontalDisplacementLevels = {30,20,15,7,4,2,0.8f,0.4f,0.3f,0.17f,0.09f,0.05f,0.02f,0.008f,0.005f,0.002f,0.0008f,0.0004f,0.0002f,0.0001f,0.00008f,0.00003f};
	private float [] verticalDisplacementLevels = {20,10,7,3.5f,2,0.9f,0.6f,0.2f,0.15f,0.1f,0.045f,0.02f,0.01f,0.004f,0.002f,0.0008f,0.0006f,0.0004f,0.00013f,0.0001f,0.00005f,0.00003f};

	double incDefault= 5.0d; //incremento de desplazamiento por defecto

	AudioClip clipFail; 
	AudioClip clipZoom; 
	AudioClip clipMarker; 
	AudioClip clipDelete; 
	AudioClip clipFailDelete; 
	AudioSource audioSource;

	//Para hacer el cambio de color de los marcadores al pasar sobre ellos
	public Texture2D textureOver;
	OnlineMapsMarker[] markersFound;		//0 found by gaze, 1 found by touch
	OnlineMapsMarker[] markersLastFound;	//0 found by gaze, 1 found by touch

/*	Gaze 	= null;
	OnlineMapsMarker markerLastFoundGaze = null;
	OnlineMapsMarker markerFoundTouch     = null;
	OnlineMapsMarker markerLastFoundTouch = null;
*/

	private void Start()
	{
		map = OnlineMaps.instance;
		control = OnlineMapsControlBase.instance;
		projection = map.projection;

		clipFail   		= (AudioClip)Resources.Load ("Sonidos/Fail") as AudioClip;
		clipZoom   		= (AudioClip)Resources.Load ("Sonidos/Zoom") as AudioClip;
		clipMarker 		= (AudioClip)Resources.Load ("Sonidos/AddMarker") as AudioClip;
		clipDelete 		= (AudioClip)Resources.Load ("Sonidos/Delete") as AudioClip;
		clipFailDelete	= (AudioClip)Resources.Load ("Sonidos/failDelete") as AudioClip;
		audioSource = gameObject.GetComponent<AudioSource> ();

		markersFound     = new OnlineMapsMarker[2];
		markersLastFound = new OnlineMapsMarker[2];

		textureOver = Resources.Load<Texture2D> ("Textures/SelectedMarker");


	}

	// Este metodo aumenta el nivel de zoom del mapa.
	public void ZoomIn ()
	{
		Vector2 inputPosition =  control.GetInputPosition(); // Asi funcionaba para los mapas de Texture
		bool rdo = control.ZoomOnPoint (GlobalVars.incZoom, inputPosition);
		if (rdo == false){
			audioSource.PlayOneShot (clipFail, 0.5F);
		} else {
			audioSource.PlayOneShot (clipZoom, 0.7F);
		}		
		//map.zoom++;
	}

	// Este metodo disminuye el nivel de zoom del mapa.
	public void ZoomOut ()
	{
		Vector2 inputPosition =  control.GetInputPosition();
		bool rdo = control.ZoomOnPoint (-GlobalVars.incZoom, inputPosition);
		if (rdo == false){
			audioSource.PlayOneShot (clipFail, 0.5F);
		} else {
			audioSource.PlayOneShot (clipZoom, 0.7F);
		}		
		//map.zoom--;
	}
		
	public void MoveUp()
	{
		double longitude;
		double latitude;
		double incMov = verticalDisplacementLevels[map.zoom];

		map.GetPosition (out longitude, out latitude);
		map.SetPosition (longitude, latitude+incMov);
	}

	public void MoveDown()
	{
		double longitude;
		double latitude;
		double incMov = verticalDisplacementLevels[map.zoom];

		map.GetPosition (out longitude, out latitude);
		map.SetPosition (longitude, latitude-incMov);
	}

	public void MoveRight()
	{
		double longitude;
		double latitude;
		double incMov = horizontalDisplacementLevels[map.zoom];

		map.GetPosition (out longitude, out latitude);
		map.SetPosition (longitude+incMov, latitude);
	}

	public void MoveLeft()
	{
		double longitude;
		double latitude;
		double incMov = horizontalDisplacementLevels[map.zoom];

		map.GetPosition (out longitude, out latitude);
		map.SetPosition (longitude-incMov, latitude);
	}

	public void AddMarker(){
		control.CreateMarker ();
		audioSource.PlayOneShot (clipMarker, 0.5F);
		Vector3 pos = Camera.main.ViewportToScreenPoint (new Vector3 (0.5F, 0.5F, 0));
		string pos1 = ((OnlineMapsTileSetControl)control).GetCoords(((OnlineMapsTileSetControl)control).GetInputPosition()).ToString();
		Debug.LogError ("Marcador Colocado en: "+ GetComponent<Timer> ().StopTimer () + pos1);
		GetComponent<ImprimirResultados> ().Imprimir (GetComponent<Timer> ().StopTimer () + pos1);
		if (!GlobalVars.nextCityControl) {
			GetComponent<Instrucciones> ().ElegirColor ();

		}


	}

	public void RemoveMarker(){
		if ((markersFound [0] == null) && (markersFound [1] == null))
			audioSource.PlayOneShot (clipFailDelete, 0.5F);
		else {
			if (markersFound [0] != null) {
				map.RemoveMarker (markersFound [0]);
				audioSource.PlayOneShot (clipDelete, 0.5F);
			}
			if (markersFound [1] != null) {
				map.RemoveMarker (markersFound [1]);
				audioSource.PlayOneShot (clipDelete, 0.5F);
			}
		}
	}

	public void AddMarkerInPos(Vector3 pos){
		((OnlineMapsTileSetControl)control).CreateMarkerInPos (pos);
		audioSource.PlayOneShot (clipMarker, 0.5F);
		string pos1 = ((OnlineMapsTileSetControl)control).GetCoordsByWorldPosition(pos).ToString();
		Debug.LogError ("Marcador Colocado en: "+ GetComponent<Timer> ().StopTimer () + " Posicion: " + pos1); //este captura la posición y tiempo al colocar marcador AS
		GetComponent<ImprimirResultados> ().Imprimir (GetComponent<Timer> ().StopTimer () + pos1);
		if (!GlobalVars.nextCityControl) {
			GetComponent<Instrucciones> ().ElegirColor ();

		}




	}

	// Este metodo arrastra el mapa con posiciones.
	public void Drag (Vector3 difference)
	{
		double longitude;
		double latitude;
		map.GetPosition (out longitude, out latitude);
		map.SetPosition (longitude - difference.y * incDefault , latitude-0.6f * difference.x * incDefault);
		//centerLocation.latitude -= difference.y * verticalDisplacementLevels[zoom];
		//centerLocation.longitude -= 0.6f * difference.x * horizontalDisplacementLevels[zoom];
		//Refresh ();
	}

	// Este metodo arrastra el mapa con ángulos.
	public void DragAngle (Vector3 differenceAngle, Vector3 differenceHeight)
	{
		/*** TZ QUITADO HASTA QUE ARREGLE EL DRAG DE MANOS BIEN
		double longitude;
		double latitude;		
		map.GetPosition (out longitude, out latitude);

		if(differenceAngle.y > 180)
			differenceAngle.y -= 360;
		else if (differenceAngle.y < -180)
			differenceAngle.y += 360;

		map.SetPosition (longitude-differenceAngle.y / 13.7f * incDefault ,latitude-longitude- 0.5f * differenceHeight.y * incDefault);
		*/
		//centerLocation.longitude -= differenceAngle.y / 13.7f * horizontalDisplacementLevels[zoom];
		//centerLocation.latitude -= 0.5f * differenceHeight.y * verticalDisplacementLevels[zoom];
		//Refresh ();
	}




	//TZ 
	//Devuelve el marcador en la posicion apuntada o nulo si no hay ninguno
	public OnlineMapsMarker GetMarkerHit(Vector2 pos)
	{
		Vector2 coords = pos;

		OnlineMapsMarker marker = null;
		double lng = double.MinValue, lat = double.MaxValue;
		double mx, my;
		foreach (OnlineMapsMarker m in map.markers)
		{
			if (!m.enabled || !m.range.InRange(map.zoom)) continue;
			if (m.HitTest(coords, map.zoom))
			{
				m.GetPosition(out mx, out my);
				if (my < lat || (my == lat && mx > lng))
				{
					marker = m;
					lat = my;
					lng = mx;
				}
			}
		}

		return marker;
	}

	// Actualiza el color del marcador apuntado en ese momento encontrado y del último encontrado anteriormente si es necesario
	// pos es 0 para marcadores apuntados por Gaze  y 1 para marcadores apuntados por Oculus Touch
	void manageMarkerHit(OnlineMapsMarker p_markerFound, int pos){
		bool bRedraw = false;

		markersFound[pos] = p_markerFound;
		if ((markersLastFound[pos] != null) && (markersLastFound[pos] !=markersFound[pos])) {
			markersLastFound[pos].texture = map.defaultMarkerTexture;
			markersLastFound[pos].Init ();
			markersLastFound[pos] = null;
			bRedraw = true;
		}

		if (markersFound[pos] != null) {
			markersFound[pos].texture = textureOver;
			markersFound[pos].Init ();
			markersLastFound[pos] = markersFound[pos];
			bRedraw = true;
		}

		if (bRedraw) {
			map.RedrawImmediately ();
		}
	}

	// Comprueba si el Oculus Touch apunta a un marcador y actualiza
	// Se llama desde el DoPointerIn del VRTK_ControllerPointerEvents_ListenerExample
	public void ManageMarkerHitByTouch(Vector3 pos){
		Vector2 coords = ((OnlineMapsTileSetControl)control).GetCoordsByWorldPosition (pos);
		OnlineMapsMarker markerFound = GetMarkerHit(coords);
		manageMarkerHit (markerFound, 1);			
	}

	// Update is called once per frame
	void Update () {
		OnlineMapsMarker markerFound = GetMarkerHit(control.GetCoords ());
		manageMarkerHit (markerFound, 0);
		Vector3 pos = Camera.main.ViewportToScreenPoint (new Vector3 (0.5F, 0.5F, 0));
		string pos1 = ((OnlineMapsTileSetControl)control).GetCoordsByWorldPosition(pos).ToString();
		//Debug.LogError ("Marcador Colocado en: "+ GetComponent<Timer> ().StopTimer () + " Posicion: " + pos1);
		if (GlobalVars.nextCityControl) {	//TZ 
			if (Input.GetKeyDown(KeyCode.Space)) {
				GetComponent<Instrucciones> ().ElegirColor ();
				//GetComponent<ImprimirResultados> ().ImprimirHeader (GetComponent<Timer> ().StopTimer () + pos1);
				GetComponent<ImprimirResultados> ().NextSample ();
			}	
		}
			
	}
		
}
