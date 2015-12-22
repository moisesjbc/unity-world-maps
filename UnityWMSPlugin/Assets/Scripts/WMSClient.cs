using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WMSClient {
	private Dictionary<string,WWW> requests_ = new Dictionary<string,WWW>();
	public List<string> serverURLs = 
		new List<string> ( 
			new string[]{
				"http://idecan1.grafcan.com/ServicioWMS/OrtoExpress",
				"http://www.ign.es/wms-inspire/pnoa-ma"
			}
		);
	public int serverURLindex = 0;

	public string Request( string server, string version = "1.1.0" )
	{
		string url = 
			server + "?REQUEST=GetCapabilities&SERVICE=WMS" + "&VERSION=" + version;

		string requestID = BuildRequestID (server, version);

		if (!requests_.ContainsKey (requestID)) {
			requests_.Add ( requestID, new WWW(url) );
		}

		return requestID;
	}


	public WMSInfo GetResponse( string requestID )
	{
		if (requests_.ContainsKey (requestID) && requests_ [requestID].isDone) {
			return WMSXMLParser.GetWMSInfo (requests_ [requestID].text);
		}

		return null;
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


	public void BookmarkServer( string server )
	{
		serverURLs.Add (server);
		serverURLindex = serverURLs.Count - 1;
	}


	public void RemoveServerFromBookmarks(string serverURL)
	{
		int indexToBeRemoved = serverURLs.FindIndex (e => e == serverURL);
		serverURLs.RemoveAt (indexToBeRemoved);

		// Update server index if it points to the removed element.
		if (serverURLindex == indexToBeRemoved) {
			if (indexToBeRemoved > 0) {
				serverURLindex--;
			} else if (serverURLs.Count > 0) {
				serverURLindex++;
			} else {
				serverURLindex = 0;
			}
		}
	}


	public bool ServerIsBookmarked(string serverURL){
		Debug.Log ("serverURLs.BinarySearch (" + serverURL +"): " + serverURLs.BinarySearch (serverURL));
		return (serverURLs.BinarySearch (serverURL) >= 0);
	}
}
