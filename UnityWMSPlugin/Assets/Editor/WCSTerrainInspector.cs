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
		
		bool layerChanged = false;
		bool boundingBoxChanged = false;

		string newServerURL = EditorGUILayout.TextArea (quadtreeLODPlane.serverURL);
		if ( quadtreeLODPlane.wmsErrorResponse == "" && quadtreeLODPlane.wmsInfo == null || newServerURL != quadtreeLODPlane.serverURL) {
			Debug.Log ("Downloading layers ...");
			quadtreeLODPlane.serverURL = newServerURL;
			quadtreeLODPlane.wmsRequestID = wmsClient.Request (newServerURL, "1.1.0");
			quadtreeLODPlane.wmsInfo = null;
			quadtreeLODPlane.wmsErrorResponse = "";
			quadtreeLODPlane.currentLayerIndex = 0;
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

		int newLayerIndex = 
			EditorGUILayout.Popup (
				"Layers",
				quadtreeLODPlane.currentLayerIndex, 
				quadtreeLODPlane.wmsInfo.GetLayerTitles()
			);
		layerChanged = (newLayerIndex != quadtreeLODPlane.currentLayerIndex);
		quadtreeLODPlane.currentLayerIndex = newLayerIndex;

		if( layerChanged ){
			quadtreeLODPlane.currentBoundingBoxIndex = 0;
			boundingBoxChanged = true;
		}
		WMSLayer currentLayer = quadtreeLODPlane.wmsInfo.GetLayer ( quadtreeLODPlane.currentLayerIndex );

		string[] boundingBoxesNames = currentLayer.GetBoundingBoxesNames();

		if (boundingBoxesNames.Length <= 0) {
			EditorGUILayout.LabelField("No bounding boxes");
			return;
		}

		WMSBoundingBox	currentBoundingBox = currentLayer.GetBoundingBox( quadtreeLODPlane.currentBoundingBoxIndex );

		Debug.Log ( "boundingBoxesNames: " + boundingBoxesNames.Length );
		if( boundingBoxesNames.Length > 0 ){
			quadtreeLODPlane.fixedQueryString = BuildWMSFixedQueryString( currentLayer, "1.1.0", currentLayer.GetBoundingBox( quadtreeLODPlane.currentBoundingBoxIndex ).SRS );

			int newBoundingBoxIndex = 
				EditorGUILayout.Popup (
					"Bounding Box",
					quadtreeLODPlane.currentBoundingBoxIndex, 
					boundingBoxesNames
					);

			boundingBoxChanged = boundingBoxChanged || (newBoundingBoxIndex != quadtreeLODPlane.currentBoundingBoxIndex);
			quadtreeLODPlane.currentBoundingBoxIndex = newBoundingBoxIndex;

			Debug.Log ( "layerChanged: " + layerChanged );
			Debug.Log ( "boundingBoxChanged: " + boundingBoxChanged );
			if( layerChanged || boundingBoxChanged ){
				quadtreeLODPlane.bottomLeftCoordinates = currentBoundingBox.bottomLeftCoordinates;
				quadtreeLODPlane.topRightCoordinates = currentBoundingBox.topRightCoordinates;
			}
		}

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
		                  newTopRightCoordinates,
		                  currentBoundingBox.ratio ());

		// Mark the target assert as changed ("dirty") so Unity save it to disk.
		if (GUI.changed) {
			EditorUtility.SetDirty (quadtreeLODPlane);
		}
		Debug.Log ("Updating inspector ...OK");
	}


	private void UpdateBoundingBox( 
	                               ref Vector2 oldBottomLeftCoordinates, 
	                               ref Vector2 oldTopRightCoordinates,
	                               Vector2 newBottomLeftCoordinates, 
	                               Vector2 newTopRightCoordinates,
	                               float boundigBoxRatio )
	{
		Vector2 auxBottomLeftCoordinates = newBottomLeftCoordinates;
		Vector2 auxTopRightCoordinates = newTopRightCoordinates;

		// TODO: Keep aspect ratio.

		oldBottomLeftCoordinates = auxBottomLeftCoordinates;
		oldTopRightCoordinates = auxTopRightCoordinates;
	}


	private string BuildWMSFixedQueryString( WMSLayer layer, string wmsVersion, string SRS )
	{
		return 
			"?SERVICE=WMS&LAYERS=" + layer.name +
			"&REQUEST=GetMap&VERSION=" + wmsVersion +
			"&FORMAT=image/jpeg" +
			"&SRS=" + SRS +
		    "&STYLES=default" +
			"&WIDTH=128&HEIGHT=128&REFERER=CAPAWARE";
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
