using UnityEngine;
using System.Collections;

public class MeshFactory {


	static public Mesh CreateMesh( float meshSize, int meshVertexResolution, bool extendBorders = false )
	{
		Vector3[] vertices;
		Vector2[] uv;

		GenerateVertexData (meshSize, meshVertexResolution, out vertices, out uv, extendBorders);
			
		int[] triangles = GenerateTriangles( extendBorders ? meshVertexResolution + 2 : meshVertexResolution);
			
		return GenerateMesh(vertices, uv, triangles);
	}


	static private void GenerateVertexData(float meshSize, int meshVertexResolution, out Vector3[] vertices, out Vector2[] uv, bool extendBorders)
	{
		float DISTANCE_BETWEEN_VERTICES = meshSize / (float)(meshVertexResolution - 1.0f) ;
		float DISTANCE_BETWEEN_UV = 1.0f / (float)(meshVertexResolution - 1.0f);

		// Generate vertices and UV.
		if (extendBorders) {
			int N_VERTICES = (meshVertexResolution + 2) * (meshVertexResolution + 2);
			vertices = new Vector3[N_VERTICES];
			uv = new Vector2[N_VERTICES];

			for (int row=-1; row<=meshVertexResolution; row++) {
				for (int column=-1; column<=meshVertexResolution; column++) {
					int VERTEX_INDEX = (row + 1) * (meshVertexResolution + 2) + (column + 1);

					vertices[VERTEX_INDEX].x = Mathf.Clamp (-meshSize / 2.0f + column * DISTANCE_BETWEEN_VERTICES, -meshSize / 2.0f, meshSize / 2.0f);
					vertices[VERTEX_INDEX].y = 0.0f;
					vertices[VERTEX_INDEX].z = Mathf.Clamp (meshSize / 2.0f - row * DISTANCE_BETWEEN_VERTICES, -meshSize / 2.0f, meshSize / 2.0f);

					uv[VERTEX_INDEX].x = Mathf.Clamp (DISTANCE_BETWEEN_UV * column, 0.0f, 1.0f);
					uv[VERTEX_INDEX].y = Mathf.Clamp (1.0f - Mathf.Max( DISTANCE_BETWEEN_UV * row, 0.0f ), 0.0f, 1.0f);
				}
			}
		} else {
			int N_VERTICES = meshVertexResolution * meshVertexResolution;
			vertices = new Vector3[N_VERTICES];
			uv = new Vector2[N_VERTICES];

			for (int row=0; row<meshVertexResolution; row++) {
				for (int column=0; column<meshVertexResolution; column++) {
					int VERTEX_INDEX = row * meshVertexResolution + column;

					vertices[VERTEX_INDEX].x = -meshSize / 2.0f + column * DISTANCE_BETWEEN_VERTICES;
					vertices[VERTEX_INDEX].y = 0.0f;
					vertices[VERTEX_INDEX].z = meshSize / 2.0f - row * DISTANCE_BETWEEN_VERTICES;

					uv[VERTEX_INDEX].x = DISTANCE_BETWEEN_UV * column;
					uv[VERTEX_INDEX].y = 1.0f - DISTANCE_BETWEEN_UV * row;
				}
			}
		}
	}


	static private int[] GenerateTriangles(int meshVertexResolution)
	{
		int N_TRIANGLES = 2 * (meshVertexResolution - 1) * (meshVertexResolution - 1);
		int[] triangles = new int[N_TRIANGLES * 3];
		int triangleIndex = 0;
		for (int row=0; row<meshVertexResolution - 1; row++) {
			for (int column=0; column<meshVertexResolution - 1; column++) {
				triangles[triangleIndex] = GetVertexIndex( row, column, meshVertexResolution );
				triangles[triangleIndex + 1] = GetVertexIndex( row, column+1, meshVertexResolution ); 
				triangles[triangleIndex + 2] = GetVertexIndex( row+1, column, meshVertexResolution ); 

				triangles[triangleIndex + 3] = GetVertexIndex( row, column+1, meshVertexResolution );
				triangles[triangleIndex + 4] = GetVertexIndex( row+1, column+1, meshVertexResolution ); 
				triangles[triangleIndex + 5] = GetVertexIndex( row+1, column, meshVertexResolution ); 

				triangleIndex += 6;
			}
		}
		return triangles;
	}


	static private Mesh GenerateMesh(Vector3[] vertices, Vector2[] uv, int[] triangles)
	{
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;
		mesh.RecalculateNormals ();
		mesh.RecalculateBounds ();
		return mesh;
	}



	static private int GetVertexIndex( int row, int column, int verticesPerRow )
	{
		return row * verticesPerRow + column;
	}
}
