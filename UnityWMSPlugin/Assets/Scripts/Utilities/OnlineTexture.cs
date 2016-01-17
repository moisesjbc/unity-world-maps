using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
public abstract class OnlineTexture : MonoBehaviour {
	public bool textureLoaded = false;
	private WWW request_;


	public void Start()
	{
		// When in edit mode, start downloading a texture preview.
		if (!Application.isPlaying) {
			RequestTexturePreview ();
		}
	}


	public void RequestTexture( string nodeID )
	{
		textureLoaded = false;
		string url = GenerateRequestURL (nodeID);
		request_ = new WWW (url);
	}


	public void RequestTexturePreview ()
	{
		RequestTexture ("0");
	}


	// Make this update with editor.
	void OnEnable(){
		EditorApplication.update += Update;
	}
		

	public void Update()
	{
		if (textureLoaded == false && request_ != null && request_.isDone) {
			if (Application.isPlaying) {
				var tempMaterial = new Material (GetComponent<MeshRenderer> ().material);
				tempMaterial.mainTexture = request_.texture;
				tempMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
				GetComponent<MeshRenderer> ().material = tempMaterial;
			} else {
				var tempMaterial = new Material (GetComponent<MeshRenderer> ().sharedMaterial);
				tempMaterial.mainTexture = request_.texture;
				tempMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
				GetComponent<MeshRenderer> ().sharedMaterial = tempMaterial;
			}
		}
	}


	protected abstract string GenerateRequestURL (string nodeID);
}
