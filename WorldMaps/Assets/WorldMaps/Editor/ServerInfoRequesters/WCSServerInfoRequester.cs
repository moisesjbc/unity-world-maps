using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WCSServerInfoRequester : ServerInfoRequester<WCSServerInfo> {
	protected override WCSServerInfo ParseResponse( string responseText )
	{
		Debug.Log (responseText);
		return WCSServerInfoXMLParser.GetWCSServerInfo (responseText);
	}


	protected override string BuildQueryString()
	{
		return "?REQUEST=GetCapabilities&SERVICE=WCS&VERSION=1.0.0";
	}
}
