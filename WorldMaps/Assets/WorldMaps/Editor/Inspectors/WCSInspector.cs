using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using System.Linq;

[CustomEditor(typeof(WCSHeightMap))]
public class WCSComponentInspector : Editor
{
	private static WCSServerInfoRequester wcsServerInfoRequester = new WCSServerInfoRequester();


	public override void OnInspectorGUI()
	{
		if (Application.isPlaying) {
			EditorGUILayout.LabelField ("Currently play mode editting is not allowed");
			return;
		}

		WCSHeightMap wcsComponent = (WCSHeightMap)target;

		bool serverChanged = false;
		bool coverageChanged = false;

		WCSServerInfo wcsServerInfo = DisplayServerSelectionPanel (ref wcsComponent, out serverChanged);

		if (wcsServerInfo != null) {
			DisplayCoverageSelectionPanel (ref wcsComponent, wcsServerInfo.coverages, out coverageChanged);
			DisplayBoundingBoxPanel (ref wcsComponent, wcsServerInfo.coverages.First((WCSCoverage c) => { return c.name == wcsComponent.coverageName; }));
		}

		if (GUI.changed) {
			EditorUtility.SetDirty (wcsComponent);
			EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
		}
	}


	private WCSServerInfo DisplayServerSelectionPanel (ref WCSHeightMap wcsComponent, out bool serverChanged)
	{
		serverChanged = false;

		EditorGUILayout.BeginVertical (EditorStyles.helpBox);

		EditorGUILayout.LabelField ("Server selection");

		string newServerURL = EditorGUILayout.TextField ("Server URL: ", wcsComponent.serverURL);
		if (newServerURL != wcsComponent.serverURL) {
			serverChanged = true;
			wcsComponent.serverURL = newServerURL;
		}

		if (serverChanged) {
			//wcsComponent.selectedLayers.Clear ();
			RequestWCSServerInfo (ref wcsComponent);
		}

		RequestStatus requestStatus = 
			wcsServerInfoRequester.GetRequestStatus (wcsComponent.wcsRequestID);

		if (requestStatus != RequestStatus.OK) {
			if( requestStatus == RequestStatus.DOWNLOADING ){
				EditorGUILayout.HelpBox("Downloading WCS info ...", MessageType.Info);
			}else if( requestStatus == RequestStatus.ERROR ){
				EditorGUILayout.HelpBox("ERROR downloading WCS info (see console for more info)", MessageType.Error);
			}

			if (GUI.changed) {
				EditorUtility.SetDirty (wcsComponent);
				EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
			}
			EditorGUILayout.EndVertical ();

			return null;
		}


		WCSServerInfo wcsServerInfo = wcsServerInfoRequester.GetResponse (wcsComponent.wcsRequestID);

		EditorGUILayout.LabelField ("Server label : " + wcsServerInfo.label );
	
		EditorGUILayout.EndVertical ();

		return wcsServerInfo;
	}


	private void DisplayCoverageSelectionPanel (ref WCSHeightMap wcsComponent, WCSCoverage[] coverages, out bool coverageChanged)
	{
		coverageChanged = false; 

		List<string> layerPopupItems = new List<string> ();
		layerPopupItems.Add ("Select a coverage");
		foreach (WCSCoverage coverage in coverages) {
			layerPopupItems.Add (coverage.label);
		}

		EditorGUILayout.BeginVertical (EditorStyles.helpBox);

		EditorGUILayout.LabelField ("Coverage selection");

		int newCoverageIndex = EditorGUILayout.Popup (0, layerPopupItems.ToArray());

		if (newCoverageIndex != 0) {
			wcsComponent.coverageName = coverages [newCoverageIndex-1].name;
			wcsComponent.coverageLabel = coverages [newCoverageIndex-1].label;
		}

		EditorGUILayout.LabelField ("Selected coverage: " + wcsComponent.coverageLabel);

		EditorGUILayout.EndVertical ();
	}


	void DisplayBoundingBoxPanel (ref WCSHeightMap wcsComponent, WCSCoverage coverage)
	{
		EditorGUILayout.BeginVertical (EditorStyles.helpBox);

		EditorGUILayout.LabelField ("Bounding box selection");

		List<string> boundingBoxPopupItems = new List<string> ();
		boundingBoxPopupItems.Add ("Select a bounding box");
		foreach (BoundingBox boundingBox in coverage.boundingBoxes) {
			boundingBoxPopupItems.Add (boundingBox.ToString ());
		}

		int newBoundingBoxIndex = EditorGUILayout.Popup (0, boundingBoxPopupItems.ToArray());
		if (newBoundingBoxIndex != 0) {
			wcsComponent.bottomLeftCoordinates = coverage.boundingBoxes [newBoundingBoxIndex-1].bottomLeftCoordinates;
			wcsComponent.topRightCoordinates = coverage.boundingBoxes [newBoundingBoxIndex-1].topRightCoordinates;
		}

		wcsComponent.bottomLeftCoordinates = EditorGUILayout.Vector2Field("Bottom left coordinates: ", wcsComponent.bottomLeftCoordinates);
		wcsComponent.topRightCoordinates = EditorGUILayout.Vector2Field("Top right coordinates: ", wcsComponent.topRightCoordinates);

		EditorGUILayout.EndVertical ();
	}


	private void RequestWCSServerInfo(ref WCSHeightMap wcsComponent)
	{
		wcsComponent.wcsRequestID = wcsServerInfoRequester.RequestServerInfo (wcsComponent.serverURL);
		EditorApplication.update += Refresh;
	}


	public void OnEnable()
	{
		WCSHeightMap wcsComponent = (WCSHeightMap)target;
		RequestWCSServerInfo (ref wcsComponent);
	}


	public void OnDisable()
	{
		EditorApplication.update -= Refresh;
	}


	public void Refresh()
	{
		WCSHeightMap wcsComponent = (WCSHeightMap)target;
		if (wcsServerInfoRequester.Update (wcsComponent.wcsRequestID) != RequestStatus.DOWNLOADING) {
			// Stop refreshing when server download stops.
			EditorApplication.update -= Refresh;
			Repaint ();
		}
	}
}
