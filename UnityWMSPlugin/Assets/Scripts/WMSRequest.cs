using UnityEngine;
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
	private WMSRequestStatus status;

	public WMSRequest (string server, string version = "1.1.0")
	{
		string url = 
			server + "?REQUEST=GetCapabilities&SERVICE=WMS" + "&VERSION=" + version;
		
		www = new WWW (url);
		status = new WMSRequestStatus ();
	}


	public WMSRequestStatus Status()
	{
		if (status.state == WMSRequestState.DOWNLOADING) {
			if (www.isDone) {
					status.response = WMSXMLParser.GetWMSInfo (www.text);
				if (status.response != null) {
					status.state = WMSRequestState.OK;
				} else {
					status.state = WMSRequestState.ERROR;
					status.errorMessage = "ERROR!";
				}
			}
		}
		return status;
	}
}

/*
public class WMSClient {
	private Dictionary<string,WMSRequest> requests_ = new Dictionary<string,WMSRequest>();

	public int serverURLindex = 0;

	public string Request( string server, string version = "1.1.0" )
	{
		string url = 
			server + "?REQUEST=GetCapabilities&SERVICE=WMS" + "&VERSION=" + version;

		string requestID = BuildRequestID (server, version);

		if (!requests_.ContainsKey (requestID)) {
			requests_.Add ( requestID, new WMSRequest(url) );
		}

		return requestID;
	}



	public WMSRequestStatus RequestStatus(string requestID)
	{
		if (requests_.ContainsKey (requestID)) {
			return requests_ [requestID].Status ();
		} else {
			throw new KeyNotFoundException("WMS request ID not found [" + requestID + "]");
		}
	}


	private string BuildRequestID( string server, string version = "1.1.0" )
	{
		return server + "@@@" + version;
	}


	private string GetHostFromURL( string url )
	{
		int separatorIndex = url.IndexOf("?");
		if (separatorIndex > 0){
			return url.Substring(0, separatorIndex);
		}
		return url;
	}
}
*/
