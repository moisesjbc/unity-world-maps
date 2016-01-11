using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WMSComponent : OnlineTexturesRequester {
	public string serverURL = "http://idecan1.grafcan.com/ServicioWMS/OrtoExpress";
	public string fixedQueryString;
	public int currentBoundingBoxIndex = 0;
	public WMSRequest wmsRequest = null;
	public int serverURLindex = 0;
	public Vector2 bottomLeftCoordinates = new Vector2 ( 416000,3067000 );
	public Vector2 topRightCoordinates = new Vector2 ( 466000,3117000 );


	protected override string GenerateRequestURL (string nodeID)
	{
		Vector2 bottomLeftCoordinates = this.bottomLeftCoordinates;
		Vector2 topRightCoordinates = this.topRightCoordinates;
		GenerateWMSBoundingBox (nodeID, ref bottomLeftCoordinates, ref topRightCoordinates);
			
		string fixedUrl = serverURL + fixedQueryString;
		string bboxUrlQuery = 
			"&BBOX=" + bottomLeftCoordinates.x.ToString("F") + "," +
			bottomLeftCoordinates.y.ToString("F") + "," +
			topRightCoordinates.x.ToString("F") + "," +
			topRightCoordinates.y.ToString("F");

		return fixedUrl + bboxUrlQuery;
	}


	protected override string GenerateRequestID(string nodeID)
	{
		Vector2 bottomLeftCoordinates = this.bottomLeftCoordinates;
		Vector2 topRightCoordinates = this.topRightCoordinates;
		GenerateWMSBoundingBox (nodeID, ref bottomLeftCoordinates, ref topRightCoordinates);

		return 
			"texture-" + 
			bottomLeftCoordinates.x + "-" + 
			bottomLeftCoordinates.y + "-" +
			topRightCoordinates.x + "-" + 
			topRightCoordinates.y +
			".jpg";
	}


	private void GenerateWMSBoundingBox(string nodeID, ref Vector2 bottomLeftCoordinates, ref Vector2 topRightCoordinates)
	{
		for (int i = 1; i < nodeID.Length; i++) {
			float x0 = bottomLeftCoordinates.x;
			float y0 = bottomLeftCoordinates.y;
			float x1 = topRightCoordinates.x;
			float y1 = topRightCoordinates.y;
			float cx = (x0 + x1)/2.0f;
			float cy = (y0 + y1)/2.0f;

			if (nodeID [i] == '0') {
				bottomLeftCoordinates = new Vector2( x0, cy );
				topRightCoordinates = new Vector2 (cx, y1);
			}else if(nodeID[i] == '1'){
				bottomLeftCoordinates = new Vector2( x0, y0 );
				topRightCoordinates = new Vector2( cx, cy );
			}else if(nodeID[i] == '2'){
				bottomLeftCoordinates = new Vector2( cx, cy );
				topRightCoordinates = new Vector2( x1, y1 );
			}else if(nodeID[i] == '3'){
				bottomLeftCoordinates = new Vector2( cx, y0 );
				topRightCoordinates = new Vector2( x1, cy );
			}
		}
	}
}
