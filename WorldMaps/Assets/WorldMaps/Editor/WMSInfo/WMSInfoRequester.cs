using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WMSInfoRequester {
	Dictionary<string, WMSRequest> requests_ = 
		new Dictionary<string, WMSRequest>();


	public string RequestWMSInfo( string serverURL )
	{
		string requestID = GenerateRequestID (serverURL);
		if (!requests_.ContainsKey (requestID)){
			Debug.Log ("Requesting WMS info: " + serverURL);
			requests_ [requestID] = new WMSRequest (serverURL);
		}
		return requestID;
	}


	public WMSRequest GetRequest( string requestID )
	{
		return requests_ [requestID];
	}
		

	protected string GenerateRequestID( string serverURL )
	{
		return serverURL;
	}
}
