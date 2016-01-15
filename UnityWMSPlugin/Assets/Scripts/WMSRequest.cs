using UnityEngine;
using System;
using System.IO;
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
	public string url;
	private WWW www;
	public WMSRequestStatus status = new WMSRequestStatus ();

	public WMSRequest (string server, string version = "1.1.0")
	{
		url = 
			server + "?REQUEST=GetCapabilities&SERVICE=WMS" + "&VERSION=" + version;

		if (RequestIsCached(url)) {
			ParseResponse (File.ReadAllText(URLToFilePath(url)));
		} else {
			www = new WWW (url);
		}
	}


	public static bool RequestIsCached(string url)
	{
		string filepath = URLToFilePath(url);

		// TODO: Check download date.
		return File.Exists (filepath);
	}
		

	public WMSRequestStatus UpdateStatus()
	{
		if (status.state == WMSRequestState.DOWNLOADING) {
			if (www.isDone) {
				ParseResponse (www.text);
			}
		}
		return status;
	}


	private void ParseResponse(string text)
	{
		try{
			status.response = WMSXMLParser.GetWMSInfo (text);
			if (status.response != null) {
				status.state = WMSRequestState.OK;
				// Cache the response in a file.
				File.WriteAllText(URLToFilePath(url), text);
			} else {
				status.state = WMSRequestState.ERROR;
				status.errorMessage = "ERROR: Unknown";
			}
		}catch( Exception e ){
			status.state = WMSRequestState.ERROR;
			status.errorMessage = "ERROR parsing WMS response: " + e.Message;
			Debug.LogError (status.errorMessage);
		}
	}


	public static string URLToFilePath(string url)
	{
		string filepath = url;

		filepath = filepath.Replace ('/', '-');
		filepath = filepath.Replace ('.', '-');
		filepath = filepath.Replace ('\\', '-');
		filepath = filepath.Replace (':', '-');
		filepath = filepath.Replace ('?', '-');

		return "Temp/" + filepath;
	}
}
