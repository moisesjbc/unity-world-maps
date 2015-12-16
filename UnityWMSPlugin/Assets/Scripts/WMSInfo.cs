using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct WMSLayer
{
	public string title;
	public WMSLayer[] subLayers;
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
		return GenerateLayerTitles (layers).ToArray ();
	}


	private List<string> GenerateLayerTitles( WMSLayer[] layers, string prefix = "" )
	{
		List<string> layersTitles = new List<string> ();

		for( int i=0; i<layers.Length; i++ ){
			if( layers[i].subLayers != null ){
				layersTitles.AddRange( GenerateLayerTitles ( layers[i].subLayers, prefix + layers[i].title + "/" ) );
			}else{
				layersTitles.Add ( prefix + layers[i].title );
			}
		}

		return layersTitles;
	}


	public int CurrentLayerIndex()
	{
		return currentLayerIndex;
	}
}