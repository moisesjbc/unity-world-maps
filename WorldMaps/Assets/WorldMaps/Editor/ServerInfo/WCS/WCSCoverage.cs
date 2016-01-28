using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WCSCoverage {
	[SerializeField]
	public string label = null;

	[SerializeField]
	public string name = null;

	[SerializeField]
	public BoundingBox[] boundingBoxes;


	public WCSCoverage(string label, string name, BoundingBox[] boundingBoxes)
	{
		this.label = label;
		this.name = name;
		this.boundingBoxes = boundingBoxes;
	}
}
