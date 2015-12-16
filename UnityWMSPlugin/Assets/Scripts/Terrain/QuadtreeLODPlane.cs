using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadtreeLODPlane : MonoBehaviour {
	private QuadtreeLODNode rootNode = null;

	public string serverURL = "http://idecan1.grafcan.com/ServicioWMS/OrtoExpress";
	public string[] layers = new string[]{};
	public Vector2 bottomLeftCoordinates = new Vector2 ( 416000,3067000 );
	public Vector2 topRightCoordinates = new Vector2 ( 466000,3117000 );


	public void Start()
	{
		Reset (bottomLeftCoordinates, topRightCoordinates);
	}


	public void Reset( Vector2 bottomLeftCoordinates, Vector3 topRightCoordinates )
	{
		Vector3 meshSize = GetComponent<MeshRenderer> ().bounds.size;
		if (meshSize.x != meshSize.z) {
			Debug.LogWarning ("LOD plane must be square (currently: " + 
			                  meshSize.x + 
			                  "x" +
			                  meshSize.y +
			                  ")");
		}

		float mapSize = Mathf.Max ( meshSize.x, meshSize.z );
		
		if (rootNode != null) {
			GameObject[] mapSectors = GameObject.FindGameObjectsWithTag ("MapSector");
			for( int i=0; i<mapSectors.Length; i++ ){
				Destroy (mapSectors[i]);
			}
		}

		rootNode = new QuadtreeLODNode( 
		                               mapSize, 
		                               20, 
		                               bottomLeftCoordinates,
		                               topRightCoordinates,
		                               transform, 
		                               this.GetComponent<Material>(),
		                               serverURL
		                               );
		GetComponent<MeshRenderer> ().enabled = false;
	}


	void Update () {
		if (rootNode != null) {
			rootNode.Update ( false );
		}
	}


	public float GetHeight( Vector3 observer )
	{
		float height;
		if (rootNode != null && rootNode.ObserverOnSector (observer, out height)) {
			return height;
		} else {
			return observer.y;
		}
	}
}
