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

		if( quadtreeLODPlane.wmsInfo.GetLayerTitles().Length > 0 ){
			Debug.Log ("Updating inspector ...");
			quadtreeLODPlane.currentLayerIndex = 
				EditorGUILayout.Popup (
					"Layers",
					quadtreeLODPlane.currentLayerIndex, 
					quadtreeLODPlane.wmsInfo.GetLayerTitles()
				);

			WMSLayer currentLayer = quadtreeLODPlane.wmsInfo.GetLayer ( quadtreeLODPlane.currentLayerIndex );
			quadtreeLODPlane.fixedQueryString = BuildWMSFixedQueryString( currentLayer, "1.1.0" );

			string[] boundingBoxesNames = currentLayer.GetBoundingBoxesNames();
			Debug.Log ( "boundingBoxesNames: " + boundingBoxesNames.Length );
			if( boundingBoxesNames.Length > 0 ){
				quadtreeLODPlane.currentBoundingBoxIndex = 
					EditorGUILayout.Popup (
						"Bounding Box",
						quadtreeLODPlane.currentBoundingBoxIndex, 
						boundingBoxesNames
						);

				if( currentLayer.GetBoundingBoxes().Count > 0 ){
					WMSBoundingBox currentBoundingBox = 
						currentLayer.GetBoundingBox( quadtreeLODPlane.currentBoundingBoxIndex );
					
					if( GUILayout.Button("Full Bounding Box") ){
						quadtreeLODPlane.bottomLeftCoordinates = currentBoundingBox.bottomLeftCoordinates;
						quadtreeLODPlane.topRightCoordinates = currentBoundingBox.topRightCoordinates;
					}
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
		}

		// Mark the target assert as changed ("dirty") so Unity save it to disk.
		if (GUI.changed) {
			EditorUtility.SetDirty (quadtreeLODPlane);
		}
		Debug.Log ("Updating inspector ...OK");
	}


	private string BuildWMSFixedQueryString( WMSLayer layer, string wmsVersion )
	{
		return 
			"?SERVICE=WMS&LAYERS=" + layer.name +
			"&REQUEST=GetMap&VERSION=" + wmsVersion +
			"&FORMAT=image/jpeg" +
			"&SRS=" + layer.boundingBoxSRS() +
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
