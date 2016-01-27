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

	static string lattitudeLabel = "Lattitude (float): ";
	static string longitudeLabel = "Longitude (float): ";
	static string zoomLabel = "Zoom (" + MIN_ZOOM + ", " + MAX_ZOOM + ")";


	public override void OnInspectorGUI()
	{
		if (Application.isPlaying) {
			EditorGUILayout.LabelField ("Currently play mode editting is not allowed");
			return;
		}

		BingMapsComponent bingMapsComponent = (BingMapsComponent)target;

		EditorGUILayout.LabelField ("Server template URL");
		bingMapsComponent.serverURL = EditorGUILayout.TextField (bingMapsComponent.serverURL);
		if (bingMapsComponent.serverURL == BingMapsComponent.testServerURL) {
			EditorGUILayout.HelpBox("This is a test server URL. When building your app, please generate a new server template URL by following the instructions on file Assets/WorldMaps/README.pdf", MessageType.Warning);
		}

		bingMapsComponent.latitude = EditorGUILayout.FloatField(lattitudeLabel, bingMapsComponent.latitude);
		bingMapsComponent.longitude = EditorGUILayout.FloatField(longitudeLabel, bingMapsComponent.longitude);
		bingMapsComponent.initialZoom = EditorGUILayout.IntField (zoomLabel, bingMapsComponent.initialZoom);
		bingMapsComponent.ComputeInitialSector ();

		if (GUILayout.Button ("Update preview (may take a while)")) {
			bingMapsComponent.RequestTexturePreview ();
		}

		if (GUI.changed) {
			EditorUtility.SetDirty (bingMapsComponent);
			EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
		}
	}
}
