using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class WCSHeightMap : MonoBehaviour
{	
	private WWW request_ = null;
	public bool heightMapLoaded = false;
	public string serverURL = "http://www.idee.es/wcs/IDEE-WCS-UTM28N/wcsServlet";
	public string coverageName = "MDT_canarias";
	public string coverageLabel = "";
	public Vector2 bottomLeftCoordinates = new Vector2 ( 416000,3067000 );
	public Vector2 topRightCoordinates = new Vector2 ( 466000,3117000 );
	public string wcsRequestID = "";


	public void RequestHeightMap( string nodeID, int N ){
		heightMapLoaded = false;
		Vector2 bottomLeftCoordinates = this.bottomLeftCoordinates;
		Vector2 topRightCoordinates = this.topRightCoordinates;
		WMSComponent.GenerateWMSBoundingBox (nodeID, ref bottomLeftCoordinates, ref topRightCoordinates);

		string fixedUrl = serverURL + "?REQUEST=GetCoverage&SERVICE=WCS&VERSION=1.0.0&FORMAT=AsciiGrid&COVERAGE=" + coverageName + "&CRS=EPSG:25828&REFERER=CAPAWARE";
		string bboxUrlQuery = 
			"&BBOX=" + bottomLeftCoordinates.x + "," +
			bottomLeftCoordinates.y + "," +
			topRightCoordinates.x + "," +
			topRightCoordinates.y;
		string dimensionsUrlQuery =
			"&WIDTH=" + N +
			"&HEIGHT=" + N;

		string url = fixedUrl + bboxUrlQuery + dimensionsUrlQuery;

		Debug.Log ("heightMap URL for node [" + nodeID + "] - " + url);

		request_ = new WWW (url);
	}


	public void Update()
	{
		if (heightMapLoaded == false && request_ != null && request_.isDone) {
			if (request_.error == null) {
				float [,] heightMatrix = ParseHeightMatrix (request_.text);
				//if( GetComponent<QuadtreeLODPlane>().depth_ == 0 ){
					SetHeightsMap( heightMatrix );
				//}else{
				//	int vertexResolution = GetComponent<QuadtreeLODPlane> ().vertexResolution;
				//	SetHeightsMap( GetSubMatrix( heightMatrix, vertexResolution - 1, vertexResolution - 1, 2 * vertexResolution - 1, 2 * vertexResolution - 1 ) );
				//}
				heightMapLoaded = true;
			} else {
				string requestedURL = request_.url;
				request_ = null;
				throw new UnityException ("Errors when height map texture [" + requestedURL + "]:\n" + request_.error);
			}
		}
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


	private float[,] GetSubMatrix( float[,] M, 
		int startRow, 
		int startColumn,
		int lastRow,
		int lastColumn )
	{
		int N_ROWS = lastRow - startRow;
		int N_COLUMNS = lastColumn - startColumn;

		float[,] subMatrix = new float[N_ROWS,N_COLUMNS];

		for (int i=0; i<N_ROWS; i++) {
			for(int j=0; j<N_COLUMNS; j++){
				subMatrix[i,j] = M[i+startRow,j+startColumn];
			}
		}
		return subMatrix;
	}


	private void SetHeightsMap( float[,] heights )
	{
		Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;

		int vertexResolution = gameObject.GetComponent<QuadtreeLODPlane> ().vertexResolution;

		int rowOffset = 0;
		int columnOffset = 0;

		if (vertices.Length > (vertexResolution * vertexResolution)) {
			Debug.LogWarning ("Offset!");
			rowOffset = 1;
			columnOffset = 1;
		}

		int N_ROWS = heights.GetLength(0);
		for (int row=0; row<N_ROWS; row++) {
			// FIXME: This is forcing N_COLUMS = N_ROWS.
			int N_COLUMNS = N_ROWS;
			for (int column=0; column<N_COLUMNS; column++) {
				int VERTEX_INDEX = (row + rowOffset) * N_COLUMNS + (column + columnOffset);
				vertices [VERTEX_INDEX].y = heights [row, column] / 10.0f; /// 500.0f;	// TODO: take metersPerUnit from OnlineTexture
				Debug.LogWarning("vertices[" + (row + rowOffset) + ", " + (column + columnOffset) + "].y: " + vertices [VERTEX_INDEX].y);
			}
		}

		GetComponent<MeshFilter>().mesh.vertices = vertices;
		GetComponent<MeshFilter>().mesh.RecalculateBounds ();
		GetComponent<MeshFilter>().mesh.RecalculateNormals ();

		// Add a collider to the node.
		//gameObject_.AddComponent<MeshCollider>();
	}


	public virtual void CopyTo(WCSHeightMap copy)
	{
		copy.request_ = null;
		copy.heightMapLoaded = heightMapLoaded;
		copy.bottomLeftCoordinates = bottomLeftCoordinates;
		copy.topRightCoordinates = topRightCoordinates;
	}
}
