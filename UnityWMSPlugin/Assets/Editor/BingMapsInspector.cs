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

		EditorGUILayout.LabelField ("Server URL (fixed): " + bingMapsComponent.CurrentFixedUrl());

		if (GUI.changed) {
			EditorApplication.MarkSceneDirty ();
		}
	}
}
