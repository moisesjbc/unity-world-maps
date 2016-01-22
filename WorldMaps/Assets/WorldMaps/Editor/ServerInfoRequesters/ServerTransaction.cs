using UnityEngine;
using System;
using System.Collections;


public enum RequestStatus
{
	DOWNLOADING,
	OK,
	ERROR
}


class ServerTransaction <ResponseType>
	where ResponseType : class
{
	public delegate ResponseType ParsingFunction(string responseText);

	public WWW request = null;
	public ResponseType response = null;
	public string errorLog = null;


	public ServerTransaction(string url)
	{
		request = new WWW (url);
	}


	public RequestStatus Update( ParsingFunction ParseResponse )
	{
		RequestStatus requestStatus = GetRequestStatus ();

		if (requestStatus == RequestStatus.DOWNLOADING) {
			if (request.isDone) {
				if (request.error == null) {
					try{
						response = ParseResponse (request.text);
						return RequestStatus.OK;
					}catch( Exception e ){
						// Parsing error
						this.errorLog = "Couldn't parse response from server: " + e.Message;
						Debug.LogError (this.errorLog);
						return RequestStatus.ERROR;
					}
				} else {
					// Connection error
					this.errorLog = "Couldn't get info from server: " + request.error;
					Debug.LogError (this.errorLog);
					return RequestStatus.ERROR;
				}
			}
			return RequestStatus.DOWNLOADING;
		} else {
			// Status other than "downloading" remain the same when updating.
			return requestStatus;
		}
	}


	public RequestStatus GetRequestStatus()
	{
		if (response != null) {
			return RequestStatus.OK;
		} else if (errorLog != null) {
			return RequestStatus.ERROR;
		} else {
			return RequestStatus.DOWNLOADING;
		}
	}
}