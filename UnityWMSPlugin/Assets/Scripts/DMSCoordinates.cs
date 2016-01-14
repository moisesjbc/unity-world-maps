using UnityEngine;
using System;
using System.Collections;

public class DMSCoordinates 
{
	public int degrees;
	public int minutes;
	public int seconds;
	public Enum sector;

	public float ToDecimalCoordinates()
	{
		return (float)degrees + (float)minutes / 60.0f + (float)seconds / 60.0f;
	}
}


public class Lattitude : DMSCoordinates
{
	enum LattitudeSector {N, S};
	public Lattitude()
	{
		sector = new LattitudeSector();
	}
}


public class Longitude : DMSCoordinates
{
	enum LongitudeSector {O, E};
	public Longitude()
	{
		sector = new LongitudeSector();
	}
}