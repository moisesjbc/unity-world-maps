using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BingMapsComponent : OnlineTexturesRequester {
	public string serverURL = "http://ecn.t0.tiles.virtualearth.net/tiles/a<id>.jpeg?g=4756";


	protected override string GenerateRequestID( string nodeID )
	{
		return 
			"bing-texture-" +
			nodeID +
			".jpg";
	}


	protected override string GenerateRequestURL( string nodeID )
	{
		// Children node numbering differs between QuadtreeLODNoDe and Bing maps, so we
		// correct it here.
		nodeID = nodeID.Replace('1','9').Replace('2','1').Replace('9','2');

		return serverURL.Replace ("<id>", nodeID);
	}


	protected override string ExtractFixedURL (string fullURL)
	{
		int startPos = fullURL.IndexOf ("tiles/a") + "tiles/a".Length;
		int endPos = fullURL.IndexOf (".jpeg?g");
		if (startPos >= 0 && endPos > startPos) { 
			return fullURL.Remove (startPos, endPos - startPos);
		} else {
			return fullURL;
		}
	}


	protected override string CurrentFixedUrl ()
	{
		return ExtractFixedURL(serverURL);
	}
}
