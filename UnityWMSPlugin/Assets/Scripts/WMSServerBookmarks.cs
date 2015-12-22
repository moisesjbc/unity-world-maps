using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WMSServerBookmarks {
	private List<string> serverURLs;


	public WMSServerBookmarks()
	{
		serverURLs =
			new List<string> ( 
				new string[]{
					"http://idecan1.grafcan.com/ServicioWMS/OrtoExpress",
					"http://www.ign.es/wms-inspire/pnoa-ma"
				}
			);
	}


	public void BookmarkServer( string server )
	{
		serverURLs.Add (server);
	}


	public void RemoveServerFromBookmarks(string serverURL)
	{
		serverURLs.Remove (serverURL);
	}


	public bool ServerIsBookmarked(string serverURL){
		return (serverURLs.BinarySearch (serverURL) >= 0);
	}


	public string[] ToArray()
	{
		return serverURLs.ToArray ();
	}
}
