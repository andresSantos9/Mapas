/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.OnlineMapsExamples
{
    [AddComponentMenu("Infinity Code/Online Maps/Examples (API Usage)/uGUICustomTooltipForAllMarkersExample")]
    public class uGUICustomTooltipForAllMarkersExample : MonoBehaviour
    {
        public GameObject tooltipPrefab;
        public Canvas container;

        private GameObject tooltip;

        private void Start()
        {
            OnlineMaps.instance.AddMarker(Vector2.zero, "Marker 1");
            OnlineMaps.instance.AddMarker(new Vector2(1, 1), "Marker 2");
            OnlineMaps.instance.AddMarker(new Vector2(2, 1), "Marker 3");
            OnlineMapsMarkerBase.OnMarkerDrawTooltip = delegate { };

            OnlineMaps.instance.OnUpdateLate += OnUpdateLate;
        }

        private void OnUpdateLate()
        {
            OnlineMapsMarker tooltipMarker = OnlineMaps.instance.tooltipMarker as OnlineMapsMarker;
            if (tooltipMarker != null)
            {
                if (tooltip == null)
                {
                    tooltip = Instantiate(tooltipPrefab) as GameObject;
                    (tooltip.transform as RectTransform).SetParent(container.transform);
                }
                Vector2 screenPosition = OnlineMapsControlBase.instance.GetScreenPosition(tooltipMarker.position);
                screenPosition.y += tooltipMarker.height;
                Vector2 point;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(container.transform as RectTransform, screenPosition, null, out point);
                (tooltip.transform as RectTransform).localPosition = point;
                tooltip.GetComponentInChildren<Text>().text = tooltipMarker.label;

            }
            else
            {
                OnlineMapsUtils.DestroyImmediate(tooltip);
                tooltip = null;
            }
        }
    }
}