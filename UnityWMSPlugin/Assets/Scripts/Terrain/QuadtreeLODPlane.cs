using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadtreeLODPlane : MonoBehaviour {
	private QuadtreeLODNode rootNode = null;
	public int vertexResolution = 20;
	public OnlineTexturesRequester texturesRequester; 


	public void Start()
	{
		Reset ();
	}


	public void Reset()
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
			
		if (GetComponent<WMSComponent> () != null) {
			texturesRequester = this.GetComponent<WMSComponent> ();

			if (GetComponent<BingComponent> () != null) {
				throw new UnityException ("Terrain can't have both WMSComponent and BingComponent componets!");
			}
		} else if (GetComponent<BingComponent> () != null) {
			texturesRequester = this.GetComponent<BingComponent> ();
		} else {
			throw new MissingComponentException ("Terrain must have a WMSComponent or BingComponent");
		}
			
		rootNode = new QuadtreeLODNode( 
		                               mapSize, 
									   vertexResolution,
		                               transform, 
		                               this.GetComponent<Material>(),
									   texturesRequester
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
