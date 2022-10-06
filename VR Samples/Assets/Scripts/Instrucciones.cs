using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Instrucciones : MonoBehaviour {

	public TextMesh instrucciones;
	public int maximoElementos;
	private string[] colores = new string [] {"Paris", "Londres","Lima", "Nueva York","Moscu","Madrid","Berlin","Bruselas","Roma","Buenos Aires",
		"Vancouver","Quito"};
	private int contador =0 ;
	private string colorActual;
	



	// Use this for initialization
	void Start () {


		instrucciones.text = colores[0] + " (" + 1 + ")";

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public string ElegirColor (){
		if (contador < maximoElementos) {
			contador++;
			//int n = contador;
			colorActual = colores [contador];
			instrucciones.text = colorActual + " (" + (contador + 1) + ")";
			return colores [contador];

		} else
			return colores [11];

	}

	public string ColorActual (){

		return colorActual;
	}

}
