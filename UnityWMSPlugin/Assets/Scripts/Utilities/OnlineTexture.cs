using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public abstract class OnlineTexture : MonoBehaviour {
	public bool textureLoaded = false;

	public void RequestTexture( string nodeID )
	{
		StartCoroutine (RequestTexture_ (nodeID));
	}


	public void RequestTexturePreview ()
	{
		RequestTexture ("0");
	}


	public IEnumerator RequestTexture_( string nodeID )
	{
		textureLoaded = false;
		string url = GenerateRequestURL (nodeID);
		WWW request = new WWW (url);

		Debug.Log ("RequestTexture_ - 1");
		yield return request;
		Debug.Log ("RequestTexture_ - 2");

		if (Application.isPlaying) {
			var tempMaterial = new Material (GetComponent<MeshRenderer> ().material);
			tempMaterial.mainTexture = request.texture;
			tempMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
			GetComponent<MeshRenderer> ().material = tempMaterial;
		} else {
			var tempMaterial = new Material (GetComponent<MeshRenderer> ().sharedMaterial);
			tempMaterial.mainTexture = request.texture;
			tempMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
			GetComponent<MeshRenderer> ().sharedMaterial = tempMaterial;
		}
	}


	protected abstract string GenerateRequestURL (string nodeID);
}
