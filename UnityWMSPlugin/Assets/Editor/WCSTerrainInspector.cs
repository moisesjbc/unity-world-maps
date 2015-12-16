using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(QuadtreeLODPlane))]
public class WCSTerrainInspector : Editor 
{
	public override void OnInspectorGUI()
	{
		QuadtreeLODPlane quadtreeLODPlane = (QuadtreeLODPlane)target;

		string newServerURL = EditorGUILayout.TextArea (quadtreeLODPlane.serverURL);
		if ( quadtreeLODPlane.wmsInfo == null || newServerURL != quadtreeLODPlane.serverURL) 
		{
			Debug.Log ("Downloading layers");
			quadtreeLODPlane.serverURL = newServerURL;
			quadtreeLODPlane.wmsInfo = WMSClient.Request (newServerURL, "1.1.0" );
		}

		if( quadtreeLODPlane.wmsInfo.GetLayerTitles().Length > 0 ){
			quadtreeLODPlane.currentLayerIndex = 
				EditorGUILayout.Popup (
					"Layers",
					quadtreeLODPlane.currentLayerIndex, 
					quadtreeLODPlane.wmsInfo.GetLayerTitles()
				);

			WMSLayer currentLayer = quadtreeLODPlane.wmsInfo.GetLayer ( quadtreeLODPlane.currentLayerIndex );
		
			if( GUILayout.Button("Full Layer") ){
				quadtreeLODPlane.bottomLeftCoordinates = currentLayer.bottomLeftCoordinates;
				quadtreeLODPlane.topRightCoordinates = currentLayer.topRightCoordinates;
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
	}
}
