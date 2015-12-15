using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MapTexturesManager : OnlineResourcesManager
{
	public string RequestTexture( 
	                            string serverURL,
	                            Vector2 bottomLeftCoordinates, 
	                            Vector2 topRightCoordinates
	                             ){
		string newId = GenerateID (bottomLeftCoordinates, topRightCoordinates );

#if CACHE_RESOURCES
		if ( !ResourceNotRequested( newId ) ) {
			Debug.LogFormat ("Requesting texture with id [{0}] - Cached!", newId);
			return newId
		}
#endif
		string fixedUrl = serverURL + "?SERVICE=WMS&LAYERS=ortoexpress&REQUEST=GetMap&VERSION=1.1.0&FORMAT=image/jpeg&SRS=EPSG:32628&WIDTH=128&HEIGHT=128&REFERER=CAPAWARE";
		string bboxUrlQuery = 
			"&BBOX=" + bottomLeftCoordinates.x + "," +
				bottomLeftCoordinates.y + "," +
				topRightCoordinates.x + "," +
				topRightCoordinates.y;
		string url = fixedUrl + bboxUrlQuery;
	
		requests_ [newId] = new WWW (url);

		Debug.LogFormat ("Requesting texture with id [{0}] - NOT cached", newId);
		Debug.Log ("Texture URL - " + url);

		return newId;
	}


	protected string GenerateID( Vector2 bottomLeftCoordinates, Vector2 topRightCoordinates )
	{
		return 
			"texture-" + 
			bottomLeftCoordinates.x + "-" + 
			bottomLeftCoordinates.y + "-" +
			topRightCoordinates.x + "-" + 
			topRightCoordinates.y +
			".jpg";
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
}
