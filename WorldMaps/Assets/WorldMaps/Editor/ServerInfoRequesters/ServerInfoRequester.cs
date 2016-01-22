using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



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
		Debug.Log ("Update() - " + requestStatus);

		if (requestStatus == RequestStatus.DOWNLOADING) {
			if (request.isDone) {
				if (request.error == null) {
					try{
						Debug.Log("response: " + request.text);
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
	

public enum RequestStatus
{
	DOWNLOADING,
	OK,
	ERROR
}


public abstract class ServerInfoRequester <ResponseType>
	where ResponseType : class
{
	Dictionary<string, ServerTransaction<ResponseType>> transactions = 
		new Dictionary<string, ServerTransaction<ResponseType>>();


	public string RequestWMSInfo( string serverURL )
	{
		string requestID = GenerateRequestID (serverURL);
		if (!transactions.ContainsKey (requestID) || transactions[requestID].errorLog != null){
			transactions [requestID] = new ServerTransaction<ResponseType> (serverURL + BuildQueryString());
		}
		return requestID;
	}


	public RequestStatus Update( string requestID )
	{
		return transactions [requestID].Update (ParseResponse);
	}


	public RequestStatus GetRequestStatus (string requestID)
	{
		return transactions [requestID].GetRequestStatus();
	}


	public ResponseType GetResponse( string requestID )
	{
		return transactions [requestID].response;
	}
		

	protected string GenerateRequestID( string serverURL )
	{
		return serverURL;
	}


	protected abstract ResponseType ParseResponse( string responseText );
	protected abstract string BuildQueryString();
}
