/*     INFINITY CODE 2013-2017      */
/*   http://www.infinity-code.com   */

using UnityEngine;

/// <summary>
/// Class control the map for the Texture.
/// </summary>
[System.Serializable]
[AddComponentMenu("Infinity Code/Online Maps/Controls/Texture")]
[RequireComponent(typeof(MeshRenderer))]
public class OnlineMapsTextureControl : OnlineMapsControlBase3D
{
    /// <summary>
    /// Singleton instance of OnlineMapsTextureControl control.
    /// </summary>
    public new static OnlineMapsTextureControl instance
    {
        get { return OnlineMapsControlBase.instance as OnlineMapsTextureControl; }
    }
		
    public override Vector2 GetCoords(Vector2 position)
    {

		Debug.LogError ("3D OnlineMapsTextureControl " + position.x +" "+position.y);

        RaycastHit hit;
        if (!cl.Raycast(activeCamera.ScreenPointToRay(position), out hit, OnlineMapsUtils.maxRaycastDistance)) return Vector2.zero;

        Renderer render = hit.collider.GetComponent<Renderer>();
        if (render == null || render.sharedMaterial == null || render.sharedMaterial.mainTexture == null) return Vector2.zero;

        Vector2 r = hit.textureCoord;

        r.x = r.x - 0.5f;
        r.y = r.y - 0.5f;

        int countX = map.width / OnlineMapsUtils.tileSize;
        int countY = map.height / OnlineMapsUtils.tileSize;

        double px, py;
        map.GetTilePosition(out px, out py);

        px += countX * r.x;
        py -= countY * r.y;
        map.projection.TileToCoordinates(px, py, map.zoom, out px, out py);
        return new Vector2((float)px, (float)py);
    }

    public override bool GetCoords(out double lng, out double lat, Vector2 position)
    {
        RaycastHit hit;

        lng = lat = 0;
        if (!cl.Raycast(activeCamera.ScreenPointToRay(position), out hit, OnlineMapsUtils.maxRaycastDistance)) return false;

        Renderer render = cl.GetComponent<Renderer>();
        if (render == null || render.sharedMaterial == null || render.sharedMaterial.mainTexture == null) return false;

        Vector2 r = hit.textureCoord;

        r.x = r.x - 0.5f;
        r.y = r.y - 0.5f;

        int countX = map.width / OnlineMapsUtils.tileSize;
        int countY = map.height / OnlineMapsUtils.tileSize;

        double px, py;
        map.GetTilePosition(out px, out py);

        px += countX * r.x;
        py -= countY * r.y;
        map.projection.TileToCoordinates(px, py, map.zoom, out lng, out lat);
        return true;
    }

    public override void SetTexture(Texture2D texture)
    {
        base.SetTexture(texture);
        GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }

	//TZ
	private Vector2 OnGetInputPosition()
	{
		//Debug.LogError ("3D OnlineMapsTextureControl - OnGetInputPosition");
		return Camera.main.ViewportToScreenPoint(new Vector3(0.5F, 0.5F, 0));
		/**** TZ
		#if !UNITY_EDITOR
		// On the device returns center of screen.
		Debug.LogError("Input Position XX");
		return Camera.main.ViewportToScreenPoint(new Vector3(0.5F, 0.5F, 0));
		//return activeCamera.ViewportToScreenPoint(new Vector3(0.5F, 0.5F, 0));
		//Ray ray = new Ray(m_Camera.position, m_Camera.forward);
		#else
		// In the editor returns the coordinates of the mouse cursor.
		Debug.LogError("Porras");
		return Input.mousePosition;
		#endif
		***/
	}

	private int OnGetTouchCount()
	{
		// If pressed Z, then it will work like the left mouse button.
		return Input.GetKey(KeyCode.Z) ? 1 : 0;
	}
	//

	private void Start()
	{
		// Intercepts getting the cursor coordinates.
		OnlineMapsControlBase.instance.OnGetInputPosition += OnGetInputPosition;

		// Intercepts getting the number of touches.
		OnlineMapsControlBase.instance.OnGetTouchCount += OnGetTouchCount;
	}

	public void AddMarker3D()
	{
		Vector2 aux = GetCoords ();
		Debug.LogError ("Create Marker 3D " + aux.x + " " + aux.y);
		AddMarker3D(aux, default3DMarker);
	}
}
