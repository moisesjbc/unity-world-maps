using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;

[CustomEditor(typeof(BingMapsComponent))]
public class BingMapsInspector : Editor
{
	public override void OnInspectorGUI()
	{
		BingMapsComponent bingMapsComponent = (BingMapsComponent)target;
		EditorUtility.SetDirty (bingMapsComponent);

		EditorGUILayout.LabelField (bingMapsComponent.CurrentFixedUrl());

		bingMapsComponent.longitude = EditorGUILayout.Slider("Longitude: ", bingMapsComponent.longitude, -180.0f, 180f);
		bingMapsComponent.lattitude = EditorGUILayout.Slider ("Lattitude: ", bingMapsComponent.lattitude, -90.0f, 90.0f);
		bingMapsComponent.initialZoom = EditorGUILayout.IntSlider ("Zoom: ", bingMapsComponent.initialZoom, 0, 7);
		bingMapsComponent.ComputeInitialSector ();

		if (GUI.changed) {
			EditorApplication.MarkSceneDirty ();
		}
	}
}
