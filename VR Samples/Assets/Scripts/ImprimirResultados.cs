using UnityEngine;
using System.Collections;
using System.IO;

public class ImprimirResultados : MonoBehaviour {

	public int numeroMuestras = 0;
	string fileName="C:\\Users\\dei-admin\\Desktop\\Resultados.txt";
	StreamWriter sr;
	int contador = 0;
	// Use this for initialization
	void Start () {

		sr = File.CreateText (fileName);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void NextSample (){       //aumenta contador si se pulsa barra espaciadora y cierra el archivo si ya se han tomado las muestras requeridas  AS


		contador++;
		Debug.LogError (contador);
		if (contador == numeroMuestras) {
			sr.WriteLine (numeroMuestras);
			sr.Close ();
			//			break;
		}
	}

	public void Imprimir (string texto){
		
		sr.WriteLine (texto + contador);



		//if (!GlobalVars.nextCityControl) {	//TZ 
			//contador++;
			//
		//	sr.WriteLine ("nueva ciudad");
		//}

		//if (!GlobalVars.nextCityControl) {	//TZ 
			//if (contador == numeroMuestras) {
				//sr.WriteLine (numeroMuestras);
				//sr.Close ();
				//			break;
			//}
		//}

			//if (contador > numeroMuestras)
//			break;

		
	}

}
