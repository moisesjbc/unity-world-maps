using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadtreeLODPlane : MonoBehaviour {
	private QuadtreeLODNode rootNode = null;
	public int vertexResolution = 20;


	public void Start()
	{
		Debug.Log ("<serverURL>: " + GetComponent<WMSComponent>().serverURL);
		Reset (GetComponent<WMSComponent>().bottomLeftCoordinates, GetComponent<WMSComponent>().topRightCoordinates);
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
									   vertexResolution,
									   GetComponent<WMSComponent>().bottomLeftCoordinates,
									   GetComponent<WMSComponent>().topRightCoordinates,
		                               transform, 
		                               this.GetComponent<Material>(),
									   GetComponent<WMSComponent>()
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
