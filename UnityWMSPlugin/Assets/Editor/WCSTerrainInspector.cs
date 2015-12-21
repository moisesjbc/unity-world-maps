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
		
		DisplayLayersSelector (ref quadtreeLODPlane, out layerChanged);

		if( layerChanged ){
			quadtreeLODPlane.currentBoundingBoxIndex = 0;
			boundingBoxChanged = true;
		}
		WMSLayer currentLayer = quadtreeLODPlane.wmsInfo.GetLayer ( quadtreeLODPlane.currentLayerIndex );

		string[] boundingBoxesNames = currentLayer.GetBoundingBoxesNames();
		Debug.Log ( "boundingBoxesNames: " + boundingBoxesNames.Length );
		if( boundingBoxesNames.Length > 0 ){
			quadtreeLODPlane.fixedQueryString = BuildWMSFixedQueryString( quadtreeLODPlane.wmsInfo.layers, "1.1.0", currentLayer.GetBoundingBox( quadtreeLODPlane.currentBoundingBoxIndex ).SRS );

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
				WMSBoundingBox currentBoundingBox = currentLayer.GetBoundingBox( quadtreeLODPlane.currentBoundingBoxIndex );

				quadtreeLODPlane.bottomLeftCoordinates = currentBoundingBox.bottomLeftCoordinates;
				quadtreeLODPlane.topRightCoordinates = currentBoundingBox.topRightCoordinates;
			}
		}

		quadtreeLODPlane.bottomLeftCoordinates = 
			EditorGUILayout.Vector2Field (
				"Bottom left coordinates",
				quadtreeLODPlane.bottomLeftCoordinates
			);
		
		quadtreeLODPlane.topRightCoordinates = 
			EditorGUILayout.Vector2Field (
				"Top right coordinates",
				quadtreeLODPlane.topRightCoordinates
			);

		// Mark the target assert as changed ("dirty") so Unity save it to disk.
		if (GUI.changed) {
			EditorUtility.SetDirty (quadtreeLODPlane);
		}
		Debug.Log ("Updating inspector ...OK");
	}


	private void DisplayLayersSelector( ref QuadtreeLODPlane quadtreeLODPlane, out bool layerChanged )
	{
		layerChanged = false;

		EditorGUILayout.LabelField ("Layers");

		WMSLayer[] layers = quadtreeLODPlane.wmsInfo.layers;
		for( int i=0; i<layers.Length; i++) {
			bool newToggleValue = 
				EditorGUILayout.Toggle (layers[i].title, layers[i].selected);

			layerChanged |= (newToggleValue != layers[i].selected);
			layers[i].selected = newToggleValue;
		}
	}


	private string BuildWMSFixedQueryString( WMSLayer[] layers, string wmsVersion, string SRS )
	{
		string layersQuery = "";
		string stylesQuery = "";
		foreach (WMSLayer layer in layers) {
			if( layer.selected ){
				layersQuery += layer.name + ",";
				stylesQuery += "default,";
			}
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
