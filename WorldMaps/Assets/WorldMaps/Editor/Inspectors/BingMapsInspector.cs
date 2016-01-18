using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(BingMapsComponent))]
public class BingMapsInspector : Editor
{
	const float MIN_LATTITUDE = -90.0f;
	const float MAX_LATTITUDE = 90.0f;

	const float MIN_LONGITUDE = -180.0f;
	const float MAX_LONGITUDE = 180.0f;

	const int MIN_ZOOM = 0;
	const int MAX_ZOOM = 7;

	static string lattitudeLabel = "Lattitude (DMS): ";
	static string longitudeLabel = "Longitude (DMS): ";
	static string zoomLabel = "Zoom (" + MIN_ZOOM + ", " + MAX_ZOOM + ")";


	public override void OnInspectorGUI()
	{
		if (Application.isPlaying) {
			EditorGUILayout.LabelField ("Currently play mode editting is not allowed");
			return;
		}

		BingMapsComponent bingMapsComponent = (BingMapsComponent)target;

		bingMapsComponent.serverURL = EditorGUILayout.TextField (bingMapsComponent.serverURL);
		bingMapsComponent.dmsLattitude = (Lattitude)GenerateDMSCoordinatesField(lattitudeLabel, bingMapsComponent.dmsLattitude);
		bingMapsComponent.dmsLongitude = (Longitude)GenerateDMSCoordinatesField(longitudeLabel, bingMapsComponent.dmsLongitude);
		bingMapsComponent.initialZoom = EditorGUILayout.IntField (zoomLabel, bingMapsComponent.initialZoom);
		bingMapsComponent.ComputeInitialSector ();

		if (GUILayout.Button ("Update preview (may take a while)")) {
			Debug.Log("Decimal lattitude: " + bingMapsComponent.dmsLattitude.ToDecimalCoordinates());
			Debug.Log("Decimal longitude: " + bingMapsComponent.dmsLongitude.ToDecimalCoordinates());
			bingMapsComponent.RequestTexturePreview ();
		}

		if (GUI.changed) {
			EditorUtility.SetDirty (bingMapsComponent);
			EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
		}
	}


	private DMSCoordinates GenerateDMSCoordinatesField(string label, DMSCoordinates dmsCoordinates)
	{
		EditorGUILayout.LabelField (label);
		EditorGUILayout.BeginHorizontal ();
		dmsCoordinates.degrees = EditorGUILayout.FloatField (dmsCoordinates.degrees);
		dmsCoordinates.minutes = EditorGUILayout.FloatField (dmsCoordinates.minutes);
		dmsCoordinates.seconds = EditorGUILayout.FloatField (dmsCoordinates.seconds);
		dmsCoordinates.sector = EditorGUILayout.EnumPopup (dmsCoordinates.sector);
		EditorGUILayout.EndHorizontal ();

		return dmsCoordinates;
	}
}
