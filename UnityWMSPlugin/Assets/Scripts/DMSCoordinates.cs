using UnityEngine;
using System;
using System.Collections;

public class DMSCoordinates 
{
	public int degrees;
	public int minutes;
	public int seconds;
	public Enum sector;


	public DMSCoordinates(int degrees = 0, int minutes = 0, int seconds = 0)
	{
		this.degrees = degrees;
		this.minutes = minutes;
		this.seconds = seconds;
	}


	public virtual float ToDecimalCoordinates()
	{
		return (float)degrees + (float)minutes / 60.0f + (float)seconds / 3600.0f;
	}
}


public class Lattitude : DMSCoordinates
{
	public enum LattitudeSector {N, S};

	public Lattitude(int degrees = 0, int minutes = 0, int seconds = 0, LattitudeSector sector = LattitudeSector.N) :
		base(degrees, minutes, seconds)
	{
		this.sector = sector;
	}


	public override float ToDecimalCoordinates()
	{
		float decimalCoordinates = Mathf.Abs(base.ToDecimalCoordinates ());
		if ((LattitudeSector)sector == LattitudeSector.S) {
			decimalCoordinates = -decimalCoordinates;
		}
		return decimalCoordinates;
	}
}


public class Longitude : DMSCoordinates
{
	public enum LongitudeSector {O, E};

	public Longitude(int degrees = 0, int minutes = 0, int seconds = 0, LongitudeSector sector = LongitudeSector.O) :
		base(degrees, minutes, seconds)
	{
		this.sector = sector;
	}

	public override float ToDecimalCoordinates()
	{
		float decimalCoordinates = Mathf.Abs(base.ToDecimalCoordinates ());
		if ((LongitudeSector)sector == LongitudeSector.O) {
			decimalCoordinates = -decimalCoordinates;
		}
		return decimalCoordinates;
	}
}