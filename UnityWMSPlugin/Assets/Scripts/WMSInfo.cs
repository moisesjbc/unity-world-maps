using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class WMSBoundingBox
{
	public string SRS;
	public Vector2 bottomLeftCoordinates;
	public Vector2 topRightCoordinates;


	public override string ToString ()
	{
		return bottomLeftCoordinates + ", " + topRightCoordinates;
	}


	public float ratio()
	{
		return (topRightCoordinates.x - bottomLeftCoordinates.x) / (topRightCoordinates.y - bottomLeftCoordinates.y);
	}
}


[Serializable]
public class WMSLayer
{
	public string title;
	public string name;
	public List<WMSBoundingBox> boundingBoxes;
	public WMSLayer parentLayer;

	public List<WMSBoundingBox> GetBoundingBoxes()
	{
		List<WMSBoundingBox> boundingBoxes = new List<WMSBoundingBox> ();

		if( parentLayer != null ){
			boundingBoxes.AddRange ( parentLayer.GetBoundingBoxes() );
		}
		boundingBoxes.AddRange ( this.boundingBoxes );

		return boundingBoxes;
	}


	public WMSBoundingBox GetBoundingBox( int index )
	{
		WMSBoundingBox[] boundingBoxes = GetBoundingBoxes().ToArray();
		
		if ( index < boundingBoxes.Length ) {
			return boundingBoxes[index];
		}else{
			throw new System.IndexOutOfRangeException();
		}
	}


	public string[] GetBoundingBoxesNames()
	{
		WMSBoundingBox[] boundingBoxes = GetBoundingBoxes().ToArray ();
		string[] boundingBoxesNames = new string[boundingBoxes.Length];

		for (int i=0; i<boundingBoxes.Length; i++) {
			boundingBoxesNames[i] = boundingBoxes[i].ToString ();
		}

		return boundingBoxesNames;
	}
}


[Serializable]
public class WMSInfo
{
	[SerializeField]
	public string serverTitle;
	[SerializeField]
	public WMSLayer[] layers = new WMSLayer[0]{};
	[SerializeField]
	private int currentLayerIndex = 0;

	public WMSInfo( string serverTitle, WMSLayer[] layers )
	{
		this.serverTitle = serverTitle;
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


	public List<WMSBoundingBox> GetBoundingBoxes(List<string> selectedLayers)
	{
		List<WMSBoundingBox> boundingBoxes = new List<WMSBoundingBox> ();

		foreach (WMSLayer layer in layers) {
			if( selectedLayers.Contains(layer.name) ){
				boundingBoxes.AddRange (layer.GetBoundingBoxes());
			}
		}

		return boundingBoxes;
	}


	public string[] GetBoundingBoxesNames(List<string> selectedLayers)
	{
		WMSBoundingBox[] boundingBoxes = GetBoundingBoxes(selectedLayers).ToArray ();
		string[] boundingBoxesNames = new string[boundingBoxes.Length];
		
		for (int i=0; i<boundingBoxes.Length; i++) {
			boundingBoxesNames[i] = boundingBoxes[i].ToString ();
		}
		
		return boundingBoxesNames;
	}


	public WMSBoundingBox GetBoundingBox(List<string> selectedLayers, int index)
	{
		return GetBoundingBoxes(selectedLayers).ToArray ()[index];
	}

}