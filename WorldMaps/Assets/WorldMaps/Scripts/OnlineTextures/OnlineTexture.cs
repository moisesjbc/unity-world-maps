using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
public abstract class OnlineTexture : MonoBehaviour {
	public bool textureLoaded = false;
	protected WWW request_;


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


	void OnDisable(){
		EditorApplication.update -= Update;
	}
		

	public void Update()
	{
		if (textureLoaded == false && request_ != null && request_.isDone) {
			string errorMessage = "";

			// Texture is always assigned because even when errors arise, request_.texture
			// returns a white texture with a red question mark, valid as a hint for the user 
			// about the error.
			GetComponent<MeshRenderer> ().material.mainTexture = request_.texture;
			GetComponent<MeshRenderer> ().material.mainTexture.wrapMode = TextureWrapMode.Clamp;

			if (ValidateDownloadedTexture (out errorMessage)) {
				textureLoaded = true;
			} else {
				string requestedURL = request_.url;
				request_ = null;
				Debug.LogError ("Errors when downloading texture [" + requestedURL + "]:\n" + errorMessage);
			}
		}
	}


	protected abstract string GenerateRequestURL (string nodeID);
	public void CopyTo(OnlineTexture copy)
	{
		copy.request_ = request_;
		// This forces inherited component to reload the texture.
		copy.textureLoaded = false;

		InnerCopyTo (copy);
	}

	protected abstract void InnerCopyTo (OnlineTexture copy);


	public virtual bool ValidateDownloadedTexture( out string errorMessage )
	{
		if (request_.error != null) {
			errorMessage = request_.error;
			return false;
		} else {
			errorMessage = "";
			return true;
		}
	}
}
