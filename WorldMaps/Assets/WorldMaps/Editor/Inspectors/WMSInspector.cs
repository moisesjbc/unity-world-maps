using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using System.Linq;

[CustomEditor(typeof(WMSComponent))]
public class WMSComponentInspector : Editor
{
	private static WMSServerBookmarks bookmarks = new WMSServerBookmarks();
	private static WMSInfoRequester wmsInfoRequester = new WMSInfoRequester();


	public override void OnInspectorGUI()
	{
		if (Application.isPlaying) {
			EditorGUILayout.LabelField ("Currently play mode editting is not allowed");
			return;
		}

		WMSComponent wmsComponent = (WMSComponent)target;

		bool serverChanged = false;
		bool layerChanged = false;
		bool boundingBoxChanged = false;

		EditorGUILayout.BeginVertical (EditorStyles.helpBox);
			EditorGUILayout.LabelField ("Server");
			DisplayServerSelector (ref wmsComponent, out serverChanged);

			if (serverChanged) {
				wmsComponent.selectedLayers.Clear ();
				RequestWMSInfo (ref wmsComponent);
			}

			WMSRequestStatus requestStatus = 
				wmsInfoRequester.GetRequest (wmsComponent.wmsRequestID).status;
				
			if (requestStatus.state != WMSRequestState.OK) {
				if( requestStatus.state == WMSRequestState.DOWNLOADING ){
					EditorGUILayout.HelpBox("Downloading WMS info ...", MessageType.Info);
				}else if( requestStatus.state == WMSRequestState.ERROR ){
					EditorGUILayout.HelpBox("ERROR downloading WMS info (see console for more info)", MessageType.Error);
				}

				if (GUI.changed) {
					EditorUtility.SetDirty (wmsComponent);
					EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
				}
				EditorGUILayout.EndVertical ();
				return;
			}

			WMSInfo wmsInfo = requestStatus.response;
			EditorGUILayout.LabelField ("Server title: " + wmsInfo.serverTitle);
			EditorGUILayout.LabelField ("Server abstract: " + wmsInfo.serverAbstract);
		EditorGUILayout.EndVertical ();

		if (wmsInfo.GetLayerTitles ().Length <= 0) {
			EditorGUILayout.LabelField("No layers");

			if (GUI.changed) {
				EditorUtility.SetDirty (wmsComponent);
				EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
			}
			return;
		}
			
		DisplayLayersSelector (ref wmsComponent, wmsInfo, out layerChanged);

		if (layerChanged) {
			wmsComponent.RequestTexturePreview ();
		}

		DisplayBoundingBoxPanel (ref wmsComponent, ref wmsInfo, out boundingBoxChanged);

		// Mark the target assert as changed ("dirty") so Unity save it to disk.
		if (GUI.changed) {
			EditorUtility.SetDirty (wmsComponent);
			EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
		}
	}


	private void RequestWMSInfo(ref WMSComponent wmsComponent)
	{
		wmsComponent.wmsRequestID = wmsInfoRequester.RequestWMSInfo (wmsComponent.serverURL);
		wmsComponent.RequestTexturePreview ();
	}


	private void DisplayServerSelector(ref WMSComponent wmsComponent, out bool serverChanged)
	{
		serverChanged = false;

		DisplayServerPopup (ref wmsComponent, ref serverChanged);

		string newServerURL = EditorGUILayout.TextField("Server URL:", wmsComponent.serverURL);

		serverChanged |= (newServerURL != wmsComponent.serverURL);
		wmsComponent.serverURL = newServerURL;

		if (wmsComponent.wmsRequestID != "") {
			WMSRequestStatus requestStatus = 
				wmsInfoRequester.GetRequest (wmsComponent.wmsRequestID).status;
			
			if (requestStatus.state == WMSRequestState.OK) {
				if (!bookmarks.ServerIsBookmarked (requestStatus.response.serverTitle)) {
					DisplayServerBookmarkButton (requestStatus.response.serverTitle, wmsComponent.serverURL);
				//} else {
				//	RemoveServerFromBookmarksButton (requestStatus.response.serverTitle);
				}
			}
		}
	}


	private void DisplayServerPopup(ref WMSComponent wmsComponent, ref bool serverChanged)
	{
		string[] serverTitles = bookmarks.ServerTitles();

		int newServerIndex = EditorGUILayout.Popup ("Bookmarked servers", 0, serverTitles);
		serverChanged = (newServerIndex != 0 && bookmarks.GetServerURL(serverTitles[newServerIndex]) != wmsComponent.serverURL);

		if (serverChanged) {
			wmsComponent.serverURL = bookmarks.GetServerURL (serverTitles[newServerIndex]);
		}
	}


	private void DisplayLayersSelector( ref WMSComponent wmsComponent, WMSInfo wmsInfo, out bool layerChanged )
	{
		layerChanged = false;

		EditorGUILayout.BeginVertical (EditorStyles.helpBox);
		EditorGUILayout.LabelField ("Layers");

		WMSLayer[] layers = wmsInfo.layers;
		for( int i=0; i<layers.Length; i++) {
			if( layers[i].name != "" ){
				bool newToggleValue = 
					EditorGUILayout.Toggle (layers[i].title, (wmsComponent.LayerSelected(layers[i].name)));
				
				layerChanged |= (newToggleValue != wmsComponent.LayerSelected(layers[i].name));
				wmsComponent.SetLayerSelected (layers[i].name, newToggleValue);
			}
		}

		if (wmsComponent.SelectedLayersNumber () == 0) {
			EditorGUILayout.HelpBox ("No layers selected", MessageType.Warning);
		}

		EditorGUILayout.EndVertical ();
	}


	public void DisplayBoundingBoxPanel(ref WMSComponent wmsComponent, ref WMSInfo wmsInfo, out bool boundingBoxChanged)
	{
		EditorGUILayout.BeginVertical (EditorStyles.helpBox);

		EditorGUILayout.LabelField ("Bounding Box");

		DisplayBoundingBoxSelector (ref wmsComponent, wmsInfo, out boundingBoxChanged);

		EditorGUILayout.LabelField ("SRS", wmsComponent.SRS);

		wmsComponent.keepBoundingBoxRatio = 
			EditorGUILayout.Toggle ("Keep ratio", wmsComponent.keepBoundingBoxRatio);

		Vector2 newBottomLeftCoordinates =
			EditorGUILayout.Vector2Field (
				"Bottom left coords.",
				wmsComponent.bottomLeftCoordinates
			);

		Vector2 newTopRightCoordinates =
			EditorGUILayout.Vector2Field (
				"Top right coords.",
				wmsComponent.topRightCoordinates
			);

		UpdateBoundingBox (
			ref wmsComponent.bottomLeftCoordinates,
			ref wmsComponent.topRightCoordinates,
			newBottomLeftCoordinates,
			newTopRightCoordinates,
			wmsComponent.keepBoundingBoxRatio);

		if( boundingBoxChanged || GUILayout.Button("Update bounding box preview (may take a while)")){
			wmsComponent.RequestTexturePreview ();
		}

		EditorGUILayout.EndVertical ();
	}


	private void DisplayBoundingBoxSelector( ref WMSComponent wmsComponent, WMSInfo wmsInfo, out bool boundingBoxChanged )
	{
		boundingBoxChanged = false;

		List<string> boundingBoxesNames = wmsInfo.GetBoundingBoxesNames(wmsComponent.selectedLayers).ToList();
		boundingBoxesNames.Insert (0, "Select bounding box from server");

		if( boundingBoxesNames.Count > 1 ){
			int newBoundingBoxIndex = 
				EditorGUILayout.Popup (
					"BBs from server",
					0, 
					boundingBoxesNames.ToArray()
					) - 1;
			
			boundingBoxChanged = (newBoundingBoxIndex != -1);

			if( boundingBoxChanged ){
				wmsComponent.SRS = wmsInfo.GetBoundingBox (wmsComponent.selectedLayers, newBoundingBoxIndex).SRS;
				WMSBoundingBox currentBoundingBox = wmsInfo.GetBoundingBox( wmsComponent.selectedLayers, newBoundingBoxIndex );

				wmsComponent.bottomLeftCoordinates = currentBoundingBox.bottomLeftCoordinates;
				wmsComponent.topRightCoordinates = currentBoundingBox.topRightCoordinates;
			}else{
				if( wmsComponent.selectedLayers.Count == 1 ){
					// If we have one layer selected, use the SRS of its first bounding box.
					wmsComponent.SRS = wmsInfo.GetBoundingBox (wmsComponent.selectedLayers, 0).SRS;
				}
			}
		}
	}


	private void UpdateBoundingBox( 
	                               ref Vector2 oldBottomLeftCoordinates, 
	                               ref Vector2 oldTopRightCoordinates,
	                               Vector2 newBottomLeftCoordinates, 
	                               Vector2 newTopRightCoordinates,
								   bool keepRatio)
	{
		Vector2 auxBottomLeftCoordinates = newBottomLeftCoordinates;
		Vector2 auxTopRightCoordinates = newTopRightCoordinates;

		if (keepRatio) {
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
		}
		
		oldBottomLeftCoordinates = auxBottomLeftCoordinates;
		oldTopRightCoordinates = auxTopRightCoordinates;
	}


	private void DisplayServerBookmarkButton(string serverTitle, string serverURL)
	{
		if (GUILayout.Button ("Bookmark server")){
			bookmarks.BookmarkServer (serverTitle, serverURL);
		}
	}


	private void DisplayRemoveServerFromBookmarksButton(string serverTitle)
	{
		if (GUILayout.Button ("Remove server from bookmarks")){
			bookmarks.RemoveServerFromBookmarks (serverTitle);
		}
	}


	public void OnEnable()
	{
		WMSComponent wmsComponent = (WMSComponent)target;
		RequestWMSInfo (ref wmsComponent);
		EditorApplication.update += Refresh;
	}


	public void OnDisable()
	{
		EditorApplication.update -= Refresh;
	}


	public void Refresh()
	{
		WMSComponent wmsComponent = (WMSComponent)target;
		wmsInfoRequester.GetRequest (wmsComponent.wmsRequestID).UpdateStatus ();
		Repaint ();
	}
}
