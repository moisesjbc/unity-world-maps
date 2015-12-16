﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WMSClient {

	public static WMSInfo Request( string server, string version = "1.1.0" )
	{
		string url = 
			server + "?REQUEST=GetCapabilities&SERVICE=WMS" + "&VERSION=" + version;

		WWW www = new WWW(url);
		while( !www.isDone );

		return WMSXMLParser.GetWMSInfo (www.text);
	}
	
}
