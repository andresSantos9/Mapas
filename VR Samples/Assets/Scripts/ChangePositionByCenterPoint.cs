using System;
using UnityEngine;

public class ChangePositionByCenterPoint : MonoBehaviour
{
    private OnlineMaps map;
    private OnlineMapsControlBase control;
    private OnlineMapsProjection projection;

    private bool isDrag;
    private double lastX, lastY;

    private void Start ()
    {
        map = OnlineMaps.instance;
        control = OnlineMapsControlBase.instance;
        projection = map.projection;
        control.allowUserControl = false;
    }

	private void Update ()
    {

								
		if ((Input.GetKey(KeyCode.Space)) || (Input.GetButton ("B"))) //|| (OVRInput.GetDown (OVRInput.Button.One)))
	    {
            double px, py;
	        if (control.GetCoords(out px, out py, new Vector2(Screen.width / 2, Screen.height / 2)))
	        {
                if (!isDrag)
                {
                    isDrag = true;
                    projection.CoordinatesToTile(px, py, map.zoom, out lastX, out lastY);
                }
                else
                {
                    projection.CoordinatesToTile(px, py, map.zoom, out px, out py);
                    double ox = px - lastX;
                    double oy = py - lastY;

                    if (ox * ox + oy * oy > 0.001)
                    {
                        map.GetPosition(out px, out py);
                        projection.CoordinatesToTile(px, py, map.zoom, out px, out py);
                        projection.TileToCoordinates(px + ox, py + oy, map.zoom, out px, out py);
                        map.SetPosition(px, py);

                        control.GetCoords(out lastX, out lastY, new Vector2(Screen.width / 2, Screen.height / 2));
                        projection.CoordinatesToTile(lastX, lastY, map.zoom, out lastX, out lastY);
                    }
                }
            }
	    }
        else if (isDrag) isDrag = false;
	}
}
