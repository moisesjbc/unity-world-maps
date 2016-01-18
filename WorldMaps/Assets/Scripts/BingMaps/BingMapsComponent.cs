using UnityEngine;
using System.Collections;
using System;
using System.Text;

public enum BingMapsType
{
	AERIAL,
	ROADS
}

public class BingMapsComponent : OnlineTexture {
	public string serverURL = "http://ecn.{subdomain}.tiles.virtualearth.net/tiles/r{quadkey}.jpeg?g=4892&mkt={culture}&shading=hill";
	public string initialSector = "0";
	public Lattitude dmsLattitude = new Lattitude(28, 7, 38, Lattitude.LattitudeSector.N);
	public Longitude dmsLongitude = new Longitude(15, 25, 53, Longitude.LongitudeSector.W);
	public float longitude;
	public int initialZoom = 0;


	public void ComputeInitialSector()
	{
		float lattitude = dmsLattitude.ToDecimalCoordinates ();
		float longitude = dmsLongitude.ToDecimalCoordinates ();

		float sinLatitude = Mathf.Sin (lattitude * Mathf.PI / 180.0f);

		int pixelX = (int)( ((longitude + 180) / 360) * 256 * Mathf.Pow (2, initialZoom + 1) );
		int pixelY = (int)( (0.5f - Mathf.Log ((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Mathf.PI)) * 256 * Mathf.Pow (2, initialZoom + 1) );

		int tileX = Mathf.FloorToInt (pixelX / 256);
		int tileY = Mathf.FloorToInt (pixelY / 256);

		initialSector = TileXYToQuadKey (tileX, tileY, initialZoom + 1);
	}


	// Function taken from "Bing Maps Tile System": https://msdn.microsoft.com/en-us/library/bb259689.aspx
	public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
	{
		StringBuilder quadKey = new StringBuilder();
		for (int i = levelOfDetail; i > 0; i--)
		{
			char digit = '0';
			int mask = 1 << (i - 1);
			if ((tileX & mask) != 0)
			{
				digit++;
			}
			if ((tileY & mask) != 0)
			{
				digit++;
				digit++;
			}
			quadKey.Append(digit);
		}
		return quadKey.ToString();
	}


	public void ValidateServerURL()
	{
		if( serverURL.IndexOf("{quadkey}" ) < 0 ){
			throw new UnityException ("BingMaps inspector - missing {quadkey} in server URL");
		}
		if( serverURL.IndexOf("{subdomain}" ) < 0 ){
			throw new UnityException ("BingMaps inspector - missing {subdomain} in server URL");
		}
	}


	protected override string GenerateRequestURL( string nodeID )
	{
		// Children node numbering differs between QuadtreeLODNoDe and Bing maps, so we
		// correct it here.
		nodeID = nodeID.Substring(1).Replace('1','9').Replace('2','1').Replace('9','2');

		ValidateServerURL ();

		string url = CurrentFixedUrl ();
		url = url.Replace ("{quadkey}", initialSector + nodeID);
		url = url.Replace ("{subdomain}", "t0");
		return url;
	}


	public string CurrentFixedUrl ()
	{
		return serverURL;
	}


	public override void CopyTo(OnlineTexture copy)
	{
		BingMapsComponent target = (BingMapsComponent)copy;
		target.serverURL = serverURL;
		target.initialSector = initialSector;
		target.dmsLattitude = dmsLattitude;
		target.dmsLongitude = dmsLongitude;
		target.longitude = longitude;
		target.initialZoom = initialZoom;
		target.textureLoaded = textureLoaded;
		target.request_ = request_;
	}
}
