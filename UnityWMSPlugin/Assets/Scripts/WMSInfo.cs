using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct WMSBoundingBox
{
	public string SRS;
	public Vector2 bottomLeftCoordinates;
	public Vector2 topRightCoordinates;


	public override string ToString ()
	{
		return bottomLeftCoordinates + ", " + topRightCoordinates;
	}
}


public class WMSLayer
{
	public string title;
	public string name;
	public List<WMSBoundingBox> boundingBoxes;
	public WMSLayer parentLayer;
	public bool selected = false;

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


public class WMSInfo
{
	public WMSLayer[] layers { get; private set; }
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