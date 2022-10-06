using UnityEngine;

//namespace InfinityCode.OnlineMapsExamples
//{
	/// <summary>
	/// Example of how to create a click event for dynamic markers and markers created by the inspector.
	/// </summary>
	[AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/MarkerClickExample")]
	public class MarkerClick : MonoBehaviour
	{
		private void Start()
		{
			OnlineMaps map = OnlineMaps.instance;

			// Add OnClick events to static markers
			foreach (OnlineMapsMarker marker in map.markers)
			{
				marker.OnClick += OnMarkerClick;
				//no va: marker.AddComponent<VRInteractiveItem> () as VRInteractiveItem;
			}

			// Add OnClick events to dynamic markers
			OnlineMapsMarker dynamicMarker = map.AddMarker(Vector2.zero, null, "Dynamic marker");
			dynamicMarker.OnClick += OnMarkerClick;
		}

		private void OnMarkerClick(OnlineMapsMarkerBase marker)
		{
			// Show in console marker label.
			Debug.LogError(marker.label);
		}
	}
//}