using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(BingMapsTexture))]
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

		BingMapsTexture bingMapsTexture = (BingMapsTexture)target;

		EditorGUILayout.LabelField ("Server template URL");
		bingMapsTexture.serverURL = EditorGUILayout.TextField (bingMapsTexture.serverURL);
		if (bingMapsTexture.serverURL == BingMapsTexture.testServerURL) {
			EditorGUILayout.HelpBox("This is a test server URL. When building your app, please generate a new server template URL by following the instructions on file Assets/WorldMaps/README.pdf", MessageType.Warning);
		}

		bingMapsTexture.latitude = EditorGUILayout.FloatField(lattitudeLabel, bingMapsTexture.latitude);
		bingMapsTexture.longitude = EditorGUILayout.FloatField(longitudeLabel, bingMapsTexture.longitude);
		bingMapsTexture.initialZoom = EditorGUILayout.IntField (zoomLabel, bingMapsTexture.initialZoom);
		bingMapsTexture.ComputeInitialSector ();

		if (GUILayout.Button ("Update preview (may take a while)")) {
			bingMapsTexture.RequestTexturePreview ();
		}

		if (GUI.changed) {
			EditorUtility.SetDirty (bingMapsTexture);
			EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
		}
	}
}
