using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BingMapsComponent : OnlineTexturesRequester {
	private string serverURL = "http://ecn.t0.tiles.virtualearth.net/tiles/a";
	public string initialSector = "0";
	public string urlTail = ".jpeg?g=4756";
	public float lattitude;
	public float longitude;

	protected override string GenerateRequestID( string nodeID )
	{
		return 
			"bing-texture-" +
			nodeID +
			".jpg";
	}


	public void ComputeInitialSector()
	{
		if (longitude <= 0.0f) {
			if (lattitude <= 0.0f) {
				initialSector = "2";
			} else {
				initialSector = "0";
			}
		} else {
			if (lattitude <= 0.0f) {
				initialSector = "3";
			} else {
				initialSector = "1";
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
