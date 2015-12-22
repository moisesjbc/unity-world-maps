using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

[CustomEditor(typeof(QuadtreeLODPlane))]
public class WCSTerrainInspector : Editor 
{
	public static WMSClient wmsClient = new WMSClient();

	public override void OnInspectorGUI()
	{
		QuadtreeLODPlane quadtreeLODPlane = (QuadtreeLODPlane)target;

		bool serverChanged = false;
		bool layerChanged = false;
		bool boundingBoxChanged = false;

		DisplayServerSelector (ref quadtreeLODPlane, out serverChanged);

		if (serverChanged) {
			Debug.Log ("Downloading layers ...");
			quadtreeLODPlane.wmsRequestID = wmsClient.Request (quadtreeLODPlane.serverURL, "1.1.0");
			quadtreeLODPlane.wmsInfo = null;
			quadtreeLODPlane.wmsErrorResponse = "";
			quadtreeLODPlane.currentBoundingBoxIndex = 0;
			Debug.Log ("Downloading layers ...OK");
		}

		if( quadtreeLODPlane.wmsInfo == null ){
			Debug.Log ("quadtreeLODPlane.wmsErrorResponse: " + quadtreeLODPlane.wmsErrorResponse );
			if( quadtreeLODPlane.wmsErrorResponse == "" ){
				EditorGUILayout.LabelField("Downloading WMS info ...");
			}else{
				EditorGUILayout.LabelField(quadtreeLODPlane.wmsErrorResponse);
			}
			return;
		}

		if (quadtreeLODPlane.wmsInfo.GetLayerTitles ().Length <= 0) {
			EditorGUILayout.LabelField("No layers");
			return;
		}
		
		Debug.Log ("Updating inspector ...");
		
		DisplayLayersSelector (ref quadtreeLODPlane, out layerChanged);

		DisplayBoundingBoxSelector (ref quadtreeLODPlane, layerChanged, out boundingBoxChanged);

		Vector2 newBottomLeftCoordinates =
			EditorGUILayout.Vector2Field (
				"Bottom left coordinates",
				quadtreeLODPlane.bottomLeftCoordinates
				);
		
		Vector2 newTopRightCoordinates =
			EditorGUILayout.Vector2Field (
				"Top right coordinates",
				quadtreeLODPlane.topRightCoordinates
				);
						
		UpdateBoundingBox (ref quadtreeLODPlane.bottomLeftCoordinates,
		                   ref quadtreeLODPlane.topRightCoordinates,
		                   newBottomLeftCoordinates,
		                   newTopRightCoordinates);
		
		// Mark the target assert as changed ("dirty") so Unity save it to disk.
		if (GUI.changed) {
			EditorUtility.SetDirty (quadtreeLODPlane);
		}
		Debug.Log ("Updating inspector ...OK");
	}


	private void DisplayServerSelector(ref QuadtreeLODPlane quadtreeLODPlane, out bool serverChanged)
	{
		Debug.LogWarning ("DisplayingServerSelector");
		serverChanged = false;

		DisplayServerPopup (ref quadtreeLODPlane, ref serverChanged);
		if (serverChanged) {
			Debug.LogWarning ("Server changed with popup: " + quadtreeLODPlane.serverURL);
		}

		string newServerURL = EditorGUILayout.TextField("Server URL:", quadtreeLODPlane.serverURL);

		serverChanged |= (quadtreeLODPlane.wmsErrorResponse == "" && quadtreeLODPlane.wmsInfo == null || newServerURL != quadtreeLODPlane.serverURL);
		quadtreeLODPlane.serverURL = newServerURL;
		if (serverChanged) {
			Debug.LogWarning ("Server changed with text: " + quadtreeLODPlane.serverURL);
		}

		DisplayServerBookmarkButton (quadtreeLODPlane.serverURL);
		if( wmsClient.ServerIsBookmarked(quadtreeLODPlane.serverURL) ){
			DisplayRemoveServerFromBookmarksButton (quadtreeLODPlane.serverURL);
		}
	}


	private void DisplayServerPopup(ref QuadtreeLODPlane quadtreeLODPlane, ref bool serverChanged)
	{
		string[] serverURLs = wmsClient.serverURLs.ToArray ();

		for( int i=0; i<serverURLs.Length; i++ ){
			serverURLs[i] = serverURLs[i].Replace ("/", "\\");
		}

		int newServerIndex = EditorGUILayout.Popup ("Bookmarked servers", wmsClient.serverURLindex, serverURLs);
		serverChanged = (quadtreeLODPlane.wmsErrorResponse == "" && quadtreeLODPlane.wmsInfo == null || newServerIndex != wmsClient.serverURLindex);

		if (serverChanged) {
			wmsClient.serverURLindex = newServerIndex;
			quadtreeLODPlane.serverURL = serverURLs [wmsClient.serverURLindex].Replace("\\", "/");
		}
	}


	private void DisplayLayersSelector( ref QuadtreeLODPlane quadtreeLODPlane, out bool layerChanged )
	{
		layerChanged = false;

		EditorGUILayout.LabelField ("Layers");

		WMSLayer[] layers = quadtreeLODPlane.wmsInfo.layers;
		for( int i=0; i<layers.Length; i++) {
			if( layers[i].name != "" ){
				bool newToggleValue = 
					EditorGUILayout.Toggle (layers[i].title, layers[i].selected);

				layerChanged |= (newToggleValue != layers[i].selected);
				layers[i].selected = newToggleValue;
			}
		}
	}


	private void DisplayBoundingBoxSelector( ref QuadtreeLODPlane quadtreeLODPlane, bool layerChanged, out bool boundingBoxChanged )
	{
		boundingBoxChanged = false;

		if( layerChanged ){
			quadtreeLODPlane.currentBoundingBoxIndex = 0;
			boundingBoxChanged = true;
		}
		
		string[] boundingBoxesNames = quadtreeLODPlane.wmsInfo.GetBoundingBoxesNames();
		Debug.Log ( "boundingBoxesNames: " + boundingBoxesNames.Length );
		if( boundingBoxesNames.Length > 0 ){
			quadtreeLODPlane.fixedQueryString = BuildWMSFixedQueryString( quadtreeLODPlane.wmsInfo.layers, "1.1.0", quadtreeLODPlane.wmsInfo.GetBoundingBox( quadtreeLODPlane.currentBoundingBoxIndex ).SRS );
			
			int newBoundingBoxIndex = 
				EditorGUILayout.Popup (
					"Bounding Box",
					quadtreeLODPlane.currentBoundingBoxIndex, 
					boundingBoxesNames
					);
			
			boundingBoxChanged = boundingBoxChanged || (newBoundingBoxIndex != quadtreeLODPlane.currentBoundingBoxIndex);
			quadtreeLODPlane.currentBoundingBoxIndex = newBoundingBoxIndex;

			if( layerChanged || boundingBoxChanged || GUILayout.Button ("Reset bounding box") ){
				WMSBoundingBox currentBoundingBox = quadtreeLODPlane.wmsInfo.GetBoundingBox( quadtreeLODPlane.currentBoundingBoxIndex );

				quadtreeLODPlane.bottomLeftCoordinates = currentBoundingBox.bottomLeftCoordinates;
				quadtreeLODPlane.topRightCoordinates = currentBoundingBox.topRightCoordinates;
			}
		}
	}


	private void UpdateBoundingBox( 
	                               ref Vector2 oldBottomLeftCoordinates, 
	                               ref Vector2 oldTopRightCoordinates,
	                               Vector2 newBottomLeftCoordinates, 
	                               Vector2 newTopRightCoordinates )
	{
		Vector2 auxBottomLeftCoordinates = newBottomLeftCoordinates;
		Vector2 auxTopRightCoordinates = newTopRightCoordinates;

		// Compute aspect ratio
		float width = oldTopRightCoordinates.y - oldBottomLeftCoordinates.y;
		if (width == 0.0f) {
			width = 1.0f;
		}

		float height = oldTopRightCoordinates.y - oldBottomLeftCoordinates.y;
		if (height == 0.0f) {
			height = 1.0f;
		}

		float boundigBoxRatio = width / height;
		
		// Update Y coordinates according to bounding box ratio (if X changed).
		auxBottomLeftCoordinates.y += (newBottomLeftCoordinates.x - oldBottomLeftCoordinates.x) / boundigBoxRatio;
		auxTopRightCoordinates.y += (newTopRightCoordinates.x - oldTopRightCoordinates.x) / boundigBoxRatio;
		
		// Update X coordinates according to bounding box ratio (if Y changed).
		auxBottomLeftCoordinates.x += (newBottomLeftCoordinates.y - oldBottomLeftCoordinates.y) * boundigBoxRatio;
		auxTopRightCoordinates.x += (newTopRightCoordinates.y - oldTopRightCoordinates.y) * boundigBoxRatio;
		
		oldBottomLeftCoordinates = auxBottomLeftCoordinates;
		oldTopRightCoordinates = auxTopRightCoordinates;
	}


	private string BuildWMSFixedQueryString( WMSLayer[] layers, string wmsVersion, string SRS )
	{
		string layersQuery = "";
		string stylesQuery = "";
		foreach (WMSLayer layer in layers) {
			if( layer.selected && layer.name != "" ){
				layersQuery += layer.name + ",";
				stylesQuery += "default,";
			}
		}
		// Remove last character (',').
		if (layersQuery.Length > 0) {
			layersQuery = layersQuery.Remove(layersQuery.Length - 1);
			stylesQuery = stylesQuery.Remove(stylesQuery.Length - 1);
		}
		return 
			"?SERVICE=WMS" +
			"&LAYERS=" + layersQuery +
			"&REQUEST=GetMap&VERSION=" + wmsVersion +
			"&FORMAT=image/jpeg" +
			"&SRS=" + SRS +
		    "&STYLES=" + stylesQuery +
			"&WIDTH=128&HEIGHT=128&REFERER=CAPAWARE";
	}


	private void DisplayServerBookmarkButton(string serverURL)
	{
		if (GUILayout.Button ("Bookmark server")){
			wmsClient.BookmarkServer (serverURL);
		}
	}


	private void DisplayRemoveServerFromBookmarksButton(string serverURL)
	{
		if (GUILayout.Button ("Remove server from bookmarks")){
			wmsClient.RemoveServerFromBookmarks (serverURL);
		}
	}


	public void OnEnable()
	{
		EditorApplication.update += Refresh;
	}



	public void OnDisable()
	{
		EditorApplication.update -= Refresh;
	}


	public void Refresh()
	{
		QuadtreeLODPlane quadtreeLODPlane = (QuadtreeLODPlane)target;
		if ( quadtreeLODPlane.wmsErrorResponse == "" && quadtreeLODPlane.wmsInfo == null) {
			try {
				quadtreeLODPlane.wmsInfo = wmsClient.GetResponse (quadtreeLODPlane.wmsRequestID);
				if (quadtreeLODPlane.wmsInfo != null) {
					Repaint ();
				}
			} catch (Exception e) {
				quadtreeLODPlane.wmsErrorResponse = "Exception: " + e.Message;
				Repaint ();
				throw e;
			}
		}
	}
}
