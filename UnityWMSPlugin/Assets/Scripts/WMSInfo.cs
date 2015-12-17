using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct WMSBoundingBox
{
	public string SRS;
	public Vector2 bottomLeftCoordinates;
	public Vector2 topRightCoordinates;
}


public struct WMSLayer
{
	public string title;
	public string name;
	public List<WMSBoundingBox> boundingBoxes;

	public string boundingBoxSRS()
	{
		if (boundingBoxes.Count > 0) {
			return boundingBoxes[0].SRS;
		}else{
			// TODO: Return parent bounding box.
			return null;
		}
	}
}


public class WMSInfo
{
	private WMSLayer[] layers;
	private int currentLayerIndex = 0;

	public WMSInfo( WMSLayer[] layers )
	{
		this.layers = layers;
	}


	public string[] GetLayerTitles()
	{
		string[] layerTitles = new string[layers.Length];
		
		for (int i=0; i<layers.Length; i++) {
			layerTitles[i] = layers[i].title;
		}

		return layerTitles;
	}


	public int CurrentLayerIndex()
	{
		return currentLayerIndex;
	}


	public WMSLayer GetLayer( int index )
	{
		return layers [index];
	}
}