using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using Leap.Unity;

public class HandGrabber : MonoBehaviour {

	List<Hand> hands;
	LeapProvider provider;
	bool grabbing, found, anyHand;
	Hand grabbingHand;
	int grabbingHandNumber;
	public Transform map, buttons;
	// Use this for initialization
	void Start () {
		provider = FindObjectOfType<LeapProvider>() as LeapProvider;
		grabbing = false;
		found = false;
		anyHand = true;
		hands = new List<Hand>();
	}

	// Update is called once per frame
	void Update () {
		Frame frame = provider.CurrentFrame;
		if(frame.Hands.Count > 0){
			hands = frame.Hands;
			if(!anyHand){
				buttons.gameObject.active = true;
				foreach (Transform button in buttons)
				{
					Color color = Color.white;
					color.a = 0.2f;
					button.GetComponent<Renderer>().material.color = color;
				}
				anyHand = true;
			}
		}else{
			buttons.gameObject.active = false;
			anyHand = false;
		}
		if(grabbing){
			int i = 0;
			foreach(Hand hand in hands){
				if(checkHand(hand)){
					grabbing = hand.GrabStrength == 1;
					if(!grabbing){
						// //gmap.GetComponent<GoogleMap> ().Drag(gmap.transform.position - new Vector3(0f,5f,14.73f));
						//map.GetComponent<GoogleMap> ().DragAngle(map.transform.rotation.eulerAngles - new Vector3(0, 0, 180), map.transform.position - new Vector3(0, 4, 5.503f));
						map.GetComponent<MapController> ().DragAngle(map.transform.rotation.eulerAngles - new Vector3(0, 0, 180), map.transform.position - new Vector3(0, 4, 5.503f));
						buttons.gameObject.active = true;
						foreach (Transform button in buttons)
						{
							Color color = Color.white;
							color.a = 0.2f;
						    button.GetComponent<Renderer>().material.color = color;
						}
					}
					found = true;
					grabbingHandNumber = i;
				}
				i++;
			}
			if(!found){
				grabbing = false;
				//gmap.GetComponent<GoogleMap> ().Drag(gmap.transform.position - new Vector3(0f,5f,14.73f));
				map.GetComponent<MapController> ().DragAngle(map.transform.rotation.eulerAngles - new Vector3(0, 0, 180), map.transform.position - new Vector3(0, 4, 5.503f));
			}
		}else{
			int i = 0;
			if(hands.Count > 0)
				foreach(Hand hand in hands){
					if(!found)
						if(hand.GrabStrength == 1){
							found = true;
							grabbing = true;
							grabbingHand = hand;
							grabbingHandNumber = i;
							buttons.gameObject.active = false;
						}
					i++;
				}
			if(!found){
				grabbing = false;
			}
		}
		found = false;
		/*
		if(grabbing){
			Vector3 temp = gmap.transform.position;
			temp.x += 0.1f*hands[grabbingHandNumber].PalmVelocity.x;
			if (temp.x > 3)
				temp.x = 3;
			else if (temp.x < -3)
				temp.x = -3;
			temp.y += 0.1f*hands[grabbingHandNumber].PalmVelocity.y;
			if (temp.y > 6)
				temp.y = 6;
			else if (temp.y < 4)
				temp.y = 4;
			gmap.transform.position = temp;
		}*/
		if(grabbing){
			Vector3 tempPosition = map.transform.position;
			Vector3 tempRotation = map.transform.rotation.eulerAngles;
			tempPosition.y += hands[grabbingHandNumber].PalmVelocity.y * 0.1f;
			tempPosition.y = tempPosition.y > 5 ? 5 : tempPosition.y;
			tempPosition.y = tempPosition.y < 3 ? 3 : tempPosition.y;
			if(hands[grabbingHandNumber].PalmPosition.x > 0){
				if(hands[grabbingHandNumber].PalmPosition.z > 6.55)
					tempRotation.y += hands[grabbingHandNumber].PalmVelocity.x - hands[grabbingHandNumber].PalmVelocity.z;
				else
					tempRotation.y -= hands[grabbingHandNumber].PalmVelocity.x + hands[grabbingHandNumber].PalmVelocity.z;
			}else{
				if(hands[grabbingHandNumber].PalmPosition.z > 6.55)
					tempRotation.y += hands[grabbingHandNumber].PalmVelocity.x + hands[grabbingHandNumber].PalmVelocity.z;
				else
					tempRotation.y -= hands[grabbingHandNumber].PalmVelocity.x - hands[grabbingHandNumber].PalmVelocity.z;
			}
			map.transform.position = tempPosition;
			map.transform.rotation = Quaternion.Euler(tempRotation);
		}
		//if(hands[grabbingHandNumber].PalmVelocity.z > 1)
			//Debug.Log(hands[grabbingHandNumber].PalmVelocity.z);
		//Debug.Log("Hands: " + hands.Count);
		//Debug.Log("Strength " + hands[1].GrabStrength);
		//if(hands[0].PalmVelocity.y > 1)
			//Debug.Log("Velocity " + hands[0].PalmVelocity.y);
	}

	bool checkHand(Hand hand){
		if(hand.Id == grabbingHand.Id)
			return true;
		return false;
	}
}
