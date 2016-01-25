using UnityEngine;
using System;

[Serializable]
public class WCSServerInfo
{
	[SerializeField]
	public string label = null;

	[SerializeField]
	public WCSCoverage[] coverages = null;


	public WCSServerInfo( string label, WCSCoverage[] coverages )
	{
		this.label = label;
		this.coverages = coverages;
	}
}