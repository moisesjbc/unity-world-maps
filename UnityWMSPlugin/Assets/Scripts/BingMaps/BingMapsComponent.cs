using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BingMapsComponent : OnlineTexturesRequester {
	private string serverURL = "http://ecn.t0.tiles.virtualearth.net/tiles/a";
	public string initialSector = "0";
	public string urlTail = ".jpeg?g=4756";
	public float lattitude;
	public float longitude;
	public int initialZoom = 0;

	protected override string GenerateRequestID( string nodeID )
	{
		return 
			"bing-texture-" +
			nodeID +
			".jpg";
	}


	public void ComputeInitialSector()
	{
		initialSector = "";
		float minLongitude = -180f;
		float maxLongitude = 180f;
		float minLattitude = -90f;
		float maxLattitude = 90f;

		for( int i=0; i<initialZoom + 1; i++ ){
			float middleLongitude = (maxLongitude + minLongitude) / 2.0f;
			float middleLattitude = (maxLattitude + minLattitude) / 2.0f;
			float halfLongitude = (maxLongitude - minLongitude) / 2.0f;
			float halfLattitude = (maxLattitude - minLattitude) / 2.0f;

			if (longitude <= middleLongitude) {
				maxLongitude -= halfLongitude;
				if (lattitude <= middleLattitude) {
					initialSector += "2";
					maxLattitude -= halfLattitude;
				} else {
					initialSector += "0";
					minLattitude += halfLattitude;
				}
			} else {
				minLongitude += halfLongitude;
				if (lattitude <= middleLattitude) {
					initialSector += "3";
					maxLattitude -= halfLattitude;
				} else {
					initialSector += "1";
					minLattitude += halfLongitude;
				}
			}
		}
	}


	protected override string GenerateRequestURL( string nodeID )
	{
		// Children node numbering differs between QuadtreeLODNoDe and Bing maps, so we
		// correct it here.
		nodeID = nodeID.Substring(1).Replace('1','9').Replace('2','1').Replace('9','2');

		return CurrentFixedUrl().Replace ("<id>", nodeID);
	}


	public override string CurrentFixedUrl ()
	{
		return serverURL + initialSector + "<id>" + urlTail;
	}
}
