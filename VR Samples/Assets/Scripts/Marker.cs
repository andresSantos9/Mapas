// -------------------------------------------------------------------------
// Marker.cs:
// -------------------------------------------------------------------------
//
// Este script representa el prefab de un marcador "dinamico". La diferencia
// entre un marcador dinamico y un marcador "estatico" esta en que el primero
// es un GameObject que ofrece soporte para interaccion con las Oculus Rift
// Consumer Edition 2016 (ademas del Xbox One Wireless Controller), mientras
// que el segundo es un conjunto de pixels que forma parte de la imagen del
// mapa para un tick concreto y que por tanto no ofrece ningun tipo de
// interaccion.
//
// Autor:
//		Alvaro Caceres Mu~noz (100303602@alumnos.uc3m.es)
// Fecha:
// 		Septiembre de 2016
// -------------------------------------------------------------------------

using UnityEngine;
using VRStandardAssets.Utils;
using UnityEngine.UI;

// Esta clase representa el marcador dinamico en si mismo. Principalmente
// contiene metodos que permiten cambiar el color del marcador dependiendo de
// como se este interactuando con el mismo. La mayor parte de este codigo
// procede del proyecto VR Samples de la web de documentacion sobre realidad
// virtual de Unity.
public class Marker : MonoBehaviour
{
    // Estas variables representan los distintos materiales (y consecuentemente
    // los colores) que puede tener el marcador dependiendo de si lo estamos
    // pulsando, mirando... de esta forma, el marcador se comporta como un boton
    // compatible con realidad virtual.
    [SerializeField] private Material m_NormalMaterial;
    [SerializeField] private Material m_OverMaterial;
    [SerializeField] private Material m_ClickedMaterial;
    [SerializeField] private Material m_DoubleClickedMaterial;

    // Esta variable representa la capacidad de interaccion del marcador
    [SerializeField] private VRInteractiveItem m_InteractiveItem;
    // Esta variable es necesaria para cambiar el material del marcador
    [SerializeField] private Renderer m_Renderer;
    // Esta variable representa un prefab con un mensage que sera mostrado una
    // vez se haga click sobre el marcador.
    public Transform clickedMessage;

    // Este metodo asigna el material de "no pulsado" al marcador en cuanto este
    // ha sido creado.
    private void Awake ()
    {
        m_Renderer.material = m_NormalMaterial;
    }

    // Este metodo procede del proyecto VR Samples importado de la web de Unity.
    private void OnEnable()
    {
        m_InteractiveItem.OnOver += HandleOver;
        m_InteractiveItem.OnOut += HandleOut;
        m_InteractiveItem.OnClick += HandleClick;
        m_InteractiveItem.OnDoubleClick += HandleDoubleClick;
    }

    // Este metodo procede del proyecto VR Samples importado de la web de Unity.
    private void OnDisable()
    {
        m_InteractiveItem.OnOver -= HandleOver;
        m_InteractiveItem.OnOut -= HandleOut;
        m_InteractiveItem.OnClick -= HandleClick;
        m_InteractiveItem.OnDoubleClick -= HandleDoubleClick;
    }

    // Este metodo emula el "mouse hover" en el marcador, y cambia el material
    // del mismo para dar a entender este cambio de estado.
    private void HandleOver()
    {
        m_Renderer.material = m_OverMaterial;
    }

    // Este metodo emula el "mouse out" en el marcador, y cambia el material
    // del mismo para dar a entender este cambio de estado.
    private void HandleOut()
    {
        m_Renderer.material = m_NormalMaterial;
    }

    // Este metodo se activa al hacer click sobre el marcador, y cambia el
    // material del mismo para dar a entender este cambio de estado. Ademas,
    // crea un cuadro con un mensaje, donde se indica que el usuario ha
    // encontrado una de las localizaciones deseadas. Notese que se podria
    // cambiar el texto de dicho mensaje dependiendo de la localizacion, aunque
    // ese comportamiento esta por implementar.
    private void HandleClick()
    {
        m_Renderer.material = m_ClickedMaterial;
        Instantiate(clickedMessage, this.transform.position + new Vector3(1.5f,1.5f,-1), Quaternion.identity);
        var message = transform.Find("ClickedMessage");
        Text text = message.GetComponent<Text>();
        text.text = "asd";
    }

    // Este metodo emula el "mouse double click" en el marcador, y cambia el
    // material del mismo para dar a entender este cambio de estado.
    private void HandleDoubleClick()
    {
        m_Renderer.material = m_DoubleClickedMaterial;
    }
}
