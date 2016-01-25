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


		DisplayServerSelectionPanel (ref wcsComponent, out serverChanged);

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
			return;
		}


		WCSServerInfo wcsServerInfo = wcsServerInfoRequester.GetResponse (wcsComponent.wcsRequestID);

		EditorGUILayout.BeginVertical (EditorStyles.helpBox);
			EditorGUILayout.LabelField ("Server info");
			EditorGUILayout.LabelField ("Server label : " + wcsServerInfo.label );
		EditorGUILayout.EndVertical ();

		/*
		bool serverChanged = false;
		bool layerChanged = false;
		bool boundingBoxChanged = false;

		EditorGUILayout.BeginVertical (EditorStyles.helpBox);
			EditorGUILayout.LabelField ("Server");
			DisplayServerSelector (ref wmsComponent, out serverChanged);

			

			RequestStatus requestStatus = 
				wmsInfoRequester.GetRequestStatus (wmsComponent.wmsRequestID);
				
			if (requestStatus != RequestStatus.OK) {
				if( requestStatus == RequestStatus.DOWNLOADING ){
					EditorGUILayout.HelpBox("Downloading WMS info ...", MessageType.Info);
				}else if( requestStatus == RequestStatus.ERROR ){
					EditorGUILayout.HelpBox("ERROR downloading WMS info (see console for more info)", MessageType.Error);
				}

				if (GUI.changed) {
					EditorUtility.SetDirty (wmsComponent);
					EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
				}
				EditorGUILayout.EndVertical ();
				return;
			}

			WMSInfo wmsInfo = wmsInfoRequester.GetResponse (wmsComponent.wmsRequestID);
			EditorGUILayout.LabelField ("Server title: " + wmsInfo.serverTitle);
			EditorGUILayout.LabelField ("Server abstract: " + wmsInfo.serverAbstract);
		EditorGUILayout.EndVertical ();

		if (wmsInfo.GetLayerTitles ().Length <= 0) {
			EditorGUILayout.LabelField("No layers");

			if (GUI.changed) {
				EditorUtility.SetDirty (wmsComponent);
				EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
			}
			return;
		}
			
		DisplayLayersSelector (ref wmsComponent, wmsInfo, out layerChanged);

		if (layerChanged) {
			wmsComponent.RequestTexturePreview ();
		}

		DisplayBoundingBoxPanel (ref wmsComponent, ref wmsInfo, out boundingBoxChanged);

		// Mark the target assert as changed ("dirty") so Unity save it to disk.
		if (GUI.changed) {
			EditorUtility.SetDirty (wmsComponent);
			EditorSceneManager.MarkSceneDirty (EditorSceneManager.GetActiveScene ());
		}
		*/
	}


	private void DisplayServerSelectionPanel (ref WCSHeightMap wcsComponent, out bool serverChanged)
	{
		serverChanged = false;

		EditorGUILayout.BeginVertical (EditorStyles.helpBox);

		EditorGUILayout.LabelField ("Server selection");

		string newServerURL = EditorGUILayout.TextField ("Server URL: ", wcsComponent.serverURL);
		if (newServerURL != wcsComponent.serverURL) {
			serverChanged = true;
			wcsComponent.serverURL = newServerURL;
		}

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
