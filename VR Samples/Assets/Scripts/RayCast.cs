using UnityEngine;
using System.Collections;

public class RayCast : MonoBehaviour {

	public Transform player;
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		Vector3 forward = player.TransformDirection(Vector3.forward) * 10;
		//Debug.DrawRay(player.position, Quaternion.AngleAxis(-25, Vector3.up) * forward, Color.green);
		Debug.DrawRay(player.position, Quaternion.AngleAxis(22, Vector3.left) * forward, Color.green);
	}
}
