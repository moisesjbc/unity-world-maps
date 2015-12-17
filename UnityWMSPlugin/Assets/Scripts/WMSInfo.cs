﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct WMSLayer
{
	public string title;
	public string name;
	public Vector2 bottomLeftCoordinates;
	public Vector2 topRightCoordinates;
	public string boundingBoxSRS;
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