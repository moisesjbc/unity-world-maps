using UnityEngine;
using System;
using System.Collections;

public class DMSCoordinates 
{
	public float degrees;
	public float minutes;
	public float seconds;
	public Enum sector;


	public DMSCoordinates(float degrees = 0.0f, float minutes = 0.0f, float seconds = 0.0f)
	{
		this.degrees = degrees;
		this.minutes = minutes;
		this.seconds = seconds;
	}


	public virtual float ToDecimalCoordinates()
	{
		return degrees + minutes / 60.0f + seconds / 3600.0f;
	}


	public override string ToString()
	{
		return degrees.ToString("F") + "º " + minutes.ToString("F") + "' " + seconds.ToString("F") + "'' " + sector;
	}
}


public class Lattitude : DMSCoordinates
{
	public enum LattitudeSector {N, S};

	public Lattitude(float degrees = 0, float minutes = 0, float seconds = 0, LattitudeSector sector = LattitudeSector.N) :
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
	public enum LongitudeSector {W, E};

	public Longitude(float degrees = 0, float minutes = 0, float seconds = 0, LongitudeSector sector = LongitudeSector.W) :
		base(degrees, minutes, seconds)
	{
		this.sector = sector;
	}

	public override float ToDecimalCoordinates()
	{
		float decimalCoordinates = Mathf.Abs(base.ToDecimalCoordinates ());
		if ((LongitudeSector)sector == LongitudeSector.W) {
			decimalCoordinates = -decimalCoordinates;
		}
		return decimalCoordinates;
	}
}