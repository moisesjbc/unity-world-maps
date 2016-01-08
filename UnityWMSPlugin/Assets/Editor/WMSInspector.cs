using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

[CustomEditor(typeof(WMSComponent))]
public class WMSComponentInspector : Editor
{
	private static WMSServerBookmarks bookmarks = new WMSServerBookmarks();

	public override void OnInspectorGUI()
	{
		WMSComponent wmsComponent = (WMSComponent)target;
		EditorUtility.SetDirty (wmsComponent);

		bool serverChanged = false;
		bool layerChanged = false;
		bool boundingBoxChanged = false;

		DisplayServerSelector (ref wmsComponent, out serverChanged);

		if (wmsComponent.wmsRequest == null || serverChanged) {
			wmsComponent.wmsRequest = new WMSRequest (wmsComponent.serverURL, "1.1.0");
			wmsComponent.currentBoundingBoxIndex = 0;
		}

		WMSRequestStatus requestStatus = 
			wmsComponent.wmsRequest.status;
			
		if (requestStatus.state != WMSRequestState.OK) {
			if( requestStatus.state == WMSRequestState.DOWNLOADING ){
				EditorGUILayout.LabelField("Downloading WMS info ...");
			}else if( requestStatus.state == WMSRequestState.ERROR ){
				EditorGUILayout.LabelField(requestStatus.errorMessage);
			}

			if (GUI.changed) {
				EditorApplication.MarkSceneDirty ();
			}
			return;
		}

		WMSInfo wmsInfo = requestStatus.response;
			
		if (wmsInfo.GetLayerTitles ().Length <= 0) {
			EditorGUILayout.LabelField("No layers");

			if (GUI.changed) {
				EditorApplication.MarkSceneDirty ();
			}
			return;
		}
		
		DisplayLayersSelector (ref wmsComponent, wmsInfo, out layerChanged);

		DisplayBoundingBoxSelector (ref wmsComponent, wmsInfo, layerChanged, out boundingBoxChanged);

		Vector2 newBottomLeftCoordinates =
			EditorGUILayout.Vector2Field (
				"Bottom left coordinates",
				wmsComponent.bottomLeftCoordinates
				);
		
		Vector2 newTopRightCoordinates =
			EditorGUILayout.Vector2Field (
				"Top right coordinates",
				wmsComponent.topRightCoordinates
				);
						
		UpdateBoundingBox (ref wmsComponent.bottomLeftCoordinates,
		                   ref wmsComponent.topRightCoordinates,
		                   newBottomLeftCoordinates,
		                   newTopRightCoordinates);

		// Mark the target assert as changed ("dirty") so Unity save it to disk.
		if (GUI.changed) {
			EditorApplication.MarkSceneDirty ();
		}
	}


	private void DisplayServerSelector(ref WMSComponent wmsComponent, out bool serverChanged)
	{
		serverChanged = false;

		DisplayServerPopup (ref wmsComponent, ref serverChanged);

		string newServerURL = EditorGUILayout.TextField("Server URL:", wmsComponent.serverURL);

		serverChanged |= (newServerURL != wmsComponent.serverURL);
		wmsComponent.serverURL = newServerURL;

		if (wmsComponent.wmsRequest != null) {
			WMSRequestStatus requestStatus = 
				wmsComponent.wmsRequest.status;
			
			if (requestStatus.state == WMSRequestState.OK) {
				DisplayServerBookmarkButton (wmsComponent.serverURL);
				if (bookmarks.ServerIsBookmarked (wmsComponent.serverURL)) {
					DisplayRemoveServerFromBookmarksButton (wmsComponent.serverURL);
				}
			}
		}
	}


	private void DisplayServerPopup(ref WMSComponent wmsComponent, ref bool serverChanged)
	{
		string[] serverURLs = bookmarks.ToArray ();

		for( int i=0; i<serverURLs.Length; i++ ){
			serverURLs[i] = serverURLs[i].Replace ("/", "\\");
		}

		int newServerIndex = EditorGUILayout.Popup ("Bookmarked servers", wmsComponent.serverURLindex, serverURLs);
		serverChanged = (newServerIndex != wmsComponent.serverURLindex);

		if (serverChanged) {
			wmsComponent.serverURLindex = newServerIndex;
			wmsComponent.serverURL = serverURLs [wmsComponent.serverURLindex].Replace("\\", "/");
		}
	}


	private void DisplayLayersSelector( ref WMSComponent wmsComponent, WMSInfo wmsInfo, out bool layerChanged )
	{
		layerChanged = false;

		EditorGUILayout.LabelField ("Layers");

		WMSLayer[] layers = wmsInfo.layers;
		for( int i=0; i<layers.Length; i++) {
			if( layers[i].name != "" ){
				bool newToggleValue = 
					EditorGUILayout.Toggle (layers[i].title, layers[i].selected);

				layerChanged |= (newToggleValue != layers[i].selected);
				layers[i].selected = newToggleValue;
			}
		}
	}


	private void DisplayBoundingBoxSelector( ref WMSComponent wmsComponent, WMSInfo wmsInfo, bool layerChanged, out bool boundingBoxChanged )
	{
		boundingBoxChanged = false;

		if( layerChanged ){
			wmsComponent.currentBoundingBoxIndex = 0;
			boundingBoxChanged = true;
		}
		
		string[] boundingBoxesNames = wmsInfo.GetBoundingBoxesNames();

		if( boundingBoxesNames.Length > 0 ){
			wmsComponent.fixedQueryString = BuildWMSFixedQueryString( wmsInfo.layers, "1.1.0", wmsInfo.GetBoundingBox( wmsComponent.currentBoundingBoxIndex ).SRS );
			
			int newBoundingBoxIndex = 
				EditorGUILayout.Popup (
					"Bounding Box",
					wmsComponent.currentBoundingBoxIndex, 
					boundingBoxesNames
					);
			
			boundingBoxChanged = boundingBoxChanged || (newBoundingBoxIndex != wmsComponent.currentBoundingBoxIndex);
			wmsComponent.currentBoundingBoxIndex = newBoundingBoxIndex;

			if( layerChanged || boundingBoxChanged || GUILayout.Button ("Reset bounding box") ){
				WMSBoundingBox currentBoundingBox = wmsInfo.GetBoundingBox( wmsComponent.currentBoundingBoxIndex );

				wmsComponent.bottomLeftCoordinates = currentBoundingBox.bottomLeftCoordinates;
				wmsComponent.topRightCoordinates = currentBoundingBox.topRightCoordinates;
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
			bookmarks.BookmarkServer (serverURL);
		}
	}


	private void DisplayRemoveServerFromBookmarksButton(string serverURL)
	{
		if (GUILayout.Button ("Remove server from bookmarks")){
			bookmarks.RemoveServerFromBookmarks (serverURL);
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
		WMSComponent wmsComponent = (WMSComponent)target;
		if (wmsComponent.wmsRequest != null) {
			wmsComponent.wmsRequest.UpdateStatus ();
			Repaint ();
		}
	}
}
