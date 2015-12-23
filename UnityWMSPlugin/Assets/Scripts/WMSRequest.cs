using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum WMSRequestState
{
	DOWNLOADING = 0,
	OK,
	ERROR
}


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


public class WMSRequest {
	private WWW www;
	public WMSRequestStatus status { get; private set; }

	public WMSRequest (string server, string version = "1.1.0")
	{
		string url = 
			server + "?REQUEST=GetCapabilities&SERVICE=WMS" + "&VERSION=" + version;
		
		www = new WWW (url);
		status = new WMSRequestStatus ();
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
					status.errorMessage = "ERROR: " + e.Message;
				}
			}
		}
		return status;
	}
}
