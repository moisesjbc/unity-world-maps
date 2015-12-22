using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WMSComponent : MonoBehaviour {
	public string serverURL = "http://idecan1.grafcan.com/ServicioWMS/OrtoExpress";
	public string fixedQueryString;
	public WMSInfo wmsInfo = null;
	public string wmsRequestID = "";
	public string wmsErrorResponse = "";
	public int currentBoundingBoxIndex = 0;
	public Vector2 bottomLeftCoordinates = new Vector2 ( 416000,3067000 );
	public Vector2 topRightCoordinates = new Vector2 ( 466000,3117000 );
}
