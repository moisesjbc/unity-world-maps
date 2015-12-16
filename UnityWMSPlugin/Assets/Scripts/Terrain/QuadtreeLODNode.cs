//#define PAINT_QUADS
//#define PRINT_CHILDREN_BOUNDARIES

using UnityEngine;
using System.Collections;

public class QuadtreeLODNode {
	private GameObject gameObject_;
	private Transform transform_;
	private Mesh mesh_;
	private int meshVertexResolution_;
	private Material material_;
	private bool visible_;
	Vector2 bottomLeftCoordinates_;
	Vector2 topRightCoordinates_;
	
	QuadtreeLODNode[] children_;

	private int depth_ = 0;
	const int MAX_DEPTH = 7;

	string textureRequestId;
	bool textureLoaded = false;

	string heightMapRequestId;
	bool heightMapLoaded = true;

	float metersPerUnit = 0.0f;

	static GameObject emptyGameObject = new GameObject();
	private static HeightMapsManager heightMapsManager = new HeightMapsManager();
	private static MapTexturesManager mapTexturesManager = new MapTexturesManager();

	private string serverURL;
	private string layersQuery;


	public QuadtreeLODNode( 
	                       float meshSize, 
	                       int meshVertexResolution, 
	                       Vector2 bottomLeftCoordinates,
	                       Vector2 topLeftCoordinates,
	                       Transform transform, 
	                       Material material,
	                       string serverURL,
	                       string layersQuery )
	{		
		gameObject_ = GameObject.Instantiate( emptyGameObject );
		gameObject_.AddComponent<MeshRenderer>();
		gameObject_.tag = "MapSector";

		// Create the root mesh.
		mesh_ = MeshFactory.CreateMesh ( meshSize, meshVertexResolution );
		gameObject_.AddComponent<MeshFilter> ().mesh = mesh_;
		meshVertexResolution_ = meshVertexResolution;

		// Make this mesh transform relative to parent.
		transform_ = gameObject_.transform;
		transform_.parent = transform;
		
		// Copy given material.
		material_ = new Material (Shader.Find ("Standard"));
		gameObject_.GetComponent<Renderer>().material = material_;

		visible_ = true;

		children_ = new QuadtreeLODNode[]{ null, null, null, null };
		
		bottomLeftCoordinates_ = bottomLeftCoordinates;
		topRightCoordinates_ = topLeftCoordinates;

		metersPerUnit = (topRightCoordinates_.x - bottomLeftCoordinates_.x) / gameObject_.GetComponent<MeshRenderer> ().bounds.size.x;
		Debug.Log ("metersPerUnit: " + metersPerUnit);
		
		textureRequestId = mapTexturesManager.RequestTexture (serverURL, layersQuery, bottomLeftCoordinates_, topRightCoordinates_);
		heightMapRequestId = heightMapsManager.RequestHeightMap ( bottomLeftCoordinates_, topRightCoordinates_, meshVertexResolution_ );
	
		this.serverURL = serverURL;
		this.layersQuery = layersQuery;
	}


	public QuadtreeLODNode( QuadtreeLODNode parent, Color color, Vector3 localPosition, Vector2 bottomLeftCoordinates, Vector2 topRightCoordinates, string serverURL, string layersQuery )
	{
		gameObject_ = GameObject.Instantiate( emptyGameObject );
		gameObject_.AddComponent<MeshRenderer>();

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
		transform_ = gameObject_.transform;
		transform_.parent = parent.gameObject_.transform;

		transform_.localScale = new Vector3( 0.5f, 1.0f, 0.5f );
		transform_.localPosition = localPosition;
		
		material_ = new Material (Shader.Find ("Standard"));
		gameObject_.GetComponent<Renderer>().material = material_;
#if PAINT_QUADS
		material_.color = color;
#endif
		depth_ = parent.depth_ + 1;

		visible_ = false;
		gameObject_.GetComponent<MeshRenderer> ().enabled = false;

		bottomLeftCoordinates_ = bottomLeftCoordinates;
		topRightCoordinates_ = topRightCoordinates;
		
		children_ = new QuadtreeLODNode[]{ null, null, null, null };

		float mapSize = topRightCoordinates_.x - bottomLeftCoordinates_.x;
		Vector2 mapSizeVector = new Vector2( mapSize, mapSize );

		metersPerUnit = (topRightCoordinates_.x - bottomLeftCoordinates_.x) / gameObject_.GetComponent<MeshRenderer> ().bounds.size.x;

		textureRequestId = mapTexturesManager.RequestTexture (serverURL, layersQuery, bottomLeftCoordinates_, topRightCoordinates_);
		heightMapRequestId = heightMapsManager.RequestHeightMap ( bottomLeftCoordinates_ - mapSizeVector, topRightCoordinates_ + mapSizeVector, meshVertexResolution_ + (meshVertexResolution_ - 1) * 2 );
	
		this.serverURL = serverURL;
		this.layersQuery = layersQuery;
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
			Vector3 meshSize = Vector3.Scale (mesh_.bounds.size, transform_.lossyScale);

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
			Texture2D texture = mapTexturesManager.GetTexture (textureRequestId);
			if (texture != null) {
				textureLoaded = true;
				material_.mainTexture = texture;
				material_.mainTexture.wrapMode = TextureWrapMode.Clamp;
			}
		}

		if (!heightMapLoaded) {
			float[,] heightMatrix = heightMapsManager.GetHeightMatrix( heightMapRequestId );
			if( heightMatrix != null ){
				heightMapLoaded = true;
				if( depth_ == 0 ){
					SetHeightsMap( heightMatrix );
				}else{
					SetHeightsMap( GetSubMatrix( heightMatrix, meshVertexResolution_ - 1, meshVertexResolution_ - 1, 2 * meshVertexResolution_ - 1, 2 * meshVertexResolution_ - 1 ) );
				}
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
			1.0f / transform_.lossyScale.x,
			1.0f / transform_.lossyScale.y,
			1.0f / transform_.lossyScale.z
			);
		Vector3[] childLocalPosition = new Vector3[]
		{
			Vector3.Scale ( new Vector3( -meshSize.x/4,0,meshSize.z/4 ), S ),
			Vector3.Scale ( new Vector3( -meshSize.x/4,0,-meshSize.z/4 ), S ),
			Vector3.Scale ( new Vector3( meshSize.x/4,0,meshSize.z/4), S ),
			Vector3.Scale ( new Vector3( meshSize.x/4,0,-meshSize.z/4), S )
		};
		
		float x0 = bottomLeftCoordinates_.x;
		float y0 = bottomLeftCoordinates_.y;
		float x1 = topRightCoordinates_.x;
		float y1 = topRightCoordinates_.y;
		
		float cx = (x0 + x1)/2.0f;
		float cy = (y0 + y1)/2.0f;
		
		Vector2[] childrenBottomLeftCoordinates = new Vector2[]
		{
			new Vector2( x0, cy ),
			new Vector2( x0, y0 ),
			new Vector2( cx, cy ),
			new Vector2( cx, y0 )
		};
		
		Vector2[] childrenTopLeftCoordinates = new Vector2[]
		{
			new Vector2( cx, y1 ),
			new Vector2( cx, cy ),
			new Vector2( x1, y1 ),
			new Vector2( x1, cy )
		};

		Color[] childrenColors = new Color[]
		{
			new Color( 1.0f, 0.0f, 0.0f, 1.0f ),
			new Color( 0.0f, 1.0f, 0.0f, 1.0f ),
			new Color( 0.0f, 0.0f, 1.0f, 1.0f ),
			new Color( 1.0f, 1.0f, 0.0f, 1.0f )
		};
		
		for( int i=0; i<4; i++ ){
			children_[i] = new QuadtreeLODNode( this, childrenColors[i], childLocalPosition[i], childrenBottomLeftCoordinates[i], childrenTopLeftCoordinates[i], serverURL, layersQuery ); 
		}
	}


	private bool AreChildrenLoaded(){
		if (children_ [0] != null) {
			for (int i = 0; i < 4; i++) {
				if (children_ [i].textureLoaded == false || children_ [i].heightMapLoaded == false) {
					return false;
				}
			}
			return true;
		} else {
			return false;
		}
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
		Vector3[] vertices = mesh_.vertices;
		
		int N_ROWS = heights.GetLength(0);
		for (int row=0; row<N_ROWS; row++) {
			// FIXME: This is forcing N_COLUMS = N_ROWS.
			int N_COLUMNS = N_ROWS;
			for (int column=0; column<N_COLUMNS; column++) {
				int VERTEX_INDEX = row * N_COLUMNS + column;
				vertices[VERTEX_INDEX].y = heights[row,column] / (metersPerUnit / 2.0f);
			}
		}
		
		mesh_.vertices = vertices;
		mesh_.RecalculateBounds ();
		mesh_.RecalculateNormals ();

		// Add a collider to the node.
		gameObject_.AddComponent<MeshCollider>();
	}


	public bool ObserverOnSector( Vector3 observer, out float observerHeight )
	{
		if (visible_) {
			// Compute if the XZ rect of the map sector contains the observer.
			Vector3 sectorSize = Vector3.Scale (mesh_.bounds.size, transform_.lossyScale);

			Rect sectorRect = new Rect (
				transform_.position.x - sectorSize.x / 2.0f,
				transform_.position.z - sectorSize.z / 2.0f,
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
