using UnityEngine;
using System.Collections;

public abstract class OnlineTexturesRequester : OnlineResourcesManager {
	
	private string lastRequestFixedUrl = "";

	public string RequestTexture( string nodeID )
	{
		string requestID = GenerateRequestID(nodeID);

		#if CACHE_RESOURCES
		if ( !ResourceNotRequested( newId ) ) {
			Debug.LogFormat ("Requesting texture with id [{0}] - Cached!", newId);
			return newId
		}
		#endif

		string url = GenerateRequestURL (nodeID);
		lastRequestFixedUrl = CurrentFixedUrl ();
		requests_ [requestID] = new WWW (url);

		return requestID;
	}


	public Texture2D GetTexture( string id )
	{
		#if CACHE_RESOURCES
		byte[] jpgTextureData = null;

		// Check if we have a finished request with the same ID
		// and save the result to a file.
		if ( requests_.ContainsKey(id) && requests_ [id].isDone ){
		jpgTextureData = requests_[id].texture.EncodeToJPG();
		File.WriteAllBytes( FilePath( id ), jpgTextureData );
		}

		// If the request hasn't finished, the file won't exist
		// yet.
		if( !File.Exists ( FilePath( id ) ) ){
		return null;
		}

		if( jpgTextureData == null ){
		jpgTextureData = File.ReadAllBytes( FilePath( id ) );
		}

		return new Texture2D( jpgTextureData );
		#else
		if ( requests_.ContainsKey(id) && requests_ [id].isDone ){
			return requests_ [id].texture;
		}else{
			return null;
		}
		#endif
	}


	public bool FixedUrlChangedSinceLastRequest ()
	{
		return lastRequestFixedUrl != CurrentFixedUrl();
	}


	protected abstract string GenerateRequestID (string nodeID);
	protected abstract string GenerateRequestURL (string nodeID);
	public abstract string CurrentFixedUrl ();
}
