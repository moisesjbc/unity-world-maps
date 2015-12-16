using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(QuadtreeLODPlane))]
public class WCSTerrainInspector : Editor 
{
	public int selectedRegion = 0;

	public override void OnInspectorGUI()
	{
		QuadtreeLODPlane quadtreeLODPlane = (QuadtreeLODPlane)target;

		Vector2 bottomLeftCoordinates = quadtreeLODPlane.bottomLeftCoordinates;
		Vector2 topRightCoordinates = quadtreeLODPlane.topRightCoordinates;

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
		
			quadtreeLODPlane.bottomLeftCoordinates = 
				EditorGUILayout.Vector2Field (
					"Bottom left coordinates",
					bottomLeftCoordinates
				);
			
			quadtreeLODPlane.topRightCoordinates = 
				EditorGUILayout.Vector2Field (
					"Top right coordinates",
					topRightCoordinates
				);
		}
		

	}


	private void GetSavedLocationCoordinates( string location, 
	                                    out Vector2 bottomLeftCoordinates, 
	                                    out Vector2 topRightCoordinates )
	{
		if (location == "Gran Canaria") {
			bottomLeftCoordinates = new Vector2 (416000, 3067000);
			topRightCoordinates = new Vector2 (466000, 3117000);
		} else {
			bottomLeftCoordinates = new Vector2 ( 310000,3090000 );
			topRightCoordinates = new Vector2 ( 392000,3172000 );
		}
	}
}
