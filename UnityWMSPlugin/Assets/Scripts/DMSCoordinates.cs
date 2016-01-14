using UnityEngine;
using System.Collections;

public class DMSCoordinates 
{
	public int degrees;
	public int minutes;
	public int seconds;

	public float ToDecimalCoordinates()
	{
		return (float)degrees + (float)minutes / 60.0f + (float)seconds / 60.0f;
	}
};


public class Lattitude : DMSCoordinates
{
}


public class Longitude : DMSCoordinates
{
}