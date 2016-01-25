using UnityEngine;
using System;

[Serializable]
public class WCSServerInfo
{
	[SerializeField]
	public string label = null;

	public WCSServerInfo( string label )
	{
		this.label = label;
	}
}