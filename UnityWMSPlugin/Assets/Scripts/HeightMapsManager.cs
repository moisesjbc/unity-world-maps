using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class HeightMapsManager : OnlineResourcesManager
{
	public string RequestHeightMap( 
	                            Vector2 bottomLeftCoordinates, 
	                            Vector2 topRightCoordinates,
	                            int N ){

		string newId = GenerateID (bottomLeftCoordinates, topRightCoordinates, N);

#if CACHE_RESOURCES
		if ( !ResourceNotRequested( newId ) ){
			Debug.LogFormat ("Requesting height map with id [{0}] - Cached!", newId);
			return newId;
		}
#endif
		string fixedUrl = "http://www.idee.es/wcs/IDEE-WCS-UTM28N/wcsServlet?REQUEST=GetCoverage&SERVICE=WCS&VERSION=1.0.0&FORMAT=AsciiGrid&COVERAGE=MDT_canarias&CRS=EPSG:25828&REFERER=CAPAWARE";
		string bboxUrlQuery = 
		"&BBOX=" + bottomLeftCoordinates.x + "," +
			bottomLeftCoordinates.y + "," +
			topRightCoordinates.x + "," +
			topRightCoordinates.y;
		string dimensionsUrlQuery =
		"&WIDTH=" + N +
			"&HEIGHT=" + N;
	
		string url = fixedUrl + bboxUrlQuery + dimensionsUrlQuery;
	
		Debug.Log ("heightMap URL - " + url);

		requests_ [newId] = new WWW (url);

		Debug.LogFormat ("Requesting height map with id [{0}] - NOT cached", newId);

		return newId;
	}


	protected string GenerateID( Vector2 bottomLeftCoordinates, Vector2 topRightCoordinates, int N )
	{
		return 
			"heightmap-" + 
			bottomLeftCoordinates.x + "-" + 
			bottomLeftCoordinates.y + "-" +
			topRightCoordinates.x + "-" + 
			topRightCoordinates.y + "-" +
			N;
	}


	public float[,] GetHeightMatrix( string id )
	{
		string heightMapSpec = null;

#if CACHE_RESOURCES
		// Check if we have a finished request with the same ID
		// and save the result to a file.
		if (requests_.ContainsKey (id) && requests_ [id].isDone) {
			StreamWriter outFile = new StreamWriter (FilePath (id));
			heightMapSpec = requests_ [id].text;
			outFile.Write (heightMapSpec);
			outFile.Close ();
		}
		
		// If the request hasn't finished, the file won't exist
		// yet.
		if (!File.Exists (FilePath (id))) {
			return null;
		}
		
		// The result file for the requested ID, exists. Parse
		// its contents and return the height matrix.
		if (heightMapSpec == null) {
			StreamReader inFile = new StreamReader (FilePath (id));
			heightMapSpec = inFile.ReadToEnd ();
			return ParseHeightMatrix (heightMapSpec);
		}
		
		if (heightMapSpec != null) {
			return ParseHeightMatrix (heightMapSpec);
		} else {
			return null;
		}
#else
		// Check if we have a finished request with the same ID
		// and save the result to a file.
		if ( requests_.ContainsKey(id) && requests_ [id].isDone ){
			//StreamWriter outFile = new StreamWriter( FilePath( id ) );
			heightMapSpec = requests_ [id].text;
		}
		
		if (heightMapSpec != null) {
			return ParseHeightMatrix (heightMapSpec);
		} else {
			return null;
		}
#endif
	}


	private float[,] ParseHeightMatrix( string heightMapSpec ){
		string[] specLines = heightMapSpec.Split ('\n');
		const int HEIGHTS_START_LINE = 6;
		int N_COLUMNS = int.Parse ( specLines [0].Split (new string[]{" "}, System.StringSplitOptions.RemoveEmptyEntries) [1] );
		int N_ROWS = int.Parse ( specLines [1].Split (new string[]{" "}, System.StringSplitOptions.RemoveEmptyEntries) [1] );
		
		float[,] heightsMatrix = new float[N_ROWS,N_COLUMNS];
		
		for (int i=0; i<N_ROWS; i++) {
			string[] heightsStrLine = specLines[HEIGHTS_START_LINE+i].Split (' ');
			
			for(int j=0; j<N_COLUMNS; j++){
				heightsMatrix[i,j] = float.Parse ( heightsStrLine[j] );
				heightsMatrix[i,j] = Mathf.Max( heightsMatrix[i,j], 0.0f );
			}
		}
		
		return heightsMatrix;
	}
}
