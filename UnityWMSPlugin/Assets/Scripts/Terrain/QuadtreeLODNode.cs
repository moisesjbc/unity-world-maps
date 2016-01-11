//#define PAINT_QUADS
//#define PRINT_CHILDREN_BOUNDARIES

using UnityEngine;
using System.Collections;

public class QuadtreeLODNode {
	private GameObject gameObject_;
	private Mesh mesh_;
	private int meshVertexResolution_;
	private Material material_;
	private bool visible_;

	private string nodeID;
	
	QuadtreeLODNode[] children_;

	private int depth_ = 0;
	const int MAX_DEPTH = 7;

	string textureRequestId;
	bool textureLoaded = false;

	float metersPerUnit = 0.0f;

	static GameObject emptyGameObject = new GameObject();
	OnlineTexturesRequester onlineTexturesRequester;


	public QuadtreeLODNode( 
	                       float meshSize, 
	                       int meshVertexResolution,  
	                       Transform transform, 
	                       Material material,
					       OnlineTexturesRequester onlineTexturesRequester )
	{		
		nodeID = "0";
		this.onlineTexturesRequester = onlineTexturesRequester;

		gameObject_ = GameObject.Instantiate( emptyGameObject );
		gameObject_.AddComponent<MeshRenderer>();
		gameObject_.tag = "MapSector";

		// Create the root mesh.
		mesh_ = MeshFactory.CreateMesh ( meshSize, meshVertexResolution );
		gameObject_.AddComponent<MeshFilter> ().mesh = mesh_;
		meshVertexResolution_ = meshVertexResolution;
				
		// Copy given material.
		material_ = new Material (Shader.Find ("Standard"));
		gameObject_.GetComponent<Renderer>().material = material_;

		visible_ = true;

		children_ = new QuadtreeLODNode[]{ null, null, null, null };

		// FIXME: Compute this instead of giving a arbitrary value.
		metersPerUnit = 1000;
		Debug.Log ("metersPerUnit: " + metersPerUnit);
		
		textureRequestId = onlineTexturesRequester.RequestTexture (nodeID);
	}


	public QuadtreeLODNode( QuadtreeLODNode parent, string nodeID, Color color, Vector3 localPosition, OnlineTexturesRequester onlineTexturesRequester )
	{
		gameObject_ = GameObject.Instantiate( emptyGameObject );
		gameObject_.AddComponent<MeshRenderer>();

		this.nodeID = nodeID;
		this.onlineTexturesRequester = onlineTexturesRequester;

		// Copy given mesh.
		mesh_ = new Mesh ();
		mesh_.vertices = parent.mesh_.vertices;
		mesh_.triangles = parent.mesh_.triangles;
		mesh_.uv = parent.mesh_.uv;
		mesh_.RecalculateNormals ();
		mesh_.RecalculateBounds ();
		meshVertexResolution_ = parent.meshVertexResolution_;
		gameObject_.AddComponent<MeshFilter> ().mesh = mesh_;
		gameObject_.tag = "MapSector";

		// Make this mesh transform relative to parent.
		gameObject_.transform.parent = parent.gameObject_.transform;

		gameObject_.transform.localScale = new Vector3( 0.5f, 1.0f, 0.5f );
		gameObject_.transform.localPosition = localPosition;
		
		material_ = new Material (Shader.Find ("Standard"));
		gameObject_.GetComponent<Renderer>().material = material_;
#if PAINT_QUADS
		material_.color = color;
#endif
		depth_ = parent.depth_ + 1;

		visible_ = false;
		gameObject_.GetComponent<MeshRenderer> ().enabled = false;
				
		children_ = new QuadtreeLODNode[]{ null, null, null, null };

		// FIXME: Compute this instead of giving a arbitrary value.
		metersPerUnit = 1000;

		textureRequestId = onlineTexturesRequester.RequestTexture (nodeID);
	}


	private void FlipUV()
	{
		Vector2[] uv = mesh_.uv;
		for (int i=0; i<uv.Length; i++) {
			uv[i].x = 1.0f - uv[i].x;
			uv[i].y = 1.0f - uv[i].y;
		}
		mesh_.uv = uv;
	}


	public void SetVisible( bool visible )
	{
		// Set node visibility.
		visible_ = visible;
		gameObject_.GetComponent<MeshRenderer> ().enabled = visible;

		// Enable or disable collider according to new visibility value.
		Collider collider = gameObject_.GetComponent<Collider>();
		if ( collider != null ) {
			collider.enabled = visible;
		}

		// No matter which visibility is applied to this node, children
		// visibility must be set to false.
		if (children_[0] != null) {
			for( int i = 0; i < children_.Length; i++ ){
				children_[i].SetVisible (false);
			}
		}
	}


	public void Update( bool existsVisibleAncestor )
	{
		if (visible_ || AreChildrenLoaded()) {
			DistanceTestResult distanceTestResult = DoDistanceTest();
			Vector3 meshSize = Vector3.Scale (mesh_.bounds.size, gameObject_.transform.lossyScale);

			// Subdivide the plane if camera is closer than a threshold.
			if (visible_ && distanceTestResult == DistanceTestResult.SUBDIVIDE ) {
				// Create children if they don't exist.
				if (depth_ < MAX_DEPTH && children_ [0] == null) {
					CreateChildren (meshSize);
				}
			
				// Make this node invisible and children visible.
				if (AreChildrenLoaded ()) {
					SetVisible (false);
					for (int i = 0; i < children_.Length; i++) {
						children_ [i].SetVisible (true);
					}
				}
			}else if ( !existsVisibleAncestor && !visible_ && AreChildrenLoaded () && distanceTestResult == DistanceTestResult.JOIN ) {
				SetVisible (true);
				for (int i = 0; i < children_.Length; i++) {
					children_ [i].SetVisible (false);
				}
			}
		}

		// Update children.
		if (children_ [0] != null) {
			for (int i=0; i<4; i++) {
				children_ [i].Update ( existsVisibleAncestor | visible_ );
			}
		}

		if (!textureLoaded) {
			Texture2D texture = onlineTexturesRequester.GetTexture (textureRequestId);
			if (texture != null) {
				textureLoaded = true;
				material_.mainTexture = texture;
				material_.mainTexture.wrapMode = TextureWrapMode.Clamp;
			}
		}
	}

	enum DistanceTestResult 
	{
		DO_NOTHING,
		SUBDIVIDE,
		JOIN
	}

	private DistanceTestResult DoDistanceTest()
	{
		const float THRESHOLD_FACTOR = 2.5f;

		Vector3 cameraPos = Camera.main.transform.position;
		float distanceCameraBorder = Vector3.Distance (cameraPos, gameObject_.GetComponent<MeshRenderer> ().bounds.ClosestPoint (cameraPos));
		Vector3 boundsSize = gameObject_.GetComponent<MeshRenderer> ().bounds.size;
		float radius = (boundsSize.x + boundsSize.y + boundsSize.z) / 3.0f;

		if (distanceCameraBorder < THRESHOLD_FACTOR * radius) {
			return DistanceTestResult.SUBDIVIDE;
		} else if (distanceCameraBorder >= THRESHOLD_FACTOR * radius) {
			return DistanceTestResult.JOIN;
		}

		return DistanceTestResult.DO_NOTHING;
	}


	private void CreateChildren( Vector3 meshSize )
	{
		Vector3 S = new Vector3(
			1.0f / gameObject_.transform.lossyScale.x,
			1.0f / gameObject_.transform.lossyScale.y,
			1.0f / gameObject_.transform.lossyScale.z
			);


		Vector3[] childLocalPosition = new Vector3[]
		{
			Vector3.Scale ( new Vector3( -meshSize.x/4,0,meshSize.z/4 ), S ),
			Vector3.Scale ( new Vector3( -meshSize.x/4,0,-meshSize.z/4 ), S ),
			Vector3.Scale ( new Vector3( meshSize.x/4,0,meshSize.z/4), S ),
			Vector3.Scale ( new Vector3( meshSize.x/4,0,-meshSize.z/4), S )
		};


		Color[] childrenColors = new Color[]
		{
			new Color( 1.0f, 0.0f, 0.0f, 1.0f ),
			new Color( 0.0f, 1.0f, 0.0f, 1.0f ),
			new Color( 0.0f, 0.0f, 1.0f, 1.0f ),
			new Color( 1.0f, 1.0f, 0.0f, 1.0f )
		};


		string[] childrenIDs = new string[] 
		{
			nodeID + "0",
			nodeID + "1",
			nodeID + "2",
			nodeID + "3"
		};
		
		for( int i=0; i<4; i++ ){
			children_[i] = new QuadtreeLODNode( this, childrenIDs[i], childrenColors[i], childLocalPosition[i], onlineTexturesRequester ); 
		}
	}


	private bool AreChildrenLoaded(){
		if (children_ [0] != null) {
			for (int i = 0; i < 4; i++) {
				if (children_ [i].textureLoaded == false) {
					return false;
				}
			}
			return true;
		} else {
			return false;
		}
	}


	public bool ObserverOnSector( Vector3 observer, out float observerHeight )
	{
		if (visible_) {
			// Compute if the XZ rect of the map sector contains the observer.
			Vector3 sectorSize = Vector3.Scale (mesh_.bounds.size, gameObject_.transform.lossyScale);

			Rect sectorRect = new Rect (
				gameObject_.transform.position.x - sectorSize.x / 2.0f,
				gameObject_.transform.position.z - sectorSize.z / 2.0f,
				sectorSize.x,
				sectorSize.z
			);
			//Debug.Log ( "sectorRect: " + sectorRect );

			if (sectorRect.Contains (new Vector2 (observer.x, observer.z))) {
				MeshCollider mapCollider = gameObject_.GetComponent<MeshCollider> ();
				if (mapCollider != null) {
					Ray ray = new Ray (observer, Vector3.down);
					RaycastHit hit;
				
					if (mapCollider.Raycast (ray, out hit, 100.0f)) {
						observerHeight = observer.y - hit.point.y;
						return true;
					}
				}
			}
		} else {
			if (AreChildrenLoaded ()) {
				for (int i=0; i<children_.Length; i++) {
					float heightOnChildren;
					if (children_ [i].ObserverOnSector (observer, out heightOnChildren)) {
						observerHeight = heightOnChildren;
						return true;
					}
				}
			}
		}

		observerHeight = -1.0f;
		return false;
	}

}
