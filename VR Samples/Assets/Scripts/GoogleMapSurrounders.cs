using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// -----------------------------------------------------------------------------
// GoogleMap.cs:
// -----------------------------------------------------------------------------
//
// Este script representa el mapa que aparece en la pantalla gigante virtual.
// Contiene atributos que definen el aspecto del mapa, asi como metodos que
// permiten modificarlo. Es posible agregar o quitar marcadores, cambiar el
// nivel de zoom, o desplazar el mapa en todas las direcciones. A falta de una
// API de Google Maps adptada para Unity, esta clase utiliza la API Estatica de
// Google Maps, con todos los inconvenientes que esto acarrea.
// La API Estatica deGoogle funciona de modo que se envia una peticion a los
// servidores de Google Maps, y estos devuelven una imagen de una mapa segun la
// configuracion que se haya especificado en la peticion. Esto implica que, para
// cada cambio del mapa, hay que realizar una nueva peticion donde se actualizan
// (manualmente) las coordenadas del centro de mira del mapa, el nivel de zoom,
// y la posicion de los marcadores.
// La gran mayoria del codigo presente en este script proviene del paquete
// Google Maps for Unity. Se han realizado varias modificaciones con respecto a
// dicho paquete, como usar listas dinamicas en vez de arrays, borrar
// funcionalidad extra que no era relevante para este proyecto, y a~nadir
// metodos que posibilitan la interaccion con el mapa. Es recomendable consultar
// dicho paquete para comprender mejor parte de la funcionalidad de este script;
// no obstante, se ha intentado describir lo mejor posible la gran mayoria del
// contenido del mismo.
// Por ultimo, cabe destacar que esta clase contiene funcionalidad que supone
// una violacion de las condiciones de uso que impone Google. Este es el motivo
// por el cual varios proyectos similares a este (en tanto en cuanto ofrecian
// la posibilidad de interactuar con Google Maps desde Unity) han sido borrados
// de la Asset Store de Unity. Debido a decisiones de dise~no que estan fuera de
// mi alcance como becario (que a su vez provienen del caracter de prototipo
// inherente a este proyecto, y a la necesidad de combinarlo con un proyecto
// anterior de este mismo departamento que utilizaba Google Maps), he programado
// este script siguiendo las indicaciones del DEI. No obstante, he descubierto
// que es posible imitar el comportamiento de este script con OpenStreetMap, a
// pesar de resultar mas complicado de implementar, tal vez, que con Google
// Maps. Se puede consultar la siguiente web para mas detalles sobre una posible
// implementacion de un proyecto parecido al presente con OpenStreetMap:
// https://ralphbarbagallo.com/2013/02/19/displaying-maps-in-unity3d/
//
// Autor:
//		Alvaro Caceres Mu~noz (100303602@alumnos.uc3m.es)
// Fecha:
// 		Septiembre de 2016
// -----------------------------------------------------------------------------

// Esta clase representa el mapa que se muestra en la pantalla desde un punto de
// vista abstracto, es decir, centrandose en la informacion que dicho
// contiene y en las formas en que dicha informacion puede ser modificada por el
// usuario.
[System.Serializable]
public class GoogleMapSurrounders : MonoBehaviour
{
	// Este enumerado define el tipo de mapa que se puede seleccionar desde
	// Unity. Por defecto se utiliza la opcion RoadMap, que ha sido ajustada
	// desde del inspector de Unity para el objeto relativo a esta clase; el
	// motivo por el que escogi este tipo de mapa por defecto fue una decision
	// de tipo personal, y cambiarlo no influye en el funcionamiento del script.
	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}

	// Esta variable determina si el mapa deberia cargarse en cuanto se carga el
	// juego. Esta definida como verdadero por defecto.
	private bool loadOnStart = true;

	// Esta variable determina si el mapa deberia estar centrado de modo que
	// todos los marcadores esten a la misma distancia de dicho centro. Esta
	// definida como falso por defecto; esto es necesario para tener control del
	// centro del mapa con la variable centerLocation.
	private bool autoLocateCenter = false;

	public GoogleMapLocation centerLocation;
	public MapType mapType;

	// Esta variable define el conjunto de marcadores que hay en el mapa. Al
	// tratarse de una lista, se pueden a~nadir o quitar marcadores de forma
	// dinamica; existe una funcionalidad en el script que actualiza el estado
	// de los marcadores en el mapa de un tick al siguiente.
	public GoogleMapMarker markers = new GoogleMapMarker ();

	// Estas dos variables definen los tama~nos horizontal y vertical,
	// respectivamente, de la imagen del mapa, medidos en pixels.
	// Dichas variables han sido inicializadas de modo que las dimensiones
	// horizontal y vertical de la imagen recibida por los servidores de Google
	// Maps tenga la misma relacion de aspecto que la pantalla gigante virtual
	// creada en Unity. He buscado, ademas, la mayor resolucion posible teniendo
	// en cuenta el limite que impone la version gratuita de la API Estatica de
	// Google Maps.
	// Es posible obtener mayores resoluciones con la API de pago Estatica de
	// Google Maps, con lo que la imagen seria mas nitida para tama~nos mayores
	// de la pantalla gigante virtual y la experiencia de usuario mejoraria
	// notablemente.
	private int size1 = 600;
	private int size2 = 200;

	// Esta variable define el nivel de resolucion de la imagen del mapa. Esta
	// definido a la resolucion maxima, x2. La API de pago Estatica de Google
	// Maps permite una mayor resolucion.
	private bool doubleResolution = true;

	// Esta variable define el nivel de zoom del mapa. Defini el valor inicial
	// a 5 porque me parecio que se ofrecia una vista suficientemente amplia del
	// mundo y a la vez lo suficientemente reducida como para tener que
	// desplazarse por el mapa y cambiar el zoom siguiendo la demo para la cual
	// esta dise~nado este proyecto. Se podria cambiar a cualquier otro valor de
	// zoom sin problemas.
	public int zoom = 5;

	// Estas dos variables definen los niveles maximos y minimos de zoom que
	// ofrece Google Maps, respectivamente.
	private int maxZoomLevel = 21;
	private int minZoomLevel = 0;
	private Vector3 lastDragPosition = new Vector3(0,0,0);

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

	// Este metodo mueve el mapa hacia arriba.
	public void MoveUp ()
	{
		centerLocation.latitude += verticalDisplacementLevels[zoom];
		Refresh ();
	}

	// Este metodo mueve el mapa hacia abajo.
	public void MoveDown ()
	{
		centerLocation.latitude -= verticalDisplacementLevels[zoom];
		Refresh ();
	}

	// Este metodo mueve el mapa hacia la derecha.
	public void MoveRight ()
	{
		centerLocation.longitude += horizontalDisplacementLevels[zoom];
		Refresh ();
	}

	// Este metodo mueve el mapa hacia la izquierda.
	public void MoveLeft ()
	{
		centerLocation.longitude -= horizontalDisplacementLevels[zoom];
		Refresh ();
	}

	// Este metodo realiza una llamada a una corrutina de C# que a su vez
	// actualiza
	public void Refresh ()
	{
		StartCoroutine(_Refresh());
	}

	// Esta corrutina actualiza la imagen del mapa, incluyendo el numero y
	// posicion de los marcadores o el nivel de zoom, entre muchos otros
	// aspectos. La implementacion de este metodo no varia practicamente con
	// respecto a la original del paquete mencionado en los comentarios
	// introductorios de este script; en el mismo se puede encontrar una
	// explicacion detallada de como funciona dicho metodo. No obstante, su
	// funcionalidad basica comprende la formacion una peticion URL con todos
	// los parametros necesarios para utilizar la API Estatica de Google Maps,
	// envia dicha peticion, recibe una imagen del mapa actualizado, y la plasma
	// en la pantalla gigante virtual del juego.
	IEnumerator _Refresh ()
	{
		Debug.Log ("zoom: " + zoom);
		var url = "http://maps.googleapis.com/maps/api/staticmap";
		var qs = "";
		if (!autoLocateCenter) {
			if (centerLocation.address != "")
				qs += "center=" + WWW.UnEscapeURL (centerLocation.address);
			else {
				qs += "center=" + WWW.UnEscapeURL (string.Format ("{0},{1}", centerLocation.latitude, centerLocation.longitude));
			}

			qs += "&zoom=" + zoom.ToString ();
		}
		qs += "&size=" + WWW.UnEscapeURL (string.Format ("{0}x{1}", size1, size2));
		qs += "&scale=" + (doubleResolution ? "2" : "1");
		qs += "&maptype=" + mapType.ToString ().ToLower ();
		var usingSensor = false;
#if UNITY_IPHONE
		usingSensor = Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
#endif
		qs += "&sensor=" + (usingSensor ? "true" : "false");

		qs += "&markers=" + string.Format ("size:{0}|color:{1}|label:{2}", markers.size.ToString ().ToLower (), markers.color, markers.label);
		foreach (var loc in markers.locations) {
			if (loc.address != "")
				qs += "|" + WWW.UnEscapeURL (loc.address);
			else
				qs += "|" + WWW.UnEscapeURL (string.Format ("{0},{1}", loc.latitude, loc.longitude));
		}

		//Debug.Log(url + "?" + qs);
		var req = new WWW (url + "?" + qs);
		yield return req;
		GetComponent<Renderer>().material.mainTexture = req.texture;
	}

	// Este metodo carga la imagen del mapa por primera vez en la pantalla
	// gigante virtual.
	void Start ()
	{
		if (loadOnStart) Refresh();
	}

	// Este metodo aumenta el nivel de zoom del mapa.
	public void ZoomIn ()
	{
		if (zoom < maxZoomLevel)
			zoom ++;
		Refresh ();
	}

	// Este metodo disminuye el nivel de zoom del mapa.
	public void ZoomOut ()
	{
		if (zoom > minZoomLevel)
			zoom--;
		Refresh ();
	}

	// Este metodo arrastra el mapa.
	public void Drag (Vector3 difference)
	{
		centerLocation.latitude -= 10* difference.x * verticalDisplacementLevels[zoom];
		centerLocation.longitude += 10* difference.y * horizontalDisplacementLevels[zoom];
		Refresh ();
	}
}
