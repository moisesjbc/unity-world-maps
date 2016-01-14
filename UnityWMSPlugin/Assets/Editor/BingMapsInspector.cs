using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;

[CustomEditor(typeof(BingMapsComponent))]
public class BingMapsInspector : Editor
{
	const float MIN_LATTITUDE = -90.0f;
	const float MAX_LATTITUDE = 90.0f;

	const float MIN_LONGITUDE = -180.0f;
	const float MAX_LONGITUDE = 180.0f;

	const int MIN_ZOOM = 0;
	const int MAX_ZOOM = 7;

	static string lattitudeLabel = "Lattitude (" + MIN_LATTITUDE + ", " + MAX_LATTITUDE + "): ";
	static string longitudeLabel = "Longitude (" + MIN_LONGITUDE + ", " + MAX_LONGITUDE + "): ";
	static string zoomLabel = "Zoom (" + MIN_ZOOM + ", " + MAX_ZOOM + ")";


	public override void OnInspectorGUI()
	{
		BingMapsComponent bingMapsComponent = (BingMapsComponent)target;
		EditorUtility.SetDirty (bingMapsComponent);

		EditorGUILayout.LabelField (bingMapsComponent.CurrentFixedUrl());

		bingMapsComponent.dmsLattitude = (Lattitude)GenerateDMSCoordinatesField(lattitudeLabel, bingMapsComponent.dmsLattitude);
		bingMapsComponent.dmsLongitude = (Longitude)GenerateDMSCoordinatesField(longitudeLabel, bingMapsComponent.dmsLongitude);
		bingMapsComponent.initialZoom = EditorGUILayout.IntField (zoomLabel, bingMapsComponent.initialZoom);
		bingMapsComponent.ComputeInitialSector ();

		if (GUILayout.Button ("Update preview (may take a while)")) {
			bingMapsComponent.RequestTexturePreview ();
		}

		if (GUI.changed) {
			EditorApplication.MarkSceneDirty ();
		}
	}


	private DMSCoordinates GenerateDMSCoordinatesField(string label, DMSCoordinates dmsCoordinates)
	{
		EditorGUILayout.LabelField (label);
		EditorGUILayout.BeginHorizontal ();
		dmsCoordinates.degrees = EditorGUILayout.IntField (dmsCoordinates.degrees);
		dmsCoordinates.minutes = EditorGUILayout.IntField (dmsCoordinates.minutes);
		dmsCoordinates.seconds = EditorGUILayout.IntField (dmsCoordinates.seconds);
		EditorGUILayout.EndHorizontal ();

		return dmsCoordinates;
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
		BingMapsComponent bingMapsComponent = (BingMapsComponent)target;

		Texture2D previewTexture = bingMapsComponent.GetTexturePreview ();
		if (previewTexture != null) {
			var tempMaterial = new Material (bingMapsComponent.gameObject.GetComponent<MeshRenderer> ().sharedMaterial);
			tempMaterial.mainTexture = previewTexture;
			tempMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
			bingMapsComponent.gameObject.GetComponent<MeshRenderer> ().sharedMaterial = tempMaterial;

			Repaint ();
		}
	}
}
