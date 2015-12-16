using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WMSLayersRequester {

	public static List<string> RequestLayers( string server, string version = "1.1.0" )
	{
		string url = 
			server + "?REQUEST=GetCapabilities&SERVICE=WMS" + "&VERSION=" + version;
			
		Debug.Log ("url: " + url);

		WWW www = new WWW(url);
		while( !www.isDone );

		return WMSXMLParser.GetLayers (www.text);
	}
	
}
