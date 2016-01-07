using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class WMSServerBookmarks {
	private List<string> serverURLs;


	public WMSServerBookmarks()
	{
		serverURLs = File.ReadAllLines ("Assets/WMSServerBookmarks").ToList();
	}


	~WMSServerBookmarks(){
		File.WriteAllLines("Assets/WMSServerBookmarks", serverURLs.ToArray());
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
