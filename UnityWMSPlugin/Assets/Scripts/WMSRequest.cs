using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[Serializable]
public enum WMSRequestState
{
	DOWNLOADING = 0,
	OK,
	ERROR
}


[Serializable]
public class WMSRequestStatus
{
	public WMSRequestState state;
	public WMSInfo response;
	public string errorMessage;

	public WMSRequestStatus()
	{
		state = WMSRequestState.DOWNLOADING;
		response = null;
		errorMessage = "";
	}
}


[Serializable]
public class WMSRequest : ScriptableObject {
	[SerializeField]
	private WWW www;
	[SerializeField]
	public WMSRequestStatus status = new WMSRequestStatus ();

	public WMSRequest (string server, string version = "1.1.0")
	{
		string url = 
			server + "?REQUEST=GetCapabilities&SERVICE=WMS" + "&VERSION=" + version;
		
		www = new WWW (url);
	}

	public void OnEnable ()
	{
		Debug.LogWarning ("[OnEnable]: status.response == null?: " + (status.response == null) );
	}


	public WMSRequestStatus UpdateStatus()
	{
		if (status.state == WMSRequestState.DOWNLOADING) {
			if (www.isDone) {
				try{
					status.response = WMSXMLParser.GetWMSInfo (www.text);
					if (status.response != null) {
						status.state = WMSRequestState.OK;
					} else {
						status.state = WMSRequestState.ERROR;
						status.errorMessage = "ERROR: Unknown";
					}
				}catch( Exception e ){
					status.state = WMSRequestState.ERROR;
					status.errorMessage = "ERROR parsing WMS response: " + e.Message;
				}
			}
		}
		return status;
	}
}
