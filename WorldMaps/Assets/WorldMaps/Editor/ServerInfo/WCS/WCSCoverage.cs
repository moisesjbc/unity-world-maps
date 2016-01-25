using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WCSCoverage {
	[SerializeField]
	public string label = null;

	[SerializeField]
	public string name = null;

	public WCSCoverage(string label, string name)
	{
		this.label = label;
		this.name = name;
	}
}
