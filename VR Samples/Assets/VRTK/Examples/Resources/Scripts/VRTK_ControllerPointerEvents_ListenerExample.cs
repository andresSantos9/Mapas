namespace VRTK.Examples
{
    using UnityEngine;

    public class VRTK_ControllerPointerEvents_ListenerExample : MonoBehaviour
    {
		public GameObject map;
		public GameObject objGameController;

		private MapController mapController;
		private GameController gameController;

		private Vector3 destinationPosition;

        private void Start()
        {

			mapController 	= map.GetComponent<MapController>();
			gameController	= objGameController.GetComponent<GameController>();

            if (GetComponent<VRTK_SimplePointer>() == null)
            {
                Debug.LogError("VRTK_ControllerPointerEvents_ListenerExample is required to be attached to a Controller that has the VRTK_SimplePointer script attached to it");
                return;
            }

            //Setup controller event listeners
            GetComponent<VRTK_SimplePointer>().DestinationMarkerEnter += new DestinationMarkerEventHandler(DoPointerIn);
            GetComponent<VRTK_SimplePointer>().DestinationMarkerExit += new DestinationMarkerEventHandler(DoPointerOut);
            GetComponent<VRTK_SimplePointer>().DestinationMarkerSet += new DestinationMarkerEventHandler(DoPointerDestinationSet);
        }

        private void DebugLogger(uint index, string action, Transform target, RaycastHit raycastHit, float distance, Vector3 tipPosition)
        {
            string targetName = (target ? target.name : "<NO VALID TARGET>");
            string colliderName = (raycastHit.collider ? raycastHit.collider.name : "<NO VALID COLLIDER>");
            Debug.Log("Controller on index '" + index + "' is " + action + " at a distance of " + distance + " on object named [" + targetName + "] on the collider named [" + colliderName + "] - the pointer tip position is/was: " + tipPosition);
        }

        private void DoPointerIn(object sender, DestinationMarkerEventArgs e)
        {
            DebugLogger(e.controllerIndex, "POINTER IN", e.target, e.raycastHit, e.distance, e.destinationPosition);
			mapController.ManageMarkerHitByTouch (e.destinationPosition);
			destinationPosition = e.destinationPosition;
        }

        private void DoPointerOut(object sender, DestinationMarkerEventArgs e)
        {
            DebugLogger(e.controllerIndex, "POINTER OUT", e.target, e.raycastHit, e.distance, e.destinationPosition);
			//destinationPosition = null;
        }

        private void DoPointerDestinationSet(object sender, DestinationMarkerEventArgs e)
        {
            DebugLogger(e.controllerIndex, "POINTER DESTINATION", e.target, e.raycastHit, e.distance, e.destinationPosition);
			//destinationPosition = null;
			//mapController.AddMarkerInPos (e.destinationPosition);
        }

		public void AddMarker (){
			mapController.AddMarkerInPos (destinationPosition);
		}
	
    }
}